﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScratchUtility;

namespace Primitives
{
    /// <summary>Represents a vertex adjoining (generally) three or more IndexedFaces by way of being the end-point for the Edges connecting those IndexedFaces.</summary>
    public class Vertex
    {
        /// <summary>The list of Edges that this Vertex is a part of.</summary>
        public List<Edge> Edges { get; private set; }

        /// <summary>The list of IndexedFaces that this Vertex is a part of.</summary>
        private List<IndexedFace> IndexedFacesList;
        private HashSet<IndexedFace> IndexedFacesSet;


#if DEBUG_USE_PROPS
        /// <summary>Specifies the index into the parent IndexFaceSet's AvailableVertexLocations and AvailableViewVertexLocations lists that this Vertex's location is stored.</summary>
        public int VertexIndex { get; private set; }

        /// <summary>The IndexedFaceSet that this Vertex is a part of.</summary>
        public IndexedFaceSet ParentIndexedFaceSet { get; private set; }
#else
        public int VertexIndex;

        /// <summary>The IndexedFaceSet that this Vertex is a part of.</summary>
        public IndexedFaceSet ParentIndexedFaceSet;
#endif

        public Vertex(IndexedFaceSet parentIndexedFaceSet, int vertexIndex)
        {
            ParentIndexedFaceSet = parentIndexedFaceSet;
            VertexIndex = vertexIndex;
            Edges = new List<Edge>();
            IndexedFacesList = new List<IndexedFace>();
            IndexedFacesSet = new HashSet<IndexedFace>();
        }

        public Coord ModelingCoord
        {
            get
            {
                return ParentIndexedFaceSet.AvailableVertexLocations[VertexIndex];
            }
        }
        public Coord ViewCoord
        {
            get
            {
                return ParentIndexedFaceSet.AvailableViewVertexLocations[VertexIndex];
            }
        }
        public Coord ViewCoord_ZeroAngle
        {
            get
            {
                return ParentIndexedFaceSet.AvailableViewVertexLocations_ZeroAngle[VertexIndex];
            }
        }

        public bool ContainsFace(IndexedFace ifc)
        {
            return IndexedFacesSet.Contains(ifc);
        }

        public void AddIndexedFace(IndexedFace face)
        {
            this.IndexedFacesList.Add(face);
            this.IndexedFacesSet.Add(face);
        }

        public int GetIndexedFacesCount()
        {
            return this.IndexedFacesList.Count;
        }
    }
}
