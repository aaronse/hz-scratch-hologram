using System;
using System.Collections.Generic;
using System.Diagnostics;
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

#if DEBUG_USE_PROPS
        public List<Vertex> Vertices { get; private set; }

        public Dictionary<Coord, Vertex> CoordVertexMap { get; private set; }

        /// <summary>Gets the List of Edges that make up this IndexedFaceSet</summary>
        public List<Edge> Edges { get; private set; }

        public List<IndexedFace> IndexedFaces { get; private set; }

        // Model Coordinates, read during parsing, never modified.
        //
        public Coord[] AvailableVertexLocations { get; private set; }
        
        // View Coordinate at Zero viewing angle
        public Coord[] AvailableViewVertexLocations_ZeroAngle { get; private set; }

        // View Coordinate at Current viewing angle
        public Coord[] AvailableViewVertexLocations { get; private set; }

        public string Name { get; private set; }

        /// <summary>Gets the Vertex in this IndexedFaceSet that is nearest the user.</summary>
        public Vertex NearestVertex { get; private set; }
#else
        public List<Vertex> Vertices;
        public Dictionary<Coord, Vertex> CoordVertexMap;

        /// <summary>Gets the List of Edges that make up this IndexedFaceSet</summary>
        public List<Edge> Edges;

        public List<IndexedFace> IndexedFaces;

        // Model Coordinates, read during parsing, never modified.
        //
        public Coord[] AvailableVertexLocations;

        // View Coordinate at Zero viewing angle
        public Coord[] AvailableViewVertexLocations_ZeroAngle;

        // View Coordinate at Current viewing angle
        public Coord[] AvailableViewVertexLocations;

        public string Name;

        /// <summary>Gets the Vertex in this IndexedFaceSet that is nearest the user.</summary>
        public Vertex NearestVertex;
