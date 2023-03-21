using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Drawing;

namespace ScratchUtility
{
    public static class Transformer
    {
        private static Matrix mModelToWindowMatrix;
        public static Matrix mWindowToModelMatrix { get; private set; }

        static MatrixD4x4 ModelToWindowMatrix4x4;
        
        public static int ModelToWindowAlgoMismatches;
        public static int ModelToWindowAlgoTotal;

        static Transformer()
        {
            mModelToWindowMatrix = new Matrix();
            mWindowToModelMatrix = new Matrix();
        }
        
        public static Matrix ModelToWindowMatrix
        {
        		get
        		{
        		    return mModelToWindowMatrix;
        		}
        		set
        		{
                    mModelToWindowMatrix = value;
                    ModelToWindowMatrix4x4 = mModelToWindowMatrix.ToMatrixD4x4();
                    mWindowToModelMatrix = mModelToWindowMatrix.Inverse();
                    ModelToWindowAlgoMismatches = 0;
                    ModelToWindowAlgoTotal = 0;
                }
        }

        //unpredictable results if windowCoord.Z != 0
        public static Coord WindowToModel(Coord windowCoord)
        {
            Matrix toTransform = windowCoord.ToVectorCol(true);
            Matrix result = mWindowToModelMatrix * toTransform;

            //normalize for perspective
            result /= result[3, 0];

            return new Coord(result[0, 0], result[1, 0], result[2, 0]);
        }

        private static Coord s_nullCoord = new Coord(0, 0, 0);

        public static Coord ModelToWindow(Coord modelCoord)
        {
#if DEBUG_COMPARE_ALGOS

            Coord retVal1;
            Coord retVal2;

            if ((Global.ModelToWindowAlgorithm & 1) == 1)
            {
                // Algo 1 
                Matrix toTransform = modelCoord.ToVectorCol(true); // 4 row, 1 col
                Matrix result = ModelToWindowMatrix * toTransform; // 4x4 * 1x4

                //normalize for perspective
                //double perspectiveNormalization = result[3, 0];
                var perspectiveNorm = result[3, 0];
                result /= perspectiveNorm;

                retVal1 = new Coord(result[0, 0], result[1, 0], result[2, 0]);
                retVal2 = s_nullCoord;
            }

            if ((Global.ModelToWindowAlgorithm & 2) == 2)
            {
#endif
                // Algo 2: Inlined cr@p out of everything since Algo 1 was Hot Path.
                // TODO:P2 Float based Matrix4x4 resulted in mismatch between Algos, caused by rounding
                // so implemented double based MatrixD4x4 for now.  Can switch to float Matrix4x4
                // if/when everything switches.

                // Matrix multiply
                // - Where:
                //   - Matrix4x4 is 1 based (not 0)
                //   - Syntax is m<row><col>
                // [r1] = (m11 * v1) + (m12 * v2) + (m13 * v3) + (m14 * v4)
                // [r2..r4] same as above
                var m = ModelToWindowMatrix4x4;
                Coord c = modelCoord;

                //// Inline Matrix4x4 * Vector3
                //float r1 = (m.M11 * c.X) + (m.M12 * c.Y) + (m.M13 * c.Z) + (m.M14);
                //float r2 = (m.M21 * c.X) + (m.M22 * c.Y) + (m.M23 * c.Z) + (m.M24);
                //float  r3 = (m.M31 * c.X) + (m.M32 * c.Y) + (m.M33 * c.Z) + (m.M34);
                //float  r4 = (m.M41 * c.X) + (m.M42 * c.Y) + (m.M43 * c.Z) + (m.M44);

                // Inline Normalization( Matrix4x4 * Vector3 )
                // Note: r4 should be normalized to 1.0 if referenced after r1..r3 are assigned.
                double r4 = (m.M41 * c.X) + (m.M42 * c.Y) + (m.M43 * c.Z) + (m.M44);
                double r1 = ((m.M11 * c.X) + (m.M12 * c.Y) + (m.M13 * c.Z) + (m.M14)) / r4;
                double r2 = ((m.M21 * c.X) + (m.M22 * c.Y) + (m.M23 * c.Z) + (m.M24)) / r4;
                double r3 = ((m.M31 * c.X) + (m.M32 * c.Y) + (m.M33 * c.Z) + (m.M34)) / r4;

#if !DEBUG_COMPARE_ALGOS
                return new Coord(r1, r2, r3);
#else
                retVal2 = new Coord(r1, r2, r3);
                retVal1 = s_nullCoord;
            }

            if (Global.ModelToWindowAlgorithm == 3 && !retVal1.Equals(retVal2, 2))
            {
                // TODO: Compute avg/variance of mismatch
                ModelToWindowAlgoMismatches++;
            }
            ModelToWindowAlgoTotal++;

            //return ((s_useIsVisibleAlgoVersion & 1) == 1) ? isVisibleAlgo1 : isVisibleAlgo2;
            return ((Global.ModelToWindowAlgorithm & 1) == 1) ? retVal1 : retVal2;
#endif
        }

