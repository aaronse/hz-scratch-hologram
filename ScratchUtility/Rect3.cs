using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchUtility
{
    // 3D bounding box state, use for cheap intersection testing.
    public class Rect3
    {
        // PERF: Intentionally not using properties, or arrays that need indexing even.
#if DEBUG_USE_PROPS
        public double XMin { get; private set; }
        public double YMin { get; private set; }
        public double ZMin { get; private set; }
        public double XMax { get; private set; }
        public double YMax { get; private set; }
        public double ZMax { get; private set; }
#else
        public double XMin;
        public double YMin;
        public double ZMin;
        public double XMax;
        public double YMax;
        public double ZMax;
#endif

        public Rect3(Coord coord1, Coord coord2)
        {
            double xMin = (coord1.X < coord2.X) ? coord1.X : coord2.X;
            double xMax = (coord1.X < coord2.X) ? coord2.X : coord1.X;
            double yMin = (coord1.Y < coord2.Y) ? coord1.Y : coord2.Y;
            double yMax = (coord1.Y < coord2.Y) ? coord2.Y : coord1.Y;
            double zMin = (coord1.Z < coord2.Z) ? coord1.Z : coord2.Z;
            double zMax = (coord1.Z < coord2.Z) ? coord2.Z : coord1.Z;

            this.XMin = xMin;
            this.YMin = yMin;
            this.ZMin = zMin;
            this.XMax = xMax;
            this.YMax = yMax;
            this.ZMax = zMax;
        }

        public Rect3(double xMin, double yMin, double zMin, double xMax, double yMax, double zMax)
        {
            this.XMin = xMin;
            this.YMin = yMin;
            this.ZMin = zMin;
            this.XMax = xMax;
            this.YMax = yMax;
            this.ZMax = zMax;
        }

        // See https://www.euclideanspace.com/threed/animation/collisiondetect/index.htm
        public bool Overlaps(Rect3 other)
        {
            bool overlaps =
                XMin < other.XMax && XMax > other.XMin &&
                YMin < other.YMax && YMax > other.YMin &&
                ZMin < other.ZMax && ZMax > other.ZMin;

            return overlaps;
        }
    }
}
