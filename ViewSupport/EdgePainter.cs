using Primitives;
using ScratchUtility;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System;

// TODO:P0 Implement Face selection.  Needed for Set Z0, and Select Contour.
// TODO:P0 Implement Contour selection.
// TODO:P0 Implement Reset Z0
// TODO:P0 Edge Point, add Edge List
// TODO:P0 Debug, implement highlighting selected Coord
// TODO:P0 Perf/Quality, merge small Arcs less than X degrees from each other.  And/Or fix underlying rounding/clipping logic error.
// TODO:P1 Refactor, compute and rendering into separate tasks
// TODO:P1 Perf/Bug, observed too many segments per arc, unexpected gaps.  Action: Check Arc Seg logic, determine why unexpected gaps?  Fix.
// TODO:P1 Debug, implement Edge/Face selection highlighting...  Requires mapping Mouse XY coord to projected coords, searching for closest.  Changing currently selected.

// TODO:P0 Perf, set point outside loop. grep... for edgeMidPoint.ToPointD().ToPoint()
// TODO:P0 Perf, fix: 1) shortcut reduce new Coord alloc, 2) avoid multiple caller depth, grep... private static bool IntersectsWith(Coord normalVector, Coord pointOnFace, Coord point1, Coord point2, out Coord intersectionPoint)


namespace ViewSupport
{
    /// <summary>A static class used to paint Edges of IndexedFacesets according to the settings specified in DrawOptions.</summary>
    public static class EdgePainter
    {

        private static int _debugMaxEdgeId = -1;

        internal static List<EdgeSection> s_visibleEdgeSections { get; set; }
        
        public static ShapeList ShapeList { get; set; }

        private static long s_coordHits = 0;

        private static long s_edgeHits = 0;

        private static long s_skippedEdgeCount = 0;

        private static HashSet<string> s_drawnEdges = new HashSet<string>();

        private static Dictionary<string, ArcInfo> s_edgePointVisibleArcs = null;

        // Hash representing computed arc content, used to shortcut/avoid compute when minor rendering computation needed.
        private static string s_holoVisibleHash = null;

        private static Font s_debugFont = new Font("Arial", 8f, FontStyle.Regular);

        private static List<ArcSegInfo> s_arcSegs;

        public static List<ArcSegInfo> ArcSegments { get { return s_arcSegs; } set { s_arcSegs = value; } }

        private static int _selectedId = -1;

        internal class ArcInfo
        {
            internal ArcInfo()
            {

            }

            internal List<Edge> Edges { get; set; }
            internal Coord ZeroCoord { get; set; }
            internal List<EdgeSection> EdgeSections { get; set; }

            // Using bit array to represent angles this Edge point is visible.  The bitarray has
            // been useful during dev/debugging to understand major errors, and rounding errors.
            // 
            // TODO:P2 Memory optimization, assuming but haven't confirmed ~23 bytes used. If
            // memory pressure than consider tuning if most arcs can be represented in less space.
            internal BitArray VisibleAngles { get; set; } = new BitArray(181);

            public override string ToString()
            {
                int[] bitBuckets = new int[18];
                for(int i = 0; i < VisibleAngles.Length; i++)
                {
                    if (i / 10 > bitBuckets.Length) break;
                    if (this.VisibleAngles[i]) bitBuckets[i / 10]++;
                }

                StringBuilder sb = new StringBuilder();
                StringBuilder sbEdgeIDs = new StringBuilder();
                if (this.Edges != null)
                {
                    foreach (var edge in this.Edges.OrderBy(e => e.EdgeID))
                    {
                        if (sbEdgeIDs.Length > 0) sbEdgeIDs.Append(",");
                        sbEdgeIDs.Append(edge.EdgeID);
                    }
                }

                sb.Append($"EdgeId: {sbEdgeIDs}, zc: {this.ZeroCoord.ToString(2)}, es: {this.EdgeSections?.Count}, va: ");

                for (int i = 0; i < bitBuckets.Length; i++)
                {
                    if (bitBuckets[i] == 0)
                    {
                        sb.Append(".");
                    }
                    else
                    {
                        sb.Append(bitBuckets[i] > 9 ? "X" : bitBuckets[i].ToString());
                    }
                }

                return sb.ToString();
            }
        }

