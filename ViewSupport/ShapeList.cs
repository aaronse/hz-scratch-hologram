using Primitives;
using ScratchUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ViewSupport
{
    public class ShapeList : IEnumerable<IndexedFaceSet>
    {
        private List<IndexedFaceSet> mShapes;

        public List<IndexedFaceSet> Shapes { get { return mShapes; } }

        public ShapeList()
        {
            mShapes = new List<IndexedFaceSet>();
        }

        public void Add(IndexedFaceSet ifs)
        {
            mShapes.Add(ifs);
        }
        public void Clear()
        {
            mShapes.Clear();
        }
        public int Count { get { return mShapes.Count; } }

        /// <summary>Calls Refresh() on each of the IndexedFaceSets in this ShapeList (to update the AvailableViewVertexLocations for each to match the latest transform matrix). Also sets the NearestVertex.</summary>
        public void Refresh(bool switchBackFront, bool arcLocationsOnly)
        {
            foreach (IndexedFaceSet ifs in mShapes)
            {
                if(arcLocationsOnly)
                    ifs.RefreshArcLocationsOnly(switchBackFront);
                else
                    ifs.Refresh(switchBackFront);
            }
            //BoundingBoxes must be refreshed after refreshing all the IndexedFaceSets. Otherwise, the BoundingBox for Auxiliary Edges won't be accurate.
            foreach (IndexedFaceSet ifs in mShapes)
            {
                foreach (Edge e in ifs.Edges)
                {
                    e.RefreshBoundingBox();
                }
            }
        }


        /// <summary>Processes the List of IndexedFaceSets to fix visual problems. For example, 
        /// adds a line at the position where two IndexedFaceSets intersect.</summary>
        public void PreProcess()
        {
            DateTime start = DateTime.UtcNow;
            int faceIntersectionCount = 0;
            int boundingBoxSkips = 0;
            int boundingBoxMismatches = 0;
            //todo: first do the plane-plane intersections, then only do the following for planes that intersect


            //compare every face to every edge to split edges at intersections with faces.
            foreach (IndexedFaceSet ifs in mShapes)
            {
                foreach (IndexedFaceSet ifsInner in mShapes)
                {
                    foreach (Edge e in ifs.Edges)
                    {
                        foreach (IndexedFace ifc in ifsInner.IndexedFaces)
                        {
                            // Only compare to faces that the edge isn't a part of
                            // TODO:P1 PERF: ContainsFace internally scans a List...  Hash instead.
                            if (e.ContainsFace(ifc) || e.StartVertex.ContainsFace(ifc) || e.EndVertex.ContainsFace(ifc))
                            {
                                continue;
                            }

                            // Skip processing if no chance of Edge intersecting the face.
                            bool intersects = e.BoundingBox3.Overlaps(ifc.BoundingBox3);
                            if (!intersects)
                            {
                                boundingBoxSkips++; // Measure
                                continue;
                            }

                            Coord c = new Coord(0, 0, 0);
                            if (!ifc.IntersectsWith_ModelingCoordinates(e, ref c))
                            {
                                if (intersects) boundingBoxMismatches++;
                                continue;
                            }

                            // Intentionally left here since am observing inconsistencies that
                            // haven't been root caused. If/when investigating need to change code
                            // above to not bail if (!intersects), otherwise boundingBoxMismatches
                            // will be artificially low.
                            if (!intersects) boundingBoxMismatches++;

                            if (!ifc.ContainsPoint2D_ModelingCoordinates(c))
                            {
                                continue;
                            }

                            double distanceFromStart = (c - e.StartVertex.ModelingCoord).CalcLength() / e.Length_ModelingCoordinates;

                            e.FaceIntersections.Add(new Intersection(e, distanceFromStart));
                            faceIntersectionCount++;
                        }
                    }

                }
            }
            // Calc Avg and Max indexed Faces per edge 
            int avgIndexedFacesPerEdge = mShapes[0].Edges.Aggregate((int)0, (curr, next) => curr + next.StartVertex.IndexedFaces.Count) / mShapes[0].Edges.Count;
            int maxIndexedFacesPerEdge = mShapes[0].Edges.Aggregate((int)0, (curr, next) => Math.Max(curr, next.StartVertex.IndexedFaces.Count));
            double missModelRatio = Math.Round((100.0 * IndexedFace.s_algoModelMismatches) / IndexedFace.s_algoModelCalls, 2);

            int durMs = (int)DateTime.UtcNow.Subtract(start).TotalMilliseconds;
            Debug.WriteLine(
                $"PreProcess, durMs={durMs}, faceIntersectionCount={faceIntersectionCount}" +
                $", avgIndexedFacesPerEdge={avgIndexedFacesPerEdge}, maxIndexedFacesPerEdge={maxIndexedFacesPerEdge}," +
                $", missModelRatio={missModelRatio}, algoModelMismatches={IndexedFace.s_algoModelMismatches}" +
                $", algoModelCalls={IndexedFace.s_algoModelCalls}, boundingBoxSkips={boundingBoxSkips}, boundingBoxMismatches={boundingBoxMismatches}");
        }


        #region IEnumerable<IndexedFaceSet> Members

        public IEnumerator<IndexedFaceSet> GetEnumerator()
        {
            return mShapes.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return mShapes.GetEnumerator();
        }

        #endregion


    }
}
