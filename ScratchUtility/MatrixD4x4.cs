using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ScratchUtility
{
    // Based on https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Matrix4x4.cs
    /// <summary>
    /// A structure encapsulating a 4x4 matrix.
    /// </summary>
    public struct MatrixD4x4 : IEquatable<MatrixD4x4>
    {
        #region Public Fields
        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11;
        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12;
        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public double M13;
        /// <summary>
        /// Value at row 1, column 4 of the matrix.
        /// </summary>
        public double M14;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21;
        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22;
        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        public double M23;
        /// <summary>
        /// Value at row 2, column 4 of the matrix.
        /// </summary>
        public double M24;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        public double M31;
        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        public double M32;
        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        public double M33;
        /// <summary>
        /// Value at row 3, column 4 of the matrix.
        /// </summary>
        public double M34;

        /// <summary>
        /// Value at row 4, column 1 of the matrix.
        /// </summary>
        public double M41;
        /// <summary>
        /// Value at row 4, column 2 of the matrix.
        /// </summary>
        public double M42;
        /// <summary>
        /// Value at row 4, column 3 of the matrix.
        /// </summary>
        public double M43;
        /// <summary>
        /// Value at row 4, column 4 of the matrix.
        /// </summary>
        public double M44;
        #endregion Public Fields

        private static readonly MatrixD4x4 _identity = new MatrixD4x4
        (
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        );

        /// <summary>
        /// Returns the multiplicative identity matrix.
        /// </summary>
        public static MatrixD4x4 Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Returns whether the matrix is the identity matrix.
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                return M11 == 1f && M22 == 1f && M33 == 1f && M44 == 1f && // Check diagonal element first for early out.
                                    M12 == 0f && M13 == 0f && M14 == 0f &&
                       M21 == 0f && M23 == 0f && M24 == 0f &&
                       M31 == 0f && M32 == 0f && M34 == 0f &&
                       M41 == 0f && M42 == 0f && M43 == 0f;
            }
        }

        /// <summary>
        /// Gets or sets the translation component of this matrix.
        /// </summary>
        public Vector3 Translation
        {
            get
            {
                return new Vector3((float)M41, (float)M42, (float)M43);
            }
            set
            {
                M41 = value.X;
                M42 = value.Y;
                M43 = value.Z;
            }
        }

        /// <summary>
        /// Constructs a MatrixD4x4 from the given components.
        /// </summary>
        public MatrixD4x4(double m11, double m12, double m13, double m14,
                         double m21, double m22, double m23, double m24,
                         double m31, double m32, double m33, double m34,
                         double m41, double m42, double m43, double m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;

            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;

            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        // ...


        /// <summary>
        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The matrix to compare this instance to.</param>
        /// <returns>True if the matrices are equal; False otherwise.</returns>
        public bool Equals(MatrixD4x4 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 && // Check diagonal element first for early out.
                                        M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                    M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
                    M31 == other.M31 && M32 == other.M32 && M34 == other.M34 &&
                    M41 == other.M41 && M42 == other.M42 && M43 == other.M43);
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is MatrixD4x4)
            {
                return Equals((MatrixD4x4)obj);
            }

            return false;
        }

        // ...

        /// <summary>
        /// Returns a String representing this matrix instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;

            return String.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2} M14:{3}}} {{M21:{4} M22:{5} M23:{6} M24:{7}}} {{M31:{8} M32:{9} M33:{10} M34:{11}}} {{M41:{12} M42:{13} M43:{14} M44:{15}}} }}",
                                 M11.ToString(ci), M12.ToString(ci), M13.ToString(ci), M14.ToString(ci),
                                 M21.ToString(ci), M22.ToString(ci), M23.ToString(ci), M24.ToString(ci),
                                 M31.ToString(ci), M32.ToString(ci), M33.ToString(ci), M34.ToString(ci),
                                 M41.ToString(ci), M42.ToString(ci), M43.ToString(ci), M44.ToString(ci));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
                   M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
        }
    }
}
