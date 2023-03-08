using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScratchUtility;

namespace Primitives
{
    public enum CoordMode
    {
        XYZ,
        YZX
    }


    public class IndexedFaceSet
    {
        public List<Vertex> Vertices { get; private set; }
        /// <summary>Gets the List of Edges that make up this IndexedFaceSet</summary>
        public List<Edge> Edges { get; private set; }

        public List<IndexedFace> IndexedFaces; // { get; private set; }

        // Model Coordinates, read during parsing, never modified.
        public List<Coord> AvailableVertexLocations { get; private set; }

        // View Coordinate at Zero viewing angle
        public List<Coord> AvailableViewVertexLocations_ZeroAngle { get; private set; }

        // View Coordinate at Current viewing angle
        public List<Coord> AvailableViewVertexLocations { get; private set; }

        public string Name { get; private set; }

        /// <summary>Gets the Vertex in this IndexedFaceSet that is nearest the user.</summary>
        public Vertex NearestVertex { get; private set; }

        static int edgeID = 0;

        /// <summary>
        /// Creates a new IndexedFaceSet
        /// </summary>
        /// <param name="name">The name of this IndexedFaceSet</param>
        /// <param name="coordIndices">The list of indices (0-based) into the points list from which to construct the IndexedFaces making up this IndexedFaceSet. Example: "46 0 2 44 -1, 3 1 47 45 -1" would create two quadriliteral faces, one with vertex indices 46 0 2 44, and the other with vertex indices 3 1 47 45.</param>
        /// <param name="points">The space-separated, comma delimited list of 3D Vertices used in this IndexedFaceSet. The coordIndices list refers to values in this list. Example: "0.437500 0.164063 0.765625, -0.437500 0.164063 0.765625" specifies two Vertices, one (index 0) at x=0.437500, y=0.164063 z=0.765625, and the other (index 1) at x=-0.437500 y=0.164063 z=0.765625.</param>
        public IndexedFaceSet(CoordMode coordMode, string name, string coordIndices, string points, double scale)
        {
            //todo: use a streamreader.
            Name = name;
            AvailableVertexLocations = new List<Coord>();
            AvailableViewVertexLocations_ZeroAngle = new List<Coord>();
            AvailableViewVertexLocations = new List<Coord>();
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
            edgeID = 0;


            //find all the points
            //string[] coords = points.TrimEnd().Split( new char[] (',', '\r', '\n'), StringSplitOptions.RemoveEmptyEntries);
            string[] coords = points.TrimEnd().Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            // Parse in Two passes, first pass to find min Z to help massage model location
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < coords.Length; i++)
                {
                    if (String.IsNullOrEmpty(coords[i].Trim()))
                    {
                        continue;
                    }

                    string[] vals = coords[i].Trim().Split(' ');
                    //values are stored in y,z,x order
                    double x, y, z;

                    if (coordMode == CoordMode.XYZ)
                    {
                        // Implemented XYZ so importing models straight from Fusion 360 is straightforward.
                        x = double.Parse(vals[0]) * scale;
                        y = double.Parse(vals[1]) * scale;
                        z = double.Parse(vals[2]) * scale;
                    }
                    else if (coordMode == CoordMode.YZX)
                    {
                        // Original/Legacy coord mode
                        x = -double.Parse(vals[1]) * scale;
                        y = double.Parse(vals[2]) * scale;
                        z = -double.Parse(vals[0]) * scale;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected Coord Mode.");
                    }

                    if (pass == 0)
                    {
                        // Find Min/Max bounds for Z.
                        if (z < minZ)
                        {
                            minZ = z;
                        }

                        if (z > maxZ)
                        {
                            maxZ = z;
                        }
                    }
                    else if (pass == 1)
                    {
                        // Offset Z so front face always 0, i.e. fixed, -ve Z will shift as view angle changes.
                        z = z - maxZ;

                        Coord c = new Coord(x, y, z);
                        AvailableVertexLocations.Add(c);
                        AvailableViewVertexLocations_ZeroAngle.Add(c);
                        AvailableViewVertexLocations.Add(c);
                        Vertices.Add(new Vertex(this, i));
                    }
                }
            }

