using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScratchUtility
{
    // TODO:P0 PERF: See Vector3 GetHashCode() https://referencesource.microsoft.com/#System.Numerics/System/Numerics/Vector3.cs,44
    // TODO:P0 PERF: Implement GetHashCode, replace the String based hashes with custom Hash implmentation.  See https://learn.microsoft.com/en-us/archive/blogs/ericlippert/guidelines-and-rules-for-gethashcode and https://theburningmonk.com/2011/03/hashset-vs-list-vs-dictionary/
    public struct Coord
    {
        // TODO:P0:PERF: Cache/Hash/Dirty derived?  Are, or can Coord be immutable?  Consider
        // #ifdef DEBUG_PROPS to 

        // Build with DEBUG_USE_PROPS defined if/when need to verify/detect if callers are unexpectedly mutating values.
#if DEBUG_USE_PROPS
        public double X { get; private set; }   
        public double Y { get; private set; }
        public double Z { get; private set; }
#else
        // PERF: OO Sacrilege for performance sake.  Intentionally using member fields instead of
        // Properties to avoid method overhead.
        public double X; 
        public double Y; 
        public double Z;
#endif

        private double _length;
        private bool _lengthComputed;

        // private Coord() // TODO:P2: C#10 feature, Implement parameterless Struct constructor/initializer
        //{
        //    _length = double.NaN;
        //}

        public Coord(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            _length = double.NaN;
        }

        public Coord(double x, double y)
            : this()
        {
            X = x;
            Y = y;
            Z = 0;
            _length = double.NaN;
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
            var a = this;
            var b = (Coord)obj;

            if (b == null) return false;

            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            //throw new ArgumentException("PERF: Avoid boxing structs or handling unknown objects.  Instead, caller should intentionally call method with explicitly typed params.");
        }

        public bool Equals(Coord other, int toleranceDecimalPlaces)
        {
            // TODO:P0:PERF: Diff instead of Math.Round calls?

            // Return true if the fields match:
            return Math.Round(this.X, toleranceDecimalPlaces) == Math.Round(other.X, toleranceDecimalPlaces) &&
                Math.Round(this.Y, toleranceDecimalPlaces) == Math.Round(other.Y, toleranceDecimalPlaces) &&
                Math.Round(this.Z, toleranceDecimalPlaces) == Math.Round(other.Z, toleranceDecimalPlaces);
        }

        public Coord Clone(int toleranceDecimalPlaces)
        {
            return new Coord(
                Math.Round(this.X, toleranceDecimalPlaces),
                Math.Round(this.Y, toleranceDecimalPlaces),
                Math.Round(this.Z, toleranceDecimalPlaces));
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
        public string ToString  (int decimalPlaces)
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
            if (!_lengthComputed)
            {
                _length = Math.Sqrt(X * X + Y * Y + Z * Z);
                _lengthComputed = true;
            }

            return _length;
        }

        // <summary>Gets a unit vector in the direction of this Coord.</summary>
        public Coord CalcUnitVector()
        {
            double len = CalcLength();
            return new Coord(this.X / len, this.Y / len, this.Z / len);
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
        /// Returns (c1 - c2) * c3, this is an inline implementation of subtract and dot product.
        /// Helps reduce method calls and allocations.
        /// </summary>
        public static double DiffDot(Coord c1, Coord c2, Coord c3)
        {
            double dX = c1.X - c2.X;
            double dY = c1.Y - c2.Y;
            double dZ = c1.Z - c2.Z;

            return dX * c3.X + dY * c3.Y + dZ * c3.Z;
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

        /// <summary>
        /// Returns true if Vectors represented as Coords are Parallel in same or inverse direction.
        /// Used to determine if two vectors intersect.
        /// </summary>
        public static bool IsEqualOrInverse(ref Coord a, ref Coord b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                return true;

            if (a.X == -1 * b.X && a.Y == -1 * b.Y && a.Z == -1 * b.Z)
                return true;

            return false;
        }

        public Coord SetZ(double newZ)
        {
            this.Z = newZ;

            // Clear cache computed state
            _lengthComputed = false;

            return this;
        }
    }
}
