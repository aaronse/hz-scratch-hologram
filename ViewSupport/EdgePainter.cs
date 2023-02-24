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


        internal static void Draw(DrawOptions options)
        {
            DrawOptions.Gcode.Clear();
            HashSet<string> drawnCoords = new HashSet<string>();
            s_drawnEdges.Clear();
            s_coordHits = 0;
            s_edgeHits = 0;
            s_skippedEdgeCount = 0;

            // Calculate Edge visibility, so we know which full edges, or edge sections to draw/arc.
            if (DrawOptions.VisibilityMode == VisibilityMode.HiddenLine)
            {
                TraverseEdges(options);
            }

            // Show Arcs, after visibility sections have been computed.
            if (DrawOptions.ShowArcs)
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

            // TODO:P2 Consider implementing flag to conditionally render light points
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
            if (!e.EdgeSections.Any(es => s_visibleEdgeSections.Contains(es))) 
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
                }

                if (options.IsRendering && Global.DebugMode)
                    options.Graphics.DrawString(
                        e.EdgeID.ToString(),
                        new Font("Arial", 8f, FontStyle.Regular),
                        options.Theme.ArcTextBrush,
                        new PointF(
                            arcRect.X + arcRect.Width / 2,
                            arcRect.Y + ((startAngle == 0) ? arcRect.Height : 0)));


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
                    new Font("Arial", 8f, FontStyle.Regular),
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
                if (es.Edge.CreatorFace.NormalVector.Equals(es.Edge.OtherFace.NormalVector, Global.NormalToleranceDecimalPlaces))
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