            //create all the indexed faces by creating Edges and connecting them
            string[] indices = coordIndices.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] old_indices = coordIndices.Split(',');
            IndexedFaces = new List<IndexedFace>(indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                if (String.IsNullOrEmpty(indices[i].Trim()))
                {
                    continue;
                }

                string[] vals = indices[i].Trim().Split(' ');
                if (vals.Length < 4) //we're ignoring the last value (seems to always be -1), so there has to be a total of at least 4 values for a triangular IndexedFace
                    throw new Exception("Can not create an IndexedFace from less than 3 Vertices");

                IndexedFace indexedFace = new IndexedFace(this);
                Vertex firstVertex = GetExistingVertex(Vertices[int.Parse(vals[0])].ModelingCoord);
                indexedFace.Vertices.Add(firstVertex);
                firstVertex.IndexedFaces.Add(indexedFace);
                Vertex previousVertex = firstVertex;
                for (int vertexIndex = 1; vertexIndex < vals.Length - 1; vertexIndex++) //ignore the last value (seems to always be -1, not a vertex), so end at Length - 2
                {

                    // Sometimes triangles are represented as squares, using a duplicate Vertex. We
                    // want them to actually be triangles.
                    Vertex currentVertex = GetExistingVertex(Vertices[int.Parse(vals[vertexIndex])].ModelingCoord);
                    if (indexedFace.Vertices.Contains(currentVertex)) 
                    {
                        // Skip...
                        continue;
                    }

                    // If this edge was an existing edge, we need to update it so it knows that it's
                    // now a part of a new IndexedFace
                    Edge e = GetNewOrExistingEdge(previousVertex, currentVertex, indexedFace);
                    if (e.CreatorFace != null && e.CreatorFace != indexedFace && 
                        e.OtherFace != null && e.OtherFace != indexedFace)
                    {
                        // TODO: Consider Debug.Assert()
                        throw new InvalidOperationException("Unexpected, found edge with more than two faces");
                    }
                    else if (e.CreatorFace != indexedFace && e.OtherFace == null)
                    {
                        e.AddFace(indexedFace);
                    }

                    indexedFace.Edges.Add(e);

                    // Make sure the Vertices know that they are now part of the new edge, if they
                    // don't already know.
                    if (!previousVertex.Edges.Contains(e))
                    {
                        previousVertex.Edges.Add(e);
                    }

                    //else
                    //    throw new Exception("how did that edge already know about me?");
                    if (!currentVertex.Edges.Contains(e))
                    {
                        currentVertex.Edges.Add(e);
                    }
                    //else
                    //    throw new Exception("how did that edge already know about me?");

                    indexedFace.Vertices.Add(currentVertex);
                    currentVertex.IndexedFaces.Add(indexedFace);
                    previousVertex = currentVertex;
                }

                //add the Edge that finishes this IndexedFace
                Edge finalEdge = GetNewOrExistingEdge(previousVertex, firstVertex, indexedFace);
                if (finalEdge.CreatorFace != indexedFace) //if this edge was an existing edge, we need to update it so it knows that it's now a part of a new IndexedFace
                    finalEdge.AddFace(indexedFace);

                indexedFace.Edges.Add(finalEdge);

                //update the first and last Vertex to so that they know about the Edge that was just added
                if (!indexedFace.Vertices[0].Edges.Contains(finalEdge))
                    indexedFace.Vertices[0].Edges.Add(finalEdge);
                if (!indexedFace.Vertices[indexedFace.Vertices.Count - 1].Edges.Contains(finalEdge))
                    indexedFace.Vertices[indexedFace.Vertices.Count - 1].Edges.Add(finalEdge);

                indexedFace.IsTransparent = (int.Parse(vals[vals.Length-1]) == 0);

                //we're now ready to set the Normal Vectors for the IndexedFace
                indexedFace.UpdateNormalVector();
                indexedFace.UpdateNormalVector_ModelingCoordinates();

                //now that the IndexedFace knows its NormalVector, we need to update all the Edges so they know their ConnectionType
                foreach (Edge e in indexedFace.Edges)
                {
                    e.UpdateConnectionType();
                }

                IndexedFaces.Add(indexedFace);
            }
        }

        // Make one, initialize with model represented by specified faceVertexes/params.
        //
        // Ensure this method is agnostic to file format used to represent the model.  Format specific
        // parsing should have occurred already.
        public IndexedFaceSet(CoordMode coordMode, string name, List<List<Coord>> facesCoords, double scale)
        {
            this.Name = name;
            this.AvailableVertexLocations = new List<Coord>();
            this.AvailableViewVertexLocations_ZeroAngle = new List<Coord>();
            this.AvailableViewVertexLocations = new List<Coord>();
            this.Vertices = new List<Vertex>();
            this.Edges = new List<Edge>();
            edgeID = 0;

            double minZ = double.MaxValue;
            double maxZ = double.MinValue;
            var nextVertexId = 0;

            // Build index of unique Vertex points, rounding points to configured tolerance
            var knownCoords = new HashSet<string>();
            foreach (var faceCoords in facesCoords)
            {
                foreach (var parsedCoord in faceCoords)
                {
                    Coord coord = parsedCoord.Clone(Global.NormalToleranceDecimalPlaces);

                    if (coord != parsedCoord)
                    {
                        throw new InvalidOperationException("Expected parsedCoord == Coord" +
                            ", mismatch hints that parser didn't round to Global.NormalToleranceDecimalPlaces");
                    }

                    if (knownCoords.Contains(coord.ToString()))
                    {
                        continue;
                    }

                    AvailableVertexLocations.Add(coord);
                    AvailableViewVertexLocations_ZeroAngle.Add(coord);
                    AvailableViewVertexLocations.Add(coord);
                    var vertexIndex = nextVertexId;
                    nextVertexId++;
                    Vertices.Add(new Vertex(this, vertexIndex));
                }
            }

            this.IndexedFaces = new List<IndexedFace>(facesCoords.Count);
            for (int i = 0; i < facesCoords.Count; i++)
            {
                var faceCoords = facesCoords[i];

                // We're ignoring the last value (seems to always be -1), so there has to be a total of at least 4 values for a triangular IndexedFace
                if (faceCoords.Count < 3)
                    throw new Exception("Can not create an IndexedFace from less than 3 Vertices");

                IndexedFace indexedFace = new IndexedFace(this);
                Vertex firstVertex = GetExistingVertex(faceCoords[0]);
                indexedFace.Vertices.Add(firstVertex);
                firstVertex.IndexedFaces.Add(indexedFace);
                Vertex previousVertex = firstVertex;

                for (int vertexIndex = 1; vertexIndex < faceCoords.Count; vertexIndex++)
                {
                    var faceCoord = faceCoords[vertexIndex];

                    // Sometimes triangles are represented as squares, using a duplicate Vertex. We
                    // want them to actually be triangles.
                    Vertex currentVertex = GetExistingVertex(faceCoord);
                    if (indexedFace.Vertices.Contains(currentVertex))
                    {
                        // Skip...
                        continue;
                    }

                    // If this edge was an existing edge, we need to update it so it knows that it's
                    // now a part of a new IndexedFace
                    Edge e = GetNewOrExistingEdge(previousVertex, currentVertex, indexedFace);
                    if (e.CreatorFace != null && e.CreatorFace != indexedFace &&
                        e.OtherFace != null && e.OtherFace != indexedFace)
                    {
                        // TODO: Consider Debug.Assert()
                        throw new InvalidOperationException("Unexpected, found edge with more than two faces");
                    }
                    else if (e.CreatorFace != indexedFace && e.OtherFace == null)
                    {
                        e.AddFace(indexedFace);
                    }

                    indexedFace.Edges.Add(e);

                    // Make sure the Vertices know that they are now part of the new edge, if they
                    // don't already know.
                    if (!previousVertex.Edges.Contains(e))
                    {
                        previousVertex.Edges.Add(e);
                    }

                    //else
                    //    throw new Exception("how did that edge already know about me?");
                    if (!currentVertex.Edges.Contains(e))
                    {
                        currentVertex.Edges.Add(e);
                    }
                    //else
                    //    throw new Exception("how did that edge already know about me?");

                    indexedFace.Vertices.Add(currentVertex);
                    currentVertex.IndexedFaces.Add(indexedFace);
                    previousVertex = currentVertex;
                }

                //add the Edge that finishes this IndexedFace
                Edge finalEdge = GetNewOrExistingEdge(previousVertex, firstVertex, indexedFace);
                if (finalEdge.CreatorFace != indexedFace) //if this edge was an existing edge, we need to update it so it knows that it's now a part of a new IndexedFace
                    finalEdge.AddFace(indexedFace);

                indexedFace.Edges.Add(finalEdge);

                //update the first and last Vertex to so that they know about the Edge that was just added
                if (!indexedFace.Vertices[0].Edges.Contains(finalEdge))
                    indexedFace.Vertices[0].Edges.Add(finalEdge);
                if (!indexedFace.Vertices[indexedFace.Vertices.Count - 1].Edges.Contains(finalEdge))
                    indexedFace.Vertices[indexedFace.Vertices.Count - 1].Edges.Add(finalEdge);

                // TODO: Q Should/how STL support transparent faces?  .X3D parser handles transparent faces.
                //indexedFace.IsTransparent = (int.Parse(vals[vals.Length - 1]) == 0);
                indexedFace.IsTransparent = false;

                //we're now ready to set the Normal Vectors for the IndexedFace
                indexedFace.UpdateNormalVector();
                indexedFace.UpdateNormalVector_ModelingCoordinates();

                //now that the IndexedFace knows its NormalVector, we need to update all the Edges so they know their ConnectionType
                foreach (Edge e in indexedFace.Edges)
                {
                    e.UpdateConnectionType();
                }

                IndexedFaces.Add(indexedFace);
            }
        }

        /// <summary>Creates and returns a new or returns an existing Edge with the specified Vertices. If a new Edge is created, its CreatorIndexedFace will be set to the passed in IndexedFace.</summary>
        private Edge GetNewOrExistingEdge(Vertex v1, Vertex v2, IndexedFace creatorIndexedFace)
        {
            Edge newEdge = new Edge(v1, v2, creatorIndexedFace);
            foreach(Edge e in Edges)
            {
                if (e.ContainsSameVerticesAs(newEdge))
                    return e;
            }
            //if this is a new edge (which it is if we didn't find one with the same vertices), add it to the master collection
            newEdge.EdgeID = edgeID;
            edgeID++;
            Edges.Add(newEdge);
            return newEdge;
        }


        private Vertex GetExistingVertex(Coord modelingCoord)
        {
            return Vertices.Find(v => v.ModelingCoord == modelingCoord);
        }


        /// <summary>Recalculates the AvailableViewVertexLocations for this IndexedFaceSet based on the current Transformation matrix. Also updates the IndexedFaces and Edges accordingly.</summary>
        public void Refresh(bool switchBackFront)
        {
            //update the ViewVertex locations to their new values based on the new ModelToWindow matrix.
            AvailableViewVertexLocations_ZeroAngle.Clear();
            NearestVertex = null;
            foreach (Coord c in AvailableVertexLocations)
            {
                Coord viewCoord = Transformer.ModelToWindow(c);
                AvailableViewVertexLocations_ZeroAngle.Add(viewCoord);
                Vertex newVertex = Vertices[AvailableViewVertexLocations_ZeroAngle.Count - 1];
                //check if the new vertex is now the nearest vertex
                if (NearestVertex == null || newVertex.ViewCoord.Z > NearestVertex.ViewCoord.Z) //nearer Vertices have higher Z values (less negative)
                    NearestVertex = newVertex;
            }
            RefreshArcLocationsOnly(switchBackFront);
        }

        /// <summary>Refreshes only the AvailableViewVertexLocation list by using the stored zero-angle values to determine where each Coord is along its arc.</summary>
        public void RefreshArcLocationsOnly(bool switchBackFront)
        {
            if (ViewContext.CosViewAngle == 0)
            {
                AvailableViewVertexLocations = new List<Coord>(AvailableViewVertexLocations_ZeroAngle);
            }
            else
            {
                AvailableViewVertexLocations.Clear();
                for (int i = 0; i < AvailableViewVertexLocations_ZeroAngle.Count; i++)
                    AvailableViewVertexLocations.Add(Transformer.GetArcCoord(AvailableViewVertexLocations_ZeroAngle[i]));
            }

            //update whether faces are front or back facing because a face may have moved to the other side of the object
            foreach (IndexedFace ifc in IndexedFaces)
            {
                ifc.Refresh(switchBackFront);
            }

            //update the edge type because an edge might have moved to the other side of the object
            foreach (Edge e in Edges)
                e.Refresh();
        }
    }
}