        public static PointF ViewFromAxis(Coord pointToTransform, Coord viewAxisUnitVector, Coord perpendicularAxisUnitVector)
        {
            Coord thirdUnitVector = viewAxisUnitVector.CrossProduct(perpendicularAxisUnitVector);

            Matrix m = new Matrix(3);
            m[0, 0] = viewAxisUnitVector.X;
            m[0, 1] = viewAxisUnitVector.Y;
            m[0, 2] = viewAxisUnitVector.Z;
            m[1, 0] = perpendicularAxisUnitVector.X;
            m[1, 1] = perpendicularAxisUnitVector.Y;
            m[1, 2] = perpendicularAxisUnitVector.Z;
            m[2, 0] = thirdUnitVector.X;
            m[2, 1] = thirdUnitVector.Y;
            m[2, 2] = thirdUnitVector.Z;

            Matrix toTransform = pointToTransform.ToVectorCol(false);

            Matrix result = m * toTransform;
            PointF retVal = new PointF((float)result[1, 0], (float)result[2, 0]);
            return retVal;
        }


        public static Coord GetArcCoord(Coord locationAtZeroAngle)
        {
            // Find the Center Point of the arc:
            // - X value is at this ViewPoint's X value because when the ViewPoint is drawn at 0
            //   angle, it appears at the apex of the arc.
            // - Y value is either shifted up or down from that point depending on whether or not
            //   the point is in front of or behind the canvas. The amount shifted is directly
            //   proportional to the distance to the canvas.
            // - if in front of the canvas, we want the arc u-shaped (with Location.Y at the
            //   bottom of the arc), so the center is Location.Y - Distance (Distance will
            //   be positive if in front of canvas)
            // - if behind the canvas, we want the arc n-shaped (with Location.Y at the top of the
            //   arc), so the center is Location.Y + Math.Abs(Distance), or Location.Y - Distance
            //   (because distance is negative if behind canvas) either way, the Y value of the
            //   arc center is at Location.Y - DistanceFromCanvas.
            double distanceFromCanvas = locationAtZeroAngle.Z - ViewContext.N_ViewCoordinates;
            PointD center = new PointD(locationAtZeroAngle.X, locationAtZeroAngle.Y - distanceFromCanvas / 2);

            PointD withOriginAtZero = locationAtZeroAngle.ToPointD() - center;

            // it doesn't matter whether we're doing an upside-down or rightside-up arc - because
            // we're rotating about the center point - and it will be above or below us depending
            // - we'll end up at the right place.
            return new Coord(
                withOriginAtZero.X * ViewContext.CosViewAngle - withOriginAtZero.Y * ViewContext.SinViewAngle + center.X,
                withOriginAtZero.X * ViewContext.SinViewAngle + withOriginAtZero.Y * ViewContext.CosViewAngle + center.Y, 
                locationAtZeroAngle.Z);
        }

        public static Rectangle GetArcSquare(Coord locationAtZeroAngle)
        {
            double distanceFromCanvas = locationAtZeroAngle.Z - ViewContext.N_ViewCoordinates;
            PointD center = new PointD(locationAtZeroAngle.X, locationAtZeroAngle.Y - distanceFromCanvas / 2);

            double halfwidth = Math.Abs(center.Y - locationAtZeroAngle.Y);
            int length = Math.Max((int)(halfwidth * 2 + .5), 1);
            Rectangle r = new Rectangle((int)(center.X - halfwidth + .5), (int)(center.Y - halfwidth + .5), length, length);
            
            return r;
        }
    }
}
