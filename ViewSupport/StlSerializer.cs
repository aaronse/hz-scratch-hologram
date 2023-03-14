using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Primitives;
using ScratchUtility;

namespace ViewSupport
{
    /// <summary>
    /// Yet another STL Serializer/DeSerializer...
    /// 
    /// 
    /// Related work:
    /// - STL format https://en.wikipedia.org/wiki/STL_%28file_format%29#Binary_STL
    /// - C# STL binary/text parser https://stackoverflow.com/questions/68568214/c-sharp-how-to-parse-an-stl-file-current-function-does-not-link-vertices-into-f
    /// - C# STL binary/text parser https://github.com/QuantumConcepts/STLdotNET
    ///   - Parsing code mixed with datastructure definition
    /// - C++ https://github.com/TinyTinni/FileSTL/blob/master/stlio.hpp
    ///   - STL stream based, makes for a cryptic read...
    /// - C# STL binary/text parser https://github.com/batu92k/STL-Viewer/blob/master/STL-Viewer/STL_Tools/STLReader.cs
    ///   - Nested, not refactored
    ///
    /// ... and many many more...
    /// </summary>
    public class StlSerializer
    {
        private static char[] s_lineDelimiters = new char[] { ' ' };

        private StlSerializer()
        {

        }

        public static IndexedFaceSet Deserialize(string filePath, bool autoCenter = true)
        {
            DateTime start = DateTime.UtcNow;

            StlSerializer instance = new StlSerializer();
            IndexedFaceSet ifs = null;

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Unable to find .STL file, path: {filePath}");
            }

            // Binary or text?
            bool isBinary = true;
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    string header = Encoding.UTF8.GetString(br.ReadBytes(6));
                    if ("solid ".Equals(header))
                    {
                        isBinary = false;
                    }
                }
            }
            
            if (isBinary)
            {
                ifs = instance.DeserializeBinary(filePath, autoCenter);
            }
            else
            {
                ifs = instance.DeserializeAscii(filePath, autoCenter);
            }
            
            Debug.WriteLine($"StlDeserialize, durMs={(int)DateTime.UtcNow.Subtract(start).TotalMilliseconds}, isBinary={isBinary}, vertices={ifs.Vertices.Count}, edges={ifs.Edges.Count}");

            return ifs;
        }


        // Related:
        // - https://en.wikipedia.org/wiki/STL_%28file_format%29#Binary_STL
        // - https://stackoverflow.com/questions/68568214/c-sharp-how-to-parse-an-stl-file-current-function-does-not-link-vertices-into-f
        private IndexedFaceSet DeserializeBinary(string filePath, bool autoCenter)
        {
            double scale = 1.0;
            string name = Path.GetFileNameWithoutExtension(filePath);
            StreamReader sr = new StreamReader(filePath);
            var parsedFaceCoords = new List<Coord>();
            var parsedModel = new List<List<Coord>>();

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        // Read header info
                        byte[] headerBytes = br.ReadBytes(80);
                        byte[] faceCountBytes = br.ReadBytes(4);
                        int faceCount = BitConverter.ToInt32(faceCountBytes, 0);
                        string headerInfo = Encoding.UTF8.GetString(headerBytes, 0, headerBytes.Length).Trim();
                        
                        Debug.WriteLine($"Reading STL, file: {filePath}, faces: {faceCount}, header: {headerInfo}");

                        // Read data from the file until either there is no data left or 
                        // the number of surfaces read is equal to the number of surfaces in the
                        // file. This can prevent reading a partial block at the end and getting
                        // out of range execptions.
                        //
                        // foreach triangle - 50 bytes:
                        //     REAL32[3] – Normal vector             -12 bytes
                        //     REAL32[3] – Vertex 1 - 12 bytes
                        //     REAL32[3] – Vertex 2 - 12 bytes
                        //     REAL32[3] – Vertex 3 - 12 bytes
                        //     UINT16    – Attribute byte count      -2 bytes
                        // end
                        byte[] block;
                        int iterFace = 0;
                        byte[] xComp = new byte[4];
                        byte[] yComp = new byte[4];
                        byte[] zComp = new byte[4];

                        while ((block = br.ReadBytes(50)) != null && iterFace++ < faceCount)
                        {
                            parsedFaceCoords = new List<Coord>();

                            // Parse data block
                            for (int i = 0; i < 4; i++)
                            {
                                for (int k = 0; k < 12; k++)
                                {
                                    int index = k + i * 12;

                                    if (k < 4)
                                    {
                                        // xComp
                                        xComp[k] = block[index];
                                    }
                                    else if (k < 8)
                                    {
                                        // yComp
                                        yComp[k - 4] = block[index];
                                    }
                                    else
                                    {
                                        // zComp
                                        zComp[k - 8] = block[index];
                                    }
                                }
                                // Convert data to useable structures
                                double x = Math.Round(BitConverter.ToSingle(xComp, 0), Global.NormalToleranceDecimalPlaces);
                                double y = Math.Round(BitConverter.ToSingle(yComp, 0), Global.NormalToleranceDecimalPlaces);
                                double z = Math.Round(BitConverter.ToSingle(zComp, 0), Global.NormalToleranceDecimalPlaces);
                                
                                if (i == 0)
                                {
                                    // This is a normal
                                    // Coord normCoord = new Coord(x, y, z);
                                    
                                    //if(Math.Abs(Math.Pow(norm.X,2) + Math.Pow(norm.X, 2) + Math.Pow(norm.X, 2) - 1) > .001)
                                    //{
                                    //    throw new InvalidOperationException("ERROR: Improper file read. Surface normal is not a unit vector.");
                                    //}

                                    // Ignoring Surface Normal within .STL
                                    // ... Add.normal = norm;
                                }
                                else
                                {
                                    // This is a vertex
                                    Coord coord = new Coord(x, y, z);
                                    parsedFaceCoords.Add(coord);
                                }
                            }
                            parsedModel.Add(parsedFaceCoords);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            // IndexedFaceSet internally transformDo post parsing initialization to update Vertex/Edge graph references
            var model = new IndexedFaceSet(CoordMode.XYZ, name, parsedModel, scale, autoCenter);

            return model;

        }


        private IndexedFaceSet DeserializeAscii(string filePath, bool autoCenter)
        {
            double scale = 1.0;
            string name = Path.GetFileNameWithoutExtension(filePath);
            StreamReader sr = new StreamReader(filePath);
            var parsedFaceCoords = new List<Coord>();
            var parsedModel = new List<List<Coord>>();

            try
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    string[] lineParts = line.Split(s_lineDelimiters, StringSplitOptions.RemoveEmptyEntries);

                    if (lineParts[0] == "vertex")
                    {
                        Coord coord = new Coord(
                            Math.Round(double.Parse(lineParts[1]), Global.NormalToleranceDecimalPlaces),
                            Math.Round(double.Parse(lineParts[2]), Global.NormalToleranceDecimalPlaces),
                            Math.Round(double.Parse(lineParts[3]), Global.NormalToleranceDecimalPlaces));
                        parsedFaceCoords.Add(coord);
                    }

                    if (lineParts[0] == "endloop")
                    {
                        parsedModel.Add(parsedFaceCoords);
                        parsedFaceCoords = new List<Coord>();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            // IndexedFaceSet internally transformDo post parsing initialization to update Vertex/Edge graph references
            var model = new IndexedFaceSet(CoordMode.XYZ, name, parsedModel, scale, autoCenter);

            return model;
        }
    }
}
