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

        public List<IndexedFaceSet> IndexedFaceSets { get; set; }

        private bool _hasCamera = false;
        public Coord CameraPosition { get; set; }

        public X3DFile(string fullPath)
        {
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
        }

        /// <summary>Parses the x3d file that this X3dFile represents and extracts the IndexedFaces and CameraPosition.</summary>
        public void Parse(double scale, bool autoCenter)
        {
            this.IndexedFaceSets = new List<IndexedFaceSet>();

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
            List<string> coordIndices = new List<string>();
            List<string> points = new List<string>();
            _hasCamera = false;

            try
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name == "Transform")
                            {
                                name = reader["DEF"];
                            }
                            else if (reader.Name == "IndexedFaceSet")
                            {
                                coordIndices.Add(reader["coordIndex"]);
                            }
                            else if (reader.Name == "Coordinate")
                            {
                                points.Add(reader["point"]);
                            }
                            else if (reader.Name == "Viewpoint")
                            {
                                string[] camera = reader["position"].Split(' ');
                                CameraPosition = new Coord(double.Parse(camera[1]) * scale, double.Parse(camera[2]) * scale, -double.Parse(camera[0]) * scale);
                                _hasCamera = true;
                            }
                            break;
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Scene")
                    {
                        break;
                    }
                }

                for (int i = 0; i < coordIndices.Count; i++)
                {
                    CoordMode coordMode = (_hasCamera) ? CoordMode.YZX : CoordMode.XYZ;
                    IndexedFaceSet ifs = new IndexedFaceSet(
                        coordMode, 
                        name, 
                        coordIndices[i], 
                        points[i], 
                        scale,
                        autoCenter);

                    IndexedFaceSets.Add(ifs);
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
