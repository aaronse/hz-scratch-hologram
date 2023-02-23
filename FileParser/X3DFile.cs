using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Primitives;
using System.Xml;
using System.IO;
using ScratchUtility;

namespace FileParser
{
    public class X3DFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }

        public List<IndexedFaceSet> IndexedFaces { get; set; }
        public Coord CameraPosition { get; set; }

        public X3DFile(string fullPath)
        {
            Name = Path.GetFileNameWithoutExtension(fullPath);
            FullPath = fullPath;
        }

        /// <summary>Parses the x3d file that this X3dFile represents and extracts the IndexedFaces and CameraPosition.</summary>
        public void Parse(double scale)
        {
            IndexedFaces = new List<IndexedFaceSet>();

            StreamReader sr = new StreamReader(FullPath);

            // Using lagging StreamReader to detect when root <Scene> element encountered.
            // Couldn't use sr.BaseStream.Position to track prev position and seek back one line.
            // Reason is underlying buffered stream may not update Position accurately.
            StreamReader srPrev = new StreamReader(FullPath);

            string s = "";
            while (!sr.EndOfStream)
            {
                s = sr.ReadLine().Trim();

                if (s.IndexOf("<scene>", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    break;
                }

                srPrev.ReadLine();
            }

            sr = srPrev;

            XmlTextReader reader = new XmlTextReader(sr);
            
            string name = "";
            string coordIndices = "";
            string points = "";

            try
            {
            
                while (reader.Read())
                {
                    //Console.WriteLine(reader.Name);
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name == "Transform")
                            {
                                name = reader["DEF"];
                            }
                            else if (reader.Name == "IndexedFaceSet")
                            {
                                coordIndices = reader["coordIndex"];
                            }
                            else if (reader.Name == "Coordinate")
                            {
                                points = reader["point"];
                            }
                            else if (reader.Name == "Viewpoint")
                            {
                                string[] camera = reader["position"].Split(' ');
                                CameraPosition = new Coord(double.Parse(camera[1]) * scale, double.Parse(camera[2]) * scale, -double.Parse(camera[0]) * scale);
                                //CameraPosition = new Coord(double.Parse(camera[0]) * scale, double.Parse(camera[1]) * scale, double.Parse(camera[2]) * scale);
                            }
                            break;
                    }


                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "IndexedFaceSet")
                    {
                        IndexedFaceSet ifs = new IndexedFaceSet(name, coordIndices, points, scale);
                        IndexedFaces.Add(ifs);
                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Scene")
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