        public class ArcSegInfo
        {
            public ArcSegInfo()
            {
            }

            // Public first EdgeID that gets serialize
            public int EdgeID { get; set; }

            // Internally used list of Edges, used for debug selected item highlighting
            internal List<Edge> Edges { get; set; }

            public Coord ZeroCoord { get; set; }
            public Rectangle ArcRect { get; set; }
            public float StartAngle { get; set; }
            public int SweepAngle { get; set; }

            public string EdgeIds
            {
                get
                {
                    if (Edges == null) return null;

                    return Edges.OrderBy(e => e.EdgeID).Aggregate(
                        "",
                        (curr, next) => curr + ((curr.Length == 0) ? "" : ",") + next.EdgeID);
                }
            }

            public override string ToString()
            {
                return $"{{ edgeId: {EdgeID}, zc: {{ x: {Math.Round(ZeroCoord.X, 3)}, y: {Math.Round(ZeroCoord.Y, 3)} }}, startAngle: {StartAngle}, sweepAngle: {SweepAngle}, arcRect: {{x:{ArcRect.X}, y:{ArcRect.Y}, width:{ArcRect.Width}, height:{ArcRect.Height} }} }}";
            }
        }


        internal static void Draw(DrawOptions options)
        {
            HashSet<string> drawnCoords = new HashSet<string>();
            s_drawnEdges.Clear();
            s_coordHits = 0;
            s_edgeHits = 0;
            s_skippedEdgeCount = 0;

            // Any item(s) selected?
            if (!int.TryParse(DrawOptions.SelectedItemExpr, out _selectedId))
            {
                _selectedId = -1;
            }

            // Compute arc segments by computing Edge/Vertex visibility for *EVERY* viewable angle.
            if (DrawOptions.ShowArcSegments)
            {
                MultiViewAngleTravese(options);
            }

            // Calculate Edge visibility, so we know which full edges, or edge sections to draw/arc.
            if (DrawOptions.VisibilityMode == VisibilityMode.HiddenLine)
            {
                TraverseEdges(options);
            }

            // Show Arcs, after visibility sections have been computed.
            if (DrawOptions.ShowArcSegments)
            {
                s_arcSegs = new List<ArcSegInfo>();
                DrawArcSegments(options, s_arcSegs);
            }
            else if (DrawOptions.ShowArcs)
            {
                foreach (IndexedFaceSet ifs in ShapeList)
                    foreach (Edge e in ifs.Edges)
                    {
                        if (_debugMaxEdgeId == -1 || e.EdgeID <= _debugMaxEdgeId)   // Debug limit # of edges
                        {
                            DrawArcs(options, e, drawnCoords);
                        }
                    }
            }

            if (DrawOptions.VisibilityMode == VisibilityMode.HiddenLine)
            {
                foreach (EdgeSection es in s_visibleEdgeSections)
                    DrawEdgeSection(options, es);
            }
            else
            {
                foreach (IndexedFaceSet ifs in ShapeList)
                    foreach (Edge e in ifs.Edges)
                    {
                        if (_debugMaxEdgeId == -1 || e.EdgeID <= _debugMaxEdgeId)   // Debug limit # of edges
                        {
                            DrawEdge(options, e);
                        }
                    }
            }

            System.Diagnostics.Debug.WriteLine($"s_arcSegs.ArcCount={s_arcSegs?.Count}, s_coordHits={s_coordHits}, s_edgeHits={s_edgeHits}, s_skippedEdgeCount={s_skippedEdgeCount}");
        }

