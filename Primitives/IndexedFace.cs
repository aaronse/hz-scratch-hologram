using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScratchUtility;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Primitives
{
    /// <summary>IndexedFace is analagous to a polygon.</summary>
    public class IndexedFace
    {
        public IndexedFaceSet ParentIndexedFaceSet { get; private set; }
        public List<Edge> Edges { get; private set; }
        public List<Vertex> Vertices { get; private set; } //this list is in order of the way they were defined in the data file to preserve frontface/backface
        private Coord mNormalVector;
        private Coord mNormalVector_ModelingCoordinates;
        // TODO: Remove
        //private bool mIsFrontFacing;
        //private bool mIsTransparent;
        //private Rectangle mBoundingBox;

        private static double mIntersectionTolerance = 0;//.01; //0.0000000000001;

        // TODO:P1:PERF: Profiler showed original GDI based IsVisible Algo consuming 32% of total cycles.  Rewrote/inlined IsVisible, omitted IsOutlineVisible logic for now...  Clipping/hidden surface is partially broken with existing and new algorithm when edge has negative Z axis coordinates, and/or when object close-to/behind camera.

        // Algo version(s) to use for hotpath IsVisible checks
        // 1 = Use GDI APIs, per original implementation.
        // 2 = Use faster PNPoly implementation, with pre check for boundary box.  Implementation from reddit, algo from https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
        // 3 = Use to measure accuracy/regressions/differences.  Compute using both Algos, accumulate total and mismatched callsstate
        // TODO: Implement Algo 2 edge intersection.  Currently only GDI implementation exists.  3-4% difference in results until Edge intersection implemented.
        public static byte s_useIsVisibleAlgoVersion = 2;

        private static Pen s_blackPen = new Pen(Color.Black, 2f);
        public static long _count = 0;

        public static int s_algoCalls = 0;
        public static int s_algoMismatches = 0;

        internal IndexedFace(IndexedFaceSet parentIndexedFaceSet)
        {
            ParentIndexedFaceSet = parentIndexedFaceSet;
            Edges = new List<Edge>();
            Vertices = new List<Vertex>();
            this.IsFrontFacing = true;
            this.IsTransparent = false;
        }

        public Coord NormalVector_ModelingCoordinates
        {
            get
            {
                return mNormalVector_ModelingCoordinates;
            }
        }

        public Coord NormalVector
        {
            get
            {
                return mNormalVector;
            }
        }

        public bool IsFrontFacing;
        //{
        //    get
        //    {
        //        return mIsFrontFacing;
        //    }
        //}

        public bool IsTransparent;
        // TODO: Remove
        //{
        //    get
        //    {
        //        return mIsTransparent;
        //    }
        //    internal set
        //    {
        //        mIsTransparent = value;
        //    }
        //}

        public Rectangle BoundingBox;


        // TODO: Remove
        //{
        //    get
        //    {
        //        return mBoundingBox;
        //    }
        //}

        private GraphicsPath _graphicsPath = null;
        private PointF[] _pointPath = null;

        // Static Array of Point Path Arrays, used to reduce mem/cpu for calculating point visibility within a Path using GDI
        // PathPointType.Start = 0, PathPointType.Line = 0
        // Related resources:
        // - .NET GDI wrapper... http://www.dotnetframework.org/default.aspx/DotNET/DotNET/8@0/untmp/whidbey/REDBITS/ndp/fx/src/CommonUI/System/Drawing/Advanced/GraphicsPath@cs/1/GraphicsPath@cs
        // - Underlying GDI methods... GdipIsVisiblePathPoint PtInRegion
        private static byte[][] s_pathPointTypeArrays = new byte[][]
        {
            new byte[] {},
            new byte[] { 0 },
            new byte[] { 0, 1 },
            new byte[] { 0, 1, 1 },
            new byte[] { 0, 1, 1, 1 },
        };

        // TODO:P1:PERF: Profile 3%
        public GraphicsPath GraphicsPath
        {
            get
            {
                if (_graphicsPath != null) return _graphicsPath;

                PointF[] fs = new PointF[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                {
                    fs[i] = Vertices[i].ViewCoord.ToPointF();
                }
                GraphicsPath g = new GraphicsPath(fs, s_pathPointTypeArrays[Vertices.Count]);
                g.CloseAllFigures();

                _graphicsPath = g;
                //_pointPath = fs;

                return g;
            }
        }

        // TODO: Deprecated, remove after confirming Algo 2 correct, and is faster
        public GraphicsPath GraphicsPath_ModelingCoordinates
        {
            get
            {
                Coord axisUnitVector = NormalVector;
                Coord perpendicularUnitVector = axisUnitVector.CrossProduct((Vertices[1].ModelingCoord - Vertices[0].ModelingCoord).CalcUnitVector()); //parallel to plane

                PointF[] fs = new PointF[Vertices.Count];
                byte[] ts = new byte[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                {
                    fs[i] = Transformer.ViewFromAxis(Vertices[i].ModelingCoord, axisUnitVector, perpendicularUnitVector);
                    ts[i] = (byte)PathPointType.Line;
                }
                ts[0] = (byte)PathPointType.Start;
                GraphicsPath g = new GraphicsPath(fs, ts);
                g.CloseAllFigures();
                return g;
            }
        }

        /// <summary>Gets the direction vector (in view coordinates) of the passed-in edge when viewed from this IndexedFace.</summary>
        public Coord GetDirectionVector(Edge e)
        {
            if (this == e.CreatorFace)
                return (e.EndVertex.ViewCoord - e.StartVertex.ViewCoord);
            else if (this == e.OtherFace)
                return (e.StartVertex.ViewCoord - e.EndVertex.ViewCoord);
            else
                throw new Exception("Failed to get direction vector. This Face is not one of supplied Edge's Faces.");
        }


        // TODO:P0:PERF ~25% !!!
        /// <summary>Returns true if the supplied Coord is contained within the on-screen projection of this IndexedFace. All Z values are ignored.</summary>
        public bool ContainsPoint2D(Coord c)
        {
            bool isOutlineVisibleAlgo1 = false;
            bool isVisibleAlgo1 = false;
            bool isVisibleAlgo2 = false;


            _count++;

            if ((s_useIsVisibleAlgoVersion & 1) == 1)
            {
                GraphicsPath g = this.GraphicsPath;

                PointF p1 = new Point((int)c.X, (int)c.Y);
                isOutlineVisibleAlgo1 = g.IsOutlineVisible(new Point((int)c.X, (int)c.Y), s_blackPen);
                isVisibleAlgo1 = g.IsVisible(p1) || isOutlineVisibleAlgo1;
            }


            if ((s_useIsVisibleAlgoVersion & 2) == 2)
            {
                if (_pointPath == null)
                {
                    PointF[] path = new PointF[Vertices.Count + 1];
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        path[i].X = (float)Vertices[i].ViewCoord.X;
                        path[i].Y = (float)Vertices[i].ViewCoord.Y;
                    }
                    // Close edge
                    path[Vertices.Count].X = path[0].X;
                    path[Vertices.Count].Y = path[0].Y;

                    _pointPath = path;
                }

                isVisibleAlgo2 = IsPointInPolygon((float)c.X, (float)c.Y, _pointPath) || isOutlineVisibleAlgo1;
            }


            s_algoCalls++;
            if (isVisibleAlgo1 != isVisibleAlgo2) s_algoMismatches++;

            ////Non-Zero Winding Method is used to determine whether or not a point is contained within the IndexedFace.
            ////imagine a line from the point going out to somewhere that is for sure outside of the IndexedFace. If the sum of the signs of the cross product of
            ////that line and all the IndexedFace edges that intersect it is non-zero, the point is inside the IndexedFace

            ////Coord rayToDistantPoint = new Edge(c, new ViewPoint(new Coord(32768, 32768, 0))); //new Coord(BoundingBox.Right + 1, BoundingBox.Bottom + 1, 0)

            //Coord rayCoord = new Coord(BoundingBox.Right + 1, BoundingBox.Bottom + 1, 0);


            //int crossProductSignSum = 0;
            //foreach (Edge e in Edges)
            //{
            //    if (Global.LinesIntersect(e.StartVertex.ViewCoord, e.EndVertex.ViewCoord, c, rayCoord))
            //    {
            //        //work in 2D:
            //        Coord edgeCoord = GetDirectionVector(e);
            //        edgeCoord.Z = 0;

            //        crossProductSignSum += Math.Sign(edgeCoord.CrossProduct(rayCoord - c).Z);
            //    }
            //}
            //return (crossProductSignSum != 0);

            return ((s_useIsVisibleAlgoVersion & 1) == 1) ? isVisibleAlgo1 : isVisibleAlgo2;
        }

        /// <summary>
        /// PNPoly, plus pre check for boundary box, See https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
        /// </summary>
        public bool IsPointInPolygon(float x, float y, PointF[] polygon)
        {
            float minX, maxX, minY, maxY;
            minX = maxX = polygon[0].X;
            minY = maxY = polygon[0].Y;
            for (int i = 1; i < polygon.Length; i++)
            {
                PointF q = polygon[i];
                if (q.X < minX) minX = q.X;
                if (q.X > maxX) maxX = q.X;
                if (q.Y < minY) minY = q.Y;
                if (q.Y > maxY) maxY = q.Y;
            }

            if (x < minX || x > maxX || y < minY || y > maxY)
            {
                return false;
            }

            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].Y > y) != (polygon[j].Y > y) &&
                     x < (polygon[j].X - polygon[i].X) * (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>Returns true if the supplied Modeling Coord is contained within the IndexedFace. After rotating, all Z values are ignored.</summary>
        public bool ContainsPoint2D_ModelingCoordinates(Coord c)
        {
            Coord axisUnitVector = NormalVector;
            Coord perpendicularUnitVector = axisUnitVector.CrossProduct((Vertices[1].ModelingCoord - Vertices[0].ModelingCoord).CalcUnitVector()); //parallel to plane

            GraphicsPath g = GraphicsPath_ModelingCoordinates;
            PointF cTransformed = Transformer.ViewFromAxis(c, axisUnitVector, perpendicularUnitVector);
            return (g.IsVisible(cTransformed));
        }
        

        /// <summary>
        /// Recalculates the NormalVector, IsFrontFacing property and BoundingBox for this IndexedFaceSet to reflect the latest AvailableViewVertexLocation data.
        /// </summary>
        //internal void Refresh()
        //{
        //    Refresh(false);
        //}

        /// <summary>Sets NormalVector_ModelingCoordinates. Only needs to be called once after adding all Vertices.</summary>
        internal void UpdateNormalVector_ModelingCoordinates()
        {
            //update the vector. don't just use the first three vertices - the polygon might have a convex edge there and the result will be wrong.
            mNormalVector_ModelingCoordinates = new Coord(0, 0, 0);
            for (int i = 0; i < Vertices.Count - 2; i++)
            {
                mNormalVector_ModelingCoordinates += (Vertices[i].ModelingCoord - Vertices[i + 1].ModelingCoord).CrossProduct(Vertices[i + 2].ModelingCoord - Vertices[i + 1].ModelingCoord);
            }
            int c = Vertices.Count;
            mNormalVector_ModelingCoordinates += (Vertices[c - 1].ModelingCoord - Vertices[c - 2].ModelingCoord).CrossProduct(Vertices[0].ModelingCoord - Vertices[c - 2].ModelingCoord);
            mNormalVector_ModelingCoordinates += (Vertices[c - 1].ModelingCoord - Vertices[0].ModelingCoord).CrossProduct(Vertices[1].ModelingCoord - Vertices[0].ModelingCoord);
            mNormalVector_ModelingCoordinates /= mNormalVector_ModelingCoordinates.CalcLength();
        }

        /// <summary>Sets the NormalVector to reflect the current view of the IndexedFace on the screen. Automatically called from Refresh().</summary>
        internal void UpdateNormalVector()
        {
            //update the vector. don't just use the first three vertices - the polygon might have a convex edge there and the result will be wrong.
            mNormalVector = new Coord(0, 0, 0);
            for (int i = 0; i < Vertices.Count - 2; i++)
            {
                mNormalVector += (Vertices[i].ViewCoord - Vertices[i + 1].ViewCoord).CrossProduct(Vertices[i + 2].ViewCoord - Vertices[i + 1].ViewCoord);
            }
            int c = Vertices.Count;
            mNormalVector += (Vertices[c - 1].ViewCoord - Vertices[c - 2].ViewCoord).CrossProduct(Vertices[0].ViewCoord - Vertices[c - 2].ViewCoord);
            mNormalVector += (Vertices[c - 1].ViewCoord - Vertices[0].ViewCoord).CrossProduct(Vertices[1].ViewCoord - Vertices[0].ViewCoord);
            mNormalVector /= mNormalVector.CalcLength();

            if (!mNormalVector.IsValid())
            {
                StringBuilder b = new StringBuilder();
                b.Append("Invalid Normal Vector: ").AppendLine(mNormalVector.ToString());
                b.AppendLine("Vertices: ");
                foreach (Vertex v in Vertices)
                    b.AppendLine(v.ViewCoord.ToString());
                System.Diagnostics.Debug.WriteLine(b.ToString());
                //throw new Exception(b.ToString());
            }
        }

        internal void Refresh(bool switchBackFront)
        {
            UpdateNormalVector();

            if (_graphicsPath != null)
            {
                _graphicsPath.Dispose();
                // TODO: Call _graphicsPath.Dispose(); when IndexdFace out of scope
            }
            _graphicsPath = null;
            _pointPath = null;

            if (this.IsTransparent || Edges.Any<Edge>(e => e.OtherFace == null))
            {
                this.IsFrontFacing = true;
            }
            else
            {
                //the face may no longer be front-facing (or no longer back-facing)
                double dotProduct = NormalVector.DotProduct(new Coord(0, 0, 1));
                int dotSign = Math.Sign(dotProduct);
                this.IsFrontFacing = switchBackFront ? (dotSign == -1) : (dotSign == 1);
            }
            //update the bounding box
            BoundingBox = Global.GetRectangleWithGivenCorners(Vertices[0].ViewCoord.ToPointD(), Vertices[1].ViewCoord.ToPointD());
            for (int i = 1; i < Vertices.Count; i++)
            {
                BoundingBox = Rectangle.Union(BoundingBox, Global.GetRectangleWithGivenCorners(Vertices[i - 1].ViewCoord.ToPointD(), Vertices[i].ViewCoord.ToPointD()));
            }
        }


        public bool IsBetweenCameraAndPoint3D(Coord point_ViewCoordinates)
        {
            Coord cameraPoint = new Coord(
                point_ViewCoordinates.X,
                point_ViewCoordinates.Y,
                0); //compare to a point at the same location but on the screen.

            return IntersectsWith_ViewCoordinates(point_ViewCoordinates, cameraPoint);
        }

        private bool IntersectsWith_ViewCoordinates(Coord point1, Coord point2)
        {
            Coord c;
            return IntersectsWith(NormalVector, Vertices[0].ViewCoord, point1, point2, out c);
        }

        public bool IntersectsWith_ModelingCoordinates(Edge toIntersect, out Coord intersectionPoint_ModelingCoordinates)
        {
            return IntersectsWith(NormalVector_ModelingCoordinates, Vertices[0].ModelingCoord, toIntersect.StartVertex.ModelingCoord, toIntersect.EndVertex.ModelingCoord, out intersectionPoint_ModelingCoordinates);
        }

        private static bool IntersectsWith(Coord normalVector, Coord pointOnFace, Coord point1, Coord point2, out Coord intersectionPoint)
        {
            //algorithm from http://local.wasp.uwa.edu.au/~pbourke/geometry/planeline/

            intersectionPoint = new Coord(0, 0, 0);
            double uDenom = normalVector.DotProduct(point2 - point1);
            if (uDenom == 0) //the line from p2 to p1 is perpendicular to the plane's normal (i.e. parallel to plane)
                return false;

            double uNum = normalVector.DotProduct(pointOnFace - point1);
            double u = uNum / uDenom;
            if (Global.IsNotWithinTolerance(u))
                return false;
            else
            {
                //P = P1 + u (P2 - P1)
                intersectionPoint = point1 + u * (point2 - point1);
                return true;
            }
        }
    }
}
