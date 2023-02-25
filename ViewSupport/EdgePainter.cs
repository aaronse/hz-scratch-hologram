using Primitives;
using ScratchUtility;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System;

namespace ViewSupport
{
    /// <summary>A static class used to paint Edges of IndexedFacesets according to the settings specified in DrawOptions.</summary>
    internal static class EdgePainter
    {
        // TODO: Remove
        //private static Graphics surface;
        internal static List<EdgeSection> s_visibleEdgeSections { get; set; }
        
        public static ShapeList ShapeList { get; set; }

        private static long s_coordHits = 0;

        private static long s_edgeHits = 0;

        private static long s_skippedEdgeCount = 0;

        private static HashSet<string> s_drawnEdges = new HashSet<string>();

        private static Dictionary<string, ArcInfo> s_holoVisibleArcs = null;

        // Hash representing computed arc content, used to shortcut/avoid compute when minor rendering computation needed.
        private static string s_holoVisibleHash = null;

        private static Font s_debugFont = new Font("Arial", 8f, FontStyle.Regular);

        internal static void Draw(DrawOptions options)
        {
            DrawOptions.Gcode.Clear();

            HashSet<string> drawnCoords = new HashSet<string>();
            s_drawnEdges.Clear();
            s_coordHits = 0;
            s_edgeHits = 0;
            s_skippedEdgeCount = 0;

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
                DrawArcSegments(options, DrawOptions.Gcode);
            }
            else if (DrawOptions.ShowArcs)
            {
                foreach (IndexedFaceSet ifs in ShapeList)
                    foreach (Edge e in ifs.Edges)
                        DrawArcs(options, e, DrawOptions.Gcode, drawnCoords);
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
                        DrawEdge(options, e);
            }

            System.Diagnostics.Debug.WriteLine($"gcode.ArcCount={DrawOptions.Gcode.ArcCount}, s_coordHits={s_coordHits}, s_edgeHits={s_edgeHits}, s_skippedEdgeCount={s_skippedEdgeCount}");
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
        private static void DrawArcs(DrawOptions options, Edge e, GCodeInfo gcode, HashSet<string> drawnCoords)
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
                    options.Graphics.DrawArc(options.Theme.ArcPen, arcRect, startAngle, 180);

                    if (Global.DebugMode)
                        options.Graphics.DrawString(
                            e.EdgeID.ToString(),
                            s_debugFont,
                            options.Theme.ArcTextBrush,
                            new PointF(
                                arcRect.X + arcRect.Width / 2,
                                arcRect.Y + ((startAngle == 0) ? arcRect.Height : 0)));
                }