        private static void DrawEdge(DrawOptions options, Edge e)
        {
            DrawEdgePart(options, e, e.StartVertex.ViewCoord, e.EndVertex.ViewCoord);
        }
        private static void DrawEdgeSection(DrawOptions options, EdgeSection es)
        {
            DrawEdgePart(options, es.Edge, es.StartCoord, es.EndCoord);
        }


        private static void DrawEdgePart(DrawOptions options, Edge e, Coord startCoord, Coord endCoord)
        {
            Point startPoint = startCoord.ToPointD().ToPoint();
            Point endPoint = endCoord.ToPointD().ToPoint();

            string edgeHash = ((startPoint.X + startPoint.Y) <= (endPoint.X + endPoint.Y))
                ? $"{startPoint.X}:{startPoint.Y}-{endPoint.X}:{endPoint.Y}"
                : $"{endPoint.X}:{endPoint.Y}-{startPoint.X}:{startPoint.Y}";
            
            // Skip already drawn edges
            if (!s_drawnEdges.Add(edgeHash))
            {
                s_edgeHits++;  // Measure cache savings
                return;
            }

            Pen pPen = options.Theme.VectorPen;
            Color backupColor = pPen.Color;
            Color actualColor = 
                (DrawOptions.ViewMode == ViewMode.RedBlue && ViewContext.StereoscopicMode != StereoscopicMode.NonStereoscopic) 
                ? Color.Black 
                : pPen.Color;

            pPen.Color = actualColor;

            if (options.IsRendering && DrawOptions.VectorMode)
            {
                // Draw Edge as vector Line
                options.Graphics.DrawLine(pPen, startPoint, endPoint);
            }

            if (options.IsRendering && DrawOptions.PointsMode)
            {
                // Draw Edge as Points along a vector
                Brush pBrush = 
                    (DrawOptions.ViewMode == ViewMode.RedBlue) 
                    ? options.Theme.RedBlueModePointBrush 
                    : options.Theme.PointBrush;

                foreach (Coord c in e.GetPoints(DrawOptions.ViewPointsPerUnitLength, false))
                {
                    PointD p = c.ToPointD();
                    
                    //Draw the point if visible
                    if ((DrawOptions.VisibilityMode == VisibilityMode.Transparent || Global.IsBetween(p, startCoord.ToPointD(), endCoord.ToPointD())))
                        Drawing.DrawPoint(options, p.ToPoint(), pBrush, (int)DrawOptions.PointWidth);
                }
            }
            pPen.Color = backupColor;
        }
        

        /// <summary>
        /// Draw Arcs not already drawn
        /// </summary>
        private static void DrawArcs(DrawOptions options, Edge e, HashSet<string> drawnCoords)
        {
            // Skip Edges without any visible sections
            // Also tried !es.Visible but observed arcs for hidden points being unexpectedly rendered.
            if (DrawOptions.VisibilityMode == VisibilityMode.HiddenLine && !e.EdgeSections.Any(es => s_visibleEdgeSections.Contains(es))) 
            {
                s_skippedEdgeCount++;
                return;
            }

            List<Coord> points = e.GetPoints(DrawOptions.ViewPointsPerUnitLength, true);
            foreach (Coord c in points)
            {
                string coordHash = $"{c.X}:{c.Y}:{c.Z}";
                if (!drawnCoords.Add(coordHash))
                {
                    s_coordHits++;  // Measure cache savings
                    continue;
                }

                Rectangle arcRect = Transformer.GetArcSquare(c);
                float startAngle = c.Z - ViewContext.N_ViewCoordinates > 0 ? 0 : 180;
                if (options.IsRendering)
                {
                    if (_selectedId == e.EdgeID)
                    {
                        options.Graphics.DrawArc(options.Theme.SelectedPen, arcRect, startAngle, 180);
                    }
                    else
                    {
                        options.Graphics.DrawArc(options.Theme.ArcPen, arcRect, startAngle, 180);
                    }

                    if (Global.DebugMode)
                        options.Graphics.DrawString(
                            "A" + e.EdgeID.ToString(),
                            s_debugFont,
                            options.Theme.ArcTextBrush,
                            new PointF(
                                arcRect.X + arcRect.Width / 2,
                                arcRect.Y + ((startAngle == 0) ? arcRect.Height : 0)));
                }
            }
        }

