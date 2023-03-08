using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScratchUtility
{
    // TODO:P0 PERF: Implement GetHashCode, replace the String based hashes with custom Hash implmentation.  See https://learn.microsoft.com/en-us/archive/blogs/ericlippert/guidelines-and-rules-for-gethashcode and https://theburningmonk.com/2011/03/hashset-vs-list-vs-dictionary/
    // TODO:P2 Very tempting to start caching computed state from Calc... methods.  But, should
    // instead Profile perf to see what's *REALLY* happening...
    public struct Coord
    {
        // TODO:P0:PERF: Cache/Hash/Dirty derived?  Are, or can Coord be immutable?  Consider
        // #ifdef DEBUG_PROPS to conditionally compile performant member fields to instead be
        // Properties to help identify unwanted mutate callers.

        // PERF:  OO Sacrilege, intentionally using member fields instead of Properties to avoid method overhead
        public double X; // { get; private set; }
        public double Y; // { get; private set; }
        public double Z; // { get; private set; }

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
            throw new ArgumentException("PERF: Avoid boxing structs or handling unknown objects.  Instead, caller should intentionally call method with explicitly typed params.");
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
    }
}
