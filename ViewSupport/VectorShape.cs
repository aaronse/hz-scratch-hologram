using Primitives;
using ScratchUtility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;



namespace ViewSupport
{
    public class VectorShape
    {


    }

    public class LineShape : VectorShape
    {
        public string Color;
        public PointF Start;
        public PointF End;
    }

    public class ArcShape : VectorShape
    {
        public ArcShape()
            : base()
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

    public class PathShape : VectorShape
    {
        public string Color;
        public List<Tuple<PointF, PointF>> Path;
    }

}