        private static void DrawArcSegments(DrawOptions options, List<ArcSegInfo> arcSegs)
        {
            foreach (var arcInfo in s_edgePointVisibleArcs.Values)
            {
                List<Edge> edges = arcInfo.Edges;

                var c = arcInfo.ZeroCoord;

                Rectangle arcRect = Transformer.GetArcSquare(c);
                float startAngle = c.Z - ViewContext.N_ViewCoordinates > 0 ? 0 : 180;

                if (options.IsRendering && DrawOptions.ShowArcs)
                {
                    // Intentionally drawing full arc here to help verify Arc Segment logic.
                    // Eventually, should have just one implementation, so, remove this, or
                    // the old DrawArc code, when bug free... 
                    options.Graphics.DrawArc(options.Theme.ArcPen, arcRect, startAngle, 180);
                }

                var edgeIdsStr = edges?.Aggregate("", (curr, next) => curr + ", " + next.EdgeID);

                if (options.IsRendering && Global.DebugMode)
                {

                    options.Graphics.DrawString(
                        "A " + edgeIdsStr,
                        s_debugFont,
                        options.Theme.ArcTextBrush,
                        new PointF(
                            arcRect.X + arcRect.Width / 2,
                            arcRect.Y + ((startAngle == 0) ? arcRect.Height : 0)));
                }

                var visibleAngles = arcInfo.VisibleAngles;

                // Max number of angles to seek/peek ahead of first miss.  Goal being to merge
                // segments due to logic/rounding issues.  Will potentially merge arcs that
                // shouldn't be, so, for now we'll a least track and log the hits/merges/assumptions
                // being made here until if/when upstream logic fixed.
                var maxVisibleMisses = 5;

                for (int i = 0; i < visibleAngles.Count;)
                {
                    // Find first visible
                    while (i < visibleAngles.Length && !visibleAngles[i]) i += options.ViewAngleResolution;


                    // Find last visible, treat small contiguous gaps as single segement.  Goal is
                    // to avoid lots of tiny, almost adjacent, segments.   we
                    // use (k) to peek ahead.  Potentially nudging (j) to keep advancing until
                    // acceptably long enough segment end is encountered.
                    int j = i;
                    int lastVisible = j;
                    int visibleMisses = 0;
                    while (j < visibleAngles.Length - 1 && (visibleAngles[j] || visibleMisses < maxVisibleMisses))
                    {
                        visibleMisses = (visibleAngles[j]) ? 0 : visibleMisses++;

                        if (visibleAngles[j]) lastVisible = j;

                        j += options.ViewAngleResolution;
                    }

                    if (visibleMisses < maxVisibleMisses)
                    {
                        lastVisible = j;
                    }


                    // Arc i to max(i, j - 1), providing i found a visible
                    if (i < visibleAngles.Length && visibleAngles[i])
                    {
                        int arcSegStart = i;
                        int arcSegEnd = Math.Max(i, lastVisible - options.ViewAngleResolution);

                        HashSet<int> edgeIds = new HashSet<int>();
                        foreach (var edge in edges)
                        {
                            edgeIds.Add(edge.EdgeID);
                        }
                        //var edgeIds = edges?.Aggregate("", (curr, next) => curr + ", " + next.EdgeID);

                        // Add Arc segment to data structure to delay renderering
                        // until after ALL simple (not visibility sensitive) 180deg
                        // arcs drawn
                        arcSegs.Add(new ArcSegInfo()
                        {
                            EdgeID = edges?[0]?.EdgeID ?? 0,
                            Edges = edges,
                            ZeroCoord = c,
                            ArcRect = arcRect,
                            StartAngle = startAngle + arcSegStart,
                            SweepAngle = arcSegEnd - arcSegStart
                        });
                    }

                    // Advance i
                    i = Math.Max(j, i + options.ViewAngleResolution);
                }
            }
            
            if (options.IsRendering)
            {
                foreach (var arcSeg in arcSegs)
                {
                    if (arcSeg.Edges.Any(e => e.EdgeID == _selectedId))
                    {
                        options.Graphics.DrawArc(
                            options.Theme.SelectedPen,
                            arcSeg.ArcRect,
                            arcSeg.StartAngle,
                            arcSeg.SweepAngle);
                    }
                    else
                    {
                        options.Graphics.DrawArc(
                            options.Theme.ArcPenHighlight,
                            arcSeg.ArcRect,
                            arcSeg.StartAngle,
                            arcSeg.SweepAngle);
                    }
                }
            }
        }

        // TODO: PERF improve algorithms, reduce repeated/unrequired work, implement Async multi core compute
        internal static void MultiViewAngleTravese(DrawOptions renderOptions)
        {
            string viewModelHash = ViewContext.GetViewHash() + ":" + DrawOptions.GetViewHash();

            if (s_holoVisibleHash == viewModelHash)
            {
                Debug.WriteLine("MultiViewAngleTravese, cache hit, durMs=0");

                return;
            }

            s_edgePointVisibleArcs =
                new Dictionary<string, ArcInfo>(StringComparer.OrdinalIgnoreCase);

            DateTime start = DateTime.Now;

            DrawOptions computeOnlyOptions = renderOptions.Clone();
            computeOnlyOptions.IsRendering = false;

            // Save ViewAngle for current render
            double drawViewAngle = ViewContext.ViewAngle;

            // For each possible viewing angle, figure out all of the Edge points that will be
            // displayed, together with their arcs.
            for (int iterAngle = ViewContext.MinViewAngle; iterAngle < ViewContext.MaxViewAngle; iterAngle += computeOnlyOptions.ViewAngleResolution)
            {
                // Set ViewAngle
                ViewContext.ViewAngle = iterAngle;

                // Refresh computed Vertex/Faces
                ShapeList.Refresh(DrawOptions.SwitchBackFront, false);

                // Compute visible edge sections (and vertexes)
                TraverseEdges(computeOnlyOptions);

                // Upsert to HoloVisibleArcs using s_visibleEdgeSections
                foreach (EdgeSection es in s_visibleEdgeSections)
                {
                    // TODO: Assert no dupes

                    Point startPoint = es.StartCoord.ToPointD().ToPoint();
                    Point endPoint = es.EndCoord.ToPointD().ToPoint();

                    List<Coord> angledCoords = es.Edge.GetPoints(DrawOptions.ViewPointsPerUnitLength, false);
                    List<Coord> zeroCoords = es.Edge.GetPoints(DrawOptions.ViewPointsPerUnitLength, true);

                    for (int i = 0; i < angledCoords.Count; i++)
                    {
                        PointD angledCoord = angledCoords[i].ToPointD();
                        if (Global.IsBetween(angledCoord, es.StartCoord.ToPointD(), es.EndCoord.ToPointD()))
                        {
                            // NOT using EdgeID in hash since multiple Points for multiple edges
                            // could be at the same X,Y,Z point.  We want to render one or more
                            // points as a single arc.
                            string pointHash = zeroCoords[i].ToString();

                            ArcInfo arcInfo = null;
                            if (!s_edgePointVisibleArcs.TryGetValue(pointHash, out arcInfo))
                            {
                                s_edgePointVisibleArcs[pointHash] = arcInfo = new ArcInfo()
                                {
                                    Edges = new List<Edge>() { es.Edge },
                                    ZeroCoord = zeroCoords[i],
                                };
                            }
                            else
                            {
                                if (!arcInfo.Edges.Contains(es.Edge))
                                {
                                    arcInfo.Edges.Add(es.Edge);
                                }
                            }

                            arcInfo.VisibleAngles[iterAngle - ViewContext.MinViewAngle] = true;
                        }
                    }
                }
            }

            // Restore current ViewAngle
            ViewContext.ViewAngle = drawViewAngle;

            // Refresh computed Vertex/Faces for render of current ViewAngle
            ShapeList.Refresh(DrawOptions.SwitchBackFront, false);

            // Mark computed arcs based on current view orientation's hash
            s_holoVisibleHash = viewModelHash;

            Debug.WriteLine("MultiViewAngleTravese, durMs=" + (int)DateTime.Now.Subtract(start).TotalMilliseconds);
        }