                if (DrawOptions.ShowGcode)
                {
                    // TODO:... int plungeRate = 3; plungeRate, Angle, Elipsis for distortion correction...
                    int rapidXY = 4000;
                    int zLift = 4;
                    float toolDepth = 0.25f;
                    float x1 = (startAngle == 0) ? arcRect.Left : arcRect.Right;
                    float y1 = arcRect.Top + arcRect.Height / 2;
                    float x2 = (startAngle == 0) ? arcRect.Right : arcRect.Left;
                    float y2 = arcRect.Top + arcRect.Height / 2;
                    float r = arcRect.Width / 2;
                    StringBuilder sbGcode = gcode.sbGcode;
                    gcode.ArcCount++;
                    sbGcode.AppendLine($"G0 X{x1} Y{y1} F{rapidXY}");   // Rapid to arc start
                    sbGcode.AppendLine($"G0 Z{toolDepth}");             // Z Plunge
                    sbGcode.AppendLine($"G2 X{x2} Y{y2} R{r}");         // Arc Move, Clockwise
                    sbGcode.AppendLine($"G0 Z{zLift}");                 // Z Lift
                }
            }
        }

        public class ArcSegInfo
        {
            public ArcSegInfo ()
            {
            }

            public Rectangle ArcRect { get; set; }
            public float StartAngle { get; set; } 
            public int SweepAngle { get; set; }
        }

        private static void DrawArcSegments(DrawOptions options, GCodeInfo gcode)
        {
            List<ArcSegInfo> arcSegs = new List<ArcSegInfo>();

            foreach (var arcInfo in s_holoVisibleArcs.Values)
            {
                Edge e = arcInfo.Edge;
            
                List<Coord> points = e.GetPoints(DrawOptions.ViewPointsPerUnitLength, true);
                foreach (Coord c in points)
                {
                    // TODO: Check whether (!drawnCoords.Add(coordHash)) return...

                    if (options.IsRendering)
                    {
                        Rectangle arcRect = Transformer.GetArcSquare(c);
                        float startAngle = c.Z - ViewContext.N_ViewCoordinates > 0 ? 0 : 180;

                        if (DrawOptions.ShowArcs)
                        {
                            // Intentionally drawing full arc here to help verify Arc Segment logic.
                            // Eventually, should have just one implementation, so, remove this, or
                            // the old DrawArc code, when bug free... 
                            options.Graphics.DrawArc(options.Theme.ArcPen, arcRect, startAngle, 180);
                        }

                        //for (int iterAngle = 0; iterAngle < 180; iterAngle += options.ViewAngleResolution)
                        //int startAngle = arcInfo.VisibleAngles[0];
                        var visibleAngles = arcInfo.VisibleAngles;

                        // TODO: Optimize memory, if ViewAngleResolution likely to remain more than 1, then 1) Make VisibleAngles array smaller and change options.ViewAngleResolution increments here to just ++
                        //if (Global.DebugMode)
                        {
                            for (int i = 0; i < visibleAngles.Count;) //i+= options.ViewAngleResolution
                            {
                                // Find first visible
                                while (i < visibleAngles.Length && !visibleAngles[i]) i += options.ViewAngleResolution;

                                // Find last visible
                                int j = i; 
                                while (j < visibleAngles.Length - 1 && visibleAngles[j]) j += options.ViewAngleResolution;

                                // Arc i to max(i, j - 1), providing i found a visible
                                if (i < visibleAngles.Length && visibleAngles[i])
                                {
                                    int arcSegStart = i;
                                    int arcSegEnd = Math.Max(i, j - options.ViewAngleResolution);

                                    // Add Arc segment to data structure to delay renderering
                                    // until after ALL simple (not visibility sensitive) 180deg
                                    // arcs drawn
                                    arcSegs.Add(new ArcSegInfo()
                                    {
                                        ArcRect = arcRect,
                                        StartAngle = startAngle + arcSegStart,
                                        SweepAngle = arcSegEnd - arcSegStart
                                    });
                                }

                                // Advance i
                                i = Math.Max(j, i + options.ViewAngleResolution);
                            }
                        }
                    }

                    //if (Global.DebugMode)
                    //    options.Graphics.DrawString(
                    //        e.EdgeID.ToString(),
                    //        s_debugFont,
                    //        options.Theme.ArcTextBrush,
                    //        new PointF(
                    //            arcRect.X + arcRect.Width / 2,
                    //            arcRect.Y + ((startAngle == 0) ? arcRect.Height : 0)));
                    //}
                }
            }

            foreach(var arcSeg in arcSegs)
            {
                options.Graphics.DrawArc(
                    options.Theme.ArcPenHighlight,
                    arcSeg.ArcRect,
                    arcSeg.StartAngle,
                    arcSeg.SweepAngle);
            }
        }

        internal class ArcInfo
        {
            internal ArcInfo()
            {

            }

            internal Edge Edge { get; set; }
            internal Coord ZeroCoord { get; set; }
            internal List<EdgeSection> EdgeSections { get; set; }
            internal BitArray VisibleAngles { get; set; } = new BitArray(181);
        }

        // TODO: PERF improve algorithms, reduce repeated/unrequired work, implement Async multi core compute
        internal static void MultiViewAngleTravese(DrawOptions renderOptions)
        {
            if (s_holoVisibleHash == ViewContext.GetViewHash())
            {
                Debug.WriteLine("MultiViewAngleTravese, cache hit, durMs=0");

                return;
            }

            s_holoVisibleArcs =
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
                            string pointHash = es.Edge.EdgeID + ":" + zeroCoords[i].ToString();

                            if (!s_holoVisibleArcs.ContainsKey(pointHash))
                            {
                                s_holoVisibleArcs[pointHash] = new ArcInfo()
                                {
                                    Edge = es.Edge,
                                    ZeroCoord = zeroCoords[i],
                                };
                            }

                            s_holoVisibleArcs[pointHash].VisibleAngles[iterAngle + -ViewContext.MinViewAngle] = true;
                        }
                    }
                }
            }

            // Restore current ViewAngle
            ViewContext.ViewAngle = drawViewAngle;

            // Refresh computed Vertex/Faces for render of current ViewAngle
            ShapeList.Refresh(DrawOptions.SwitchBackFront, false);

            // Mark computed arcs based on current view orientation's hash
            s_holoVisibleHash = ViewContext.GetViewHash();

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
                    ProcessEdge(options, e);
                }
            }
        }
        static double count = 0;


        /// <summary>Splits the edge up into sections with constant visibility by determining if and where it intersects any of the ShapeList's Silhouette Edges.</summary>
        internal static void ProcessEdge(DrawOptions options, Edge e)
        {
            foreach (Intersection inter in e.FaceIntersections)
            {
                //surface.FillEllipse(Brushes.Black, new Rectangle((int)inter.IntersectionPoint_ViewCoordinates.X - 5, (int)inter.IntersectionPoint_ViewCoordinates.Y - 5, 10, 10));
                //surface.DrawString(ScratchUtility.Transformer.WindowToModel(inter.IntersectionPoint_ViewCoordinates).ToString(2), new Font("Arial", 8f, FontStyle.Regular), Brushes.Magenta, (float)inter.IntersectionPoint_ViewCoordinates.X - 5, (float)inter.IntersectionPoint_ViewCoordinates.Y - 5);
            }

            Global.Print("Processing Edge " + e.StartVertex.VertexIndex + " to " + e.EndVertex.VertexIndex);
            if (e.Type == EdgeType.FrontFacing || e.Type == EdgeType.Silhouette)
            {
                //initialize the intersections list with the face intersections.
                List<Intersection> intersections = new List<Intersection>(e.FaceIntersections);
                //List<Intersection> intersections = new List<Intersection>();


                //loop through every Silhouette Edge in the ShapeList
                foreach (IndexedFaceSet ifs in ShapeList)
                {
                    foreach (Edge edgeToCheck in ifs.Edges.Where(silhouetteEdge => !DrawOptions.QuickMode || silhouetteEdge.Type == EdgeType.Silhouette)) //check against all edges if not QuickMode
                    {
                        if (e == edgeToCheck) //don't compare against itself
                        {
                            continue;
                        }

                        //Check for intersection
                        Coord intersectionPoint;

                        if (e.IntersectsBehind(edgeToCheck, out intersectionPoint))
                        {
                            double distanceFromStart = (intersectionPoint - e.StartVertex.ViewCoord).Length / e.Length_ViewCoordinates;
                            intersections.Add(new Intersection(e, distanceFromStart));
                            if (Global.DebugMode)
                                options.Graphics.FillEllipse(Brushes.Red, new Rectangle((int)intersectionPoint.X - 5, (int)intersectionPoint.Y - 5, 10, 10));
                        }
                    }
                }

                //we now know the location of every intersection with a Silhouette Edge that is in front of the Edge being processed.
                //the list needs to be ordered in the direction of travel so we can appropriately add the EdgeSections to the Edge's list.

                //Separate the Edge into the appropriate EdgeSections ordered by increasing distance from the e.StartVertex.
                IOrderedEnumerable<Intersection> orderedIntersections = 
                    intersections.OrderBy<Intersection, double>(
                        c => (c.IntersectionPoint_ViewCoordinates - e.StartVertex.ViewCoord).Length);

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

                Debug.Assert(lastCoord != e.EndVertex.ViewCoord, "Should we skip zero length Edge?");
            
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
            Coord edgeMidPoint = (es.StartCoord + es.EndCoord) / 2; //test visibility of the midpoint of the EdgeSection

            if (Global.DebugMode)
                options.Graphics.DrawString(
                    es.Edge.EdgeID.ToString() + " (" + count.ToString() + ")",
                    s_debugFont,
                    options.Theme.RedBlueModePointBrush,
                    edgeMidPoint.ToPointF());
            count++;

            foreach (IndexedFaceSet ifs in ShapeList)
            {
                foreach (IndexedFace face in ifs.IndexedFaces.Where(iface => !iface.IsTransparent && (!DrawOptions.QuickMode || iface.IsFrontFacing))) //if QuickMode, only check against front-facing faces
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
                if (es.Edge.OtherFace != null && es.Edge.CreatorFace.NormalVector.Equals(es.Edge.OtherFace.NormalVector, Global.NormalToleranceDecimalPlaces))
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
