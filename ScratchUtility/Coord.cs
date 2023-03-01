using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScratchUtility
{
    // TODO:P2 Very tempting to start caching computed state from Calc... methods.  But, should
    // instead Profile perf to see what's *REALLY* happening...
    public struct Coord
    {
        // PERF: Using fields instead of Properties to avoid  method overhead
        // TODO: PERF Cache/Hash/Dirty derived?  Are/Should Coord be immutable?

        public double X;
        public double Y;
        public double Z;

        public Coord(double x, double y, double z)
            :this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coord(double x, double y)
            :this()
        {
            X = x;
            Y = y;
            Z = 0;
        }

        public static Coord operator *(Coord c, double rhs)
        {
            return new Coord(c.X * rhs, c.Y * rhs, c.Z * rhs);
        }
        public static Coord operator *(double lhs, Coord c)
        {
            return c * lhs;
        }
        public static Coord operator /(Coord c, double rhs)
        {
            return new Coord(c.X / rhs, c.Y / rhs, c.Z / rhs);
        }
        public static Coord operator -(Coord c, Coord rhs)
        {
            return new Coord(c.X - rhs.X, c.Y - rhs.Y, c.Z - rhs.Z);
        }
        public static Coord operator +(Coord c, Coord rhs)
        {
            return new Coord(c.X + rhs.X, c.Y + rhs.Y, c.Z + rhs.Z);
        }
        public static bool operator ==(Coord a, Coord b)
        {
            //// If both are null, or both are same instance, return true.
            //if (System.Object.ReferenceEquals(a, b))
            //{
            //    throw new Exception("Not expected");
            //    //return true;
            //}

            //// If one is null, but not both, return false.
            //if (((object)a == null) || ((object)b == null))
            //{
            //    throw new Exception("Not expected");
            //    //return false;
            //}

            // Return true if the fields match:
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;

            //if (a.X != b.X) return false;
            //if (a.Y != b.Y) return false;
            //if (a.Z != b.Z) return false;

            //return true;
        }

        public static bool operator !=(Coord a, Coord b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            throw new ArgumentException("PERF: PERF: Avoid boxing structs or handling unknown objects.  Instead, caller should intentionally call method with explicitly typed params.");
            //if (obj.GetType() == typeof(Coord))
            //{
            //    return this == (Coord)obj;
            //}
            //else
            //    return false;
        }

        public bool Equals(Coord other, int toleranceDecimalPlaces)
        {
            // TODO: Diff

            // Return true if the fields match:
            return Math.Round(this.X, toleranceDecimalPlaces) == Math.Round(other.X, toleranceDecimalPlaces) &&
                Math.Round(this.Y, toleranceDecimalPlaces) == Math.Round(other.Y, toleranceDecimalPlaces) &&
                Math.Round(this.Z, toleranceDecimalPlaces) == Math.Round(other.Z, toleranceDecimalPlaces);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
        public string ToString(int decimalPlaces)
        {
            return "(" + X.ToString("N" + decimalPlaces) + ", " + Y.ToString("N" + decimalPlaces) + ", " + Z.ToString("N" + decimalPlaces) + ")";
        }


        public Matrix ToVectorCol(bool includeBottom1)
        {
            if (includeBottom1)
            {
                Matrix v = new Matrix(4, 1);
                v[0, 0] = X;
                v[1, 0] = Y;
                v[2, 0] = Z;
                v[3, 0] = 1.0;
                return v;
            }
            else
            {
                Matrix v = new Matrix(3, 1);
                v[0, 0] = X;
                v[1, 0] = Y;
                v[2, 0] = Z;
                return v;
            }

        }

        // Was .Length property, changed to method since it's doing work and the property cannot be
        // serialized.  Intentionally making computationally expensive 'getters' methods to help
        // hint to callers about the CPU cost.
        public double CalcLength()
        {
            // TODO:P0 Cached value?
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // <summary>Gets a unit vector in the direction of this Coord.</summary>
        public Coord CalcUnitVector()
        {
            // TODO:P0 Cached value?
            return this / CalcLength();
        }

        /// <summary>Returns the Cross Product of this and rhs.</summary>
        /// <param name="rhs">The right-hand-side Coord.</param>
        /// <returns>The Cross Product of this and rhs.</returns>
        public Coord CrossProduct(Coord rhs)
        {
            Coord retVal = new Coord
                (
                this.Y * rhs.Z - this.Z * rhs.Y,
                this.Z * rhs.X - this.X * rhs.Z,
                this.X * rhs.Y - this.Y * rhs.X
                );
            return retVal;
        }
        /// <summary>Returns the Dot Product of this and rhs.</summary>
        /// <param name="rhs">The right-hand-side Coord.</param>
        /// <returns>The Dot Product of this and rhs.</returns>
        public double DotProduct(Coord rhs)
        {
            return this.X * rhs.X + this.Y * rhs.Y + this.Z * rhs.Z;
        }

        /// <summary>
        /// Returns a new PointD object with this Coord's X and Y value. The Z value is eliminated.
        /// </summary>
        public PointD ToPointD()
        {
            return new PointD(X, Y);
        }
        /// <summary>
        /// Returns a new PointF object with this Coord's X and Y value. The Z value is eliminated.
        /// </summary>
        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }
        public bool IsValid()
        {
            return !(double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z));
        }
    }
}