        internal static void TraverseEdges(DrawOptions options)
        {
            s_visibleEdgeSections = new List<EdgeSection>();
            
            count = 0;

            foreach (IndexedFaceSet ifs in ShapeList)
            {
                foreach (Edge e in ifs.Edges)
                {
                    if (_debugMaxEdgeId == -1 || e.EdgeID <= _debugMaxEdgeId)   // Debug limit # of edges
                    {
                        ProcessEdge(options, e);
                    }
                }
            }
        }
        static double count = 0;


        /// <summary>Splits the edge up into sections with constant visibility by determining if and where it intersects any of the ShapeList's Silhouette Edges.</summary>
        internal static void ProcessEdge(DrawOptions options, Edge e)
        {
            //foreach (Intersection inter in e.FaceIntersections)
            //{
            //    //options.Graphics.FillEllipse(Brushes.Black, new Rectangle((int)inter.IntersectionPoint_ViewCoordinates.X - 5, (int)inter.IntersectionPoint_ViewCoordinates.Y - 5, 10, 10));
            //    //options.Graphics.DrawString(ScratchUtility.Transformer.WindowToModel(inter.IntersectionPoint_ViewCoordinates).ToString(2), new Font("Arial", 8f, FontStyle.Regular), Brushes.Magenta, (float)inter.IntersectionPoint_ViewCoordinates.X - 5, (float)inter.IntersectionPoint_ViewCoordinates.Y - 5);
            //}

            //Global.Print("Processing Edge " + e.StartVertex.VertexIndex + " to " + e.EndVertex.VertexIndex);
            if (e.Type == EdgeType.FrontFacing || e.Type == EdgeType.Silhouette)
            {
                //initialize the intersections list with the face intersections.
                List<Intersection> intersections = new List<Intersection>(e.FaceIntersections);
                //List<Intersection> intersections = new List<Intersection>();


                //loop through every Silhouette Edge in the ShapeList
                bool isQuickMode = DrawOptions.QuickMode;
                foreach (IndexedFaceSet ifs in ShapeList)
                {
                    foreach (Edge edgeToCheck in ifs.Edges.Where(silhouetteEdge => !isQuickMode || silhouetteEdge.Type == EdgeType.Silhouette)) //check against all edges if not QuickMode
                    {
                        if (e == edgeToCheck) //don't compare against itself
                        {
                            continue;
                        }

                        //Check for intersection
                        Coord intersectionPoint;

                        if (e.IntersectsBehind(edgeToCheck, out intersectionPoint))
                        {
                            double distanceFromStart = (intersectionPoint - e.StartVertex.ViewCoord).CalcLength() / e.Length_ViewCoordinates;
                            intersections.Add(new Intersection(e, distanceFromStart));
                            if (options.IsRendering && Global.DebugMode)
                                options.Graphics.FillEllipse(Brushes.Orange, new Rectangle((int)intersectionPoint.X - 5, (int)intersectionPoint.Y - 5, 10, 10));
                        }
                    }
                }

                //we now know the location of every intersection with a Silhouette Edge that is in front of the Edge being processed.
                //the list needs to be ordered in the direction of travel so we can appropriately add the EdgeSections to the Edge's list.

                //Separate the Edge into the appropriate EdgeSections ordered by increasing distance from the e.StartVertex.
                IOrderedEnumerable<Intersection> orderedIntersections = 
                    intersections.OrderBy<Intersection, double>(
                        c => (c.IntersectionPoint_ViewCoordinates - e.StartVertex.ViewCoord).CalcLength());

                e.EdgeSections.Clear();
                Coord lastCoord = e.StartVertex.ViewCoord;
                EdgeSection es;
                foreach (Intersection si in orderedIntersections)
                {
                    es = new EdgeSection(e, lastCoord, si.IntersectionPoint_ViewCoordinates);
                    ComputeVisibility(options, es);
                    e.EdgeSections.Add(es);
                    if (es.Visible)
                        s_visibleEdgeSections.Add(es);
                    lastCoord = si.IntersectionPoint_ViewCoordinates;
                }

                es = new EdgeSection(e, lastCoord, e.EndVertex.ViewCoord);
                ComputeVisibility(options, es);
                e.EdgeSections.Add(es);
                if (es.Visible)
                    s_visibleEdgeSections.Add(es);
            }
        }


        /// <summary>Appropriately sets the Visible property of the specified EdgeSection.</summary>
        private static void ComputeVisibility(DrawOptions options, EdgeSection es)
        {
            // TODO:P0 PERF Add Coord.CalcMidPoint
            Coord edgeMidPoint = (es.StartCoord + es.EndCoord) / 2; //test visibility of the midpoint of the EdgeSection

            if (options.IsRendering && Global.DebugMode)
                options.Graphics.DrawString(
                    es.Edge.EdgeID.ToString() + " (" + count.ToString() + ")",
                    s_debugFont,
                    options.Theme.RedBlueModePointBrush,
                    edgeMidPoint.ToPointF());
            count++;

            bool isQuickMode = DrawOptions.QuickMode;
            foreach (IndexedFaceSet ifs in ShapeList)
            {
                foreach (IndexedFace face in ifs.IndexedFaces.Where(iface => !iface.IsTransparent && (!isQuickMode || iface.IsFrontFacing))) //if QuickMode, only check against front-facing faces
                {
                    // Edge visible if part of specified face.  Could be Creator or Other face.
                    if (es.Edge.ContainsFace(face)) 
                    {
                        continue;
                    }

                    // TODO:P2:Check whether visible edge accuracy here is sufficient.  We're only testing Edge's midpoint.

                    // Skip checking edges obviously outside Face's bounding box, must be outside of the polygon (no overlap).
                    if (!face.BoundingBox.Contains(edgeMidPoint.ToPointD().ToPoint()))
                    {
                        continue;
                    }

                    // Skip checking edge if midpoint is outside 2D projection of the face.
                    if (!face.ContainsPoint2D(edgeMidPoint))
                    {
                        continue;
                    }

                    // NOT Visible if iterated face is blocking view of edge's midpoint
                    if (face.IsBetweenCameraAndPoint3D(edgeMidPoint))
                    {
                        es.Visible = false;
                        return;
                    }
                }
            }

            // 'Merge faces', i.e. hide redundant edge inbetween two adjacent faces with the same normal?
            if (DrawOptions.MergeFaces)
            {
                if (es.Edge.OtherFace != null && es.Edge.CreatorFace.NormalVector.Equals(
                    es.Edge.OtherFace.NormalVector, 
                    Global.NormalToleranceDecimalPlaces))
                {
                    es.Visible = false;
                    return;
                }
            }

            es.Visible = true;
        }

  
        private static bool SilhouetteEdgeExistsAtVertex(Vertex v)
        {
            foreach (Edge e in v.Edges)
                if (e.Type == EdgeType.Silhouette)
                    return true;
            return false;
        }

    }
}
