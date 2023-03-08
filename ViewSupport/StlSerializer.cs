using System;
using System.Collections.Generic;
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

        public static IndexedFaceSet Deserialize(string filePath)
        {
            // TODO:P0 Implement auto align Z axis... Massage model's Z axis

            StlSerializer instance = new StlSerializer();
            return instance.InternalDeserialize(filePath);
        }

        private IndexedFaceSet InternalDeserialize(string filePath)
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
            catch (Exception e)
            {
                throw;
            }

            // IndexedFaceSet internally transformDo post parsing initialization to update Vertex/Edge graph references
            var model = new IndexedFaceSet(CoordMode.XYZ, name, parsedModel, scale, autoCenter: true);

            return model;
        }
    }
}