#endif

        static int edgeID { get; set; } // = 0;

        /// <summary>
        /// Creates a new IndexedFaceSet
        /// </summary>
        /// <param name="name">The name of this IndexedFaceSet</param>
        /// <param name="coordIndices">The list of indices (0-based) into the points list from which to construct the IndexedFaces making up this IndexedFaceSet. Example: "46 0 2 44 -1, 3 1 47 45 -1" would create two quadriliteral faces, one with vertex indices 46 0 2 44, and the other with vertex indices 3 1 47 45.</param>
        /// <param name="points">The space-separated, comma delimited list of 3D Vertices used in this IndexedFaceSet. The coordIndices list refers to values in this list. Example: "0.437500 0.164063 0.765625, -0.437500 0.164063 0.765625" specifies two Vertices, one (index 0) at x=0.437500, y=0.164063 z=0.765625, and the other (index 1) at x=-0.437500 y=0.164063 z=0.765625.</param>
        public IndexedFaceSet(
            CoordMode coordMode, 
            string name, 
            string coordIndices, 
            string points, 
            double scale,
            bool autoCenter)
        {
            //todo: use a streamreader.
            Name = name;
            var availableVertexLocations = new List<Coord>();
            var availableViewVertexLocations_ZeroAngle = new List<Coord>();
            var availableViewVertexLocations = new List<Coord>();
            this.Vertices = new List<Vertex>();
            this.CoordVertexMap = new Dictionary<Coord, Vertex>();
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

                        // Offset Z (so not Autocenter... got a better name?) so front face always 0,
                        // i.e. fixed, -ve Z will shift as view angle changes.
                        if (autoCenter)
                        {
                            // Auto center X & Y
                            z = z - maxZ;
                        }

                        Coord c = new Coord(x, y, z);
                        availableVertexLocations.Add(c);
                        availableViewVertexLocations_ZeroAngle.Add(c);
                        availableViewVertexLocations.Add(c);
                        Vertex vertex = new Vertex(this, i);
                        this.Vertices.Add(vertex);
                        this.CoordVertexMap[c] = vertex;
                    }
                }
            }

            // Convert List<T> to Arrays that are faster to seek by index
            this.AvailableVertexLocations = availableVertexLocations.ToArray();
            this.AvailableViewVertexLocations_ZeroAngle = availableViewVertexLocations_ZeroAngle.ToArray();
            this.AvailableViewVertexLocations = availableViewVertexLocations.ToArray();

            //create all the indexed faces by creating Edges and connecting them
            string[] indices = coordIndices.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] old_indices = coordIndices.Split(',');
            this.IndexedFaces = new List<IndexedFace>(indices.Length);
            var knownEdges = new Dictionary<ulong, Edge>();

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
                //Vertex firstVertex = GetExistingVertex(this.Vertices[int.Parse(vals[0])].ModelingCoord);
                Vertex firstVertex = this.Vertices[int.Parse(vals[0])];
                indexedFace.Vertices.Add(firstVertex);
                firstVertex.AddIndexedFace(indexedFace);
                Vertex previousVertex = firstVertex;
                for (int vertexIndex = 1; vertexIndex < vals.Length - 1; vertexIndex++) //ignore the last value (seems to always be -1, not a vertex), so end at Length - 2
                {

                    // Sometimes triangles are represented as squares, using a duplicate Vertex. We
                    // want them to actually be triangles.
                    // Vertex currentVertex = GetExistingVertex(this.Vertices[int.Parse(vals[vertexIndex])].ModelingCoord);
                    Vertex currentVertex = this.Vertices[int.Parse(vals[vertexIndex])];
                    if (indexedFace.Vertices.Contains(currentVertex)) 
                    {
                        // Skip...
                        continue;
                    }

                    // If this edge was an existing edge, we need to update it so it knows that it's
                    // now a part of a new IndexedFace
                    Edge e = GetNewOrExistingEdge(
                        knownEdges,
                        previousVertex,
                        currentVertex,
                        indexedFace);

                    if (e.CreatorFace != null && e.CreatorFace != indexedFace && 
                        e.OtherFace != null && e.OtherFace != indexedFace)
                    {
                        // TODO:P2: Investigate and fix or at least document why this is ok.
                        Debug.WriteLine("Unexpected, found edge with more than two faces");
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
                    currentVertex.AddIndexedFace(indexedFace);
                    previousVertex = currentVertex;
                }

                //add the Edge that finishes this IndexedFace
                Edge finalEdge = GetNewOrExistingEdge(
                    knownEdges,
                    previousVertex,
                    firstVertex,
                    indexedFace);

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
                indexedFace.InitializeModelState();

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
        public IndexedFaceSet(
            CoordMode coordMode, 
            string name, 
            List<List<Coord>> facesCoords, 
            double scale,
            bool autoCenter)
        {
            this.Name = name;
            var availableVertexLocations = new List<Coord>();
            var availableViewVertexLocations_ZeroAngle = new List<Coord>();
            var availableViewVertexLocations = new List<Coord>();
            this.Vertices = new List<Vertex>();
            this.CoordVertexMap = new Dictionary<Coord, Vertex>();
            this.Edges = new List<Edge>();
            edgeID = 0;

            double minZ = double.MaxValue;
            double maxZ = double.MinValue;
            var nextVertexId = 0;

            // Adjust coords needed?
            if (autoCenter)
            {
                foreach (var faceCoords in facesCoords)
                {
                    foreach (var parsedCoord in faceCoords)
                    {
                        if (parsedCoord.Z < minZ) minZ = parsedCoord.Z;
                        if (parsedCoord.Z > maxZ) maxZ = parsedCoord.Z;
                    }
                }
            }

            // Build index of unique Vertex points, rounding points to configured tolerance
            var knownCoords = new HashSet<string>();
            for (int faceIndex = 0; faceIndex < facesCoords.Count; faceIndex++)
            {
                var faceCoords = facesCoords[faceIndex];

                for ( int coordIndex = 0; coordIndex < faceCoords.Count; coordIndex++)
                {
                    var coord = facesCoords[faceIndex][coordIndex];
                    
                    // Adjust coords needed?
                    if (autoCenter)
                    {
                        // Adjusting state passed in... Not just the 'coord' copy because...  Even
                        // further down we're referencing Coord instances to lookup Vertexes being
                        // created in this fragment.
                        double newZ = Math.Round(coord.Z - maxZ, Global.NormalToleranceDecimalPlaces);
                        facesCoords[faceIndex][coordIndex] = coord.SetZ(newZ);
                    }

                    // Check: Ensure Coord rounded to tolerance decimal places to reduce rounding issues
                    // later during rendering/hidden surface detection, etc...
                    Coord coordClone = coord.Clone(Global.NormalToleranceDecimalPlaces);
                    if (coord != coordClone)
                    {
                        throw new InvalidOperationException("Expected coord == coordClone" +
                            ", mismatch hints that parser didn't round to Global.NormalToleranceDecimalPlaces");
                    }

                    if (knownCoords.Contains(coord.ToString()))
                    {
                        continue;
                    }

                    availableVertexLocations.Add(coord);
                    availableViewVertexLocations_ZeroAngle.Add(coord);
                    availableViewVertexLocations.Add(coord);
                    var vertexIndex = nextVertexId;
                    nextVertexId++;
                    Vertex vertex = new Vertex(this, vertexIndex);
                    this.Vertices.Add(vertex);
                    this.CoordVertexMap[coord] = vertex;
                }
            }

            // Convert List<T> to Arrays that are faster to seek by index
            this.AvailableVertexLocations = availableVertexLocations.ToArray();
            this.AvailableViewVertexLocations_ZeroAngle = availableViewVertexLocations_ZeroAngle.ToArray();
            this.AvailableViewVertexLocations = availableViewVertexLocations.ToArray();

            this.IndexedFaces = new List<IndexedFace>(facesCoords.Count);
            var knownEdges = new Dictionary<ulong, Edge>();
            for (int i = 0; i < facesCoords.Count; i++)
            {
                var faceCoords = facesCoords[i];

                // We're ignoring the last value (seems to always be -1), so there has to be a total of at least 4 values for a triangular IndexedFace
                if (faceCoords.Count < 3)
                    throw new Exception("Can not create an IndexedFace from less than 3 Vertices");

                IndexedFace indexedFace = new IndexedFace(this);
                Vertex firstVertex = GetExistingVertex(faceCoords[0]);
                indexedFace.Vertices.Add(firstVertex);
                firstVertex.AddIndexedFace(indexedFace);
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
                    Edge e = GetNewOrExistingEdge(
                        knownEdges,
                        previousVertex,
                        currentVertex,
                        indexedFace);

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
                    currentVertex.AddIndexedFace(indexedFace);
                    previousVertex = currentVertex;
                }

                //add the Edge that finishes this IndexedFace
                Edge finalEdge = GetNewOrExistingEdge(
                    knownEdges,
                    previousVertex,
                    firstVertex,
                    indexedFace);

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
                indexedFace.InitializeModelState();

                //now that the IndexedFace knows its NormalVector, we need to update all the Edges so they know their ConnectionType
                foreach (Edge e in indexedFace.Edges)
                {
                    e.UpdateConnectionType();
                }

                IndexedFaces.Add(indexedFace);
            }

        }

        /// <summary>Creates and returns a new or returns an existing Edge with the specified Vertices. If a new Edge is created, its CreatorIndexedFace will be set to the passed in IndexedFace.</summary>
        private Edge GetNewOrExistingEdge(
            Dictionary<ulong, Edge> knownEdges,
            Vertex v1,
            Vertex v2,
            IndexedFace creatorIndexedFace)
        {
            Edge newEdge = new Edge(v1, v2, creatorIndexedFace);

            // Build key representing with same vertex pairs.  Match even if vertexes swapped.
            ulong edgeKey = (v1.VertexIndex < v2.VertexIndex)
                ? (ulong)((uint)v2.VertexIndex) << 32 | (uint)v1.VertexIndex
                : (ulong)((uint)v1.VertexIndex) << 32 | (uint)v2.VertexIndex;

            // Lookup, or add, edge with matching key
            Edge existingEdge;
            if (knownEdges.TryGetValue(edgeKey, out existingEdge))
            {
                return existingEdge;
            }
            knownEdges[edgeKey] = newEdge;

            newEdge.EdgeID = edgeID;
            edgeID++;
            this.Edges.Add(newEdge);
            return newEdge;
        }


        private Vertex GetExistingVertex(Coord modelingCoord)
        {
            return this.CoordVertexMap[modelingCoord];
        }


        /// <summary>Recalculates the AvailableViewVertexLocations for this IndexedFaceSet based on 
        /// the current Transformation matrix. Also updates the IndexedFaces and Edges accordingly.</summary>
        public void Refresh(bool switchBackFront)
        {
            //update the ViewVertex locations to their new values based on the new ModelToWindow matrix.
            var availableViewVertexLocations_ZeroAngle = new List<Coord>();
            NearestVertex = null;
            foreach (Coord c in this.AvailableVertexLocations)
            {
                Coord viewCoord = Transformer.ModelToWindow(c);
                availableViewVertexLocations_ZeroAngle.Add(viewCoord);
                Vertex newVertex = this.Vertices[availableViewVertexLocations_ZeroAngle.Count - 1];
                //check if the new vertex is now the nearest vertex
                if (NearestVertex == null || newVertex.ViewCoord.Z > NearestVertex.ViewCoord.Z) //nearer Vertices have higher Z values (less negative)
                    NearestVertex = newVertex;
            }

            this.AvailableViewVertexLocations_ZeroAngle = availableViewVertexLocations_ZeroAngle.ToArray();

            RefreshArcLocationsOnly(switchBackFront);
        }

        /// <summary>Refreshes only the AvailableViewVertexLocation list by using the stored 
        /// zero-angle values to determine where each Coord is along its arc.</summary>
        public void RefreshArcLocationsOnly(bool switchBackFront)
        {
            if (ViewContext.CosViewAngle == 0)
            {
                this.AvailableViewVertexLocations = AvailableViewVertexLocations_ZeroAngle;
            }
            else
            {
                var availableViewVertexLocations = new List<Coord>();
                var length = this.AvailableViewVertexLocations_ZeroAngle.Length;
                for (int i = 0; i < length; i++)
                    availableViewVertexLocations.Add(Transformer.GetArcCoord(this.AvailableViewVertexLocations_ZeroAngle[i]));

                this.AvailableViewVertexLocations = availableViewVertexLocations.ToArray();
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