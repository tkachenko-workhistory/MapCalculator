using System;

namespace Core
{
    /// <summary>
    /// Основа для точек в 3D, в идеале я хочу тут увидеть операции сравнения, нахождения расстояний, перегрузку операторов и тд
    /// </summary>
    public class Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        /// <summary>
        /// Расстояние между двумя точками (без учёта высоты)
        /// </summary>
        public double Distance(Point3D point)
        {
            return Math.Sqrt((point.X - X) * (point.X - X) +
                      (point.Y - Y) * (point.Y - Y));
        }

        public override bool Equals(object obj)
        {
            return Equals((Point3D)obj);
        }

        protected bool Equals(Point3D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            var x = p1.X + p2.X;
            var y = p1.Y + p2.Y;
            var z = p1.Z + p2.Z;
            return new Point3D(x, y, z);
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            var x = p1.X - p2.X;
            var y = p1.Y - p2.Y;
            var z = p1.Z - p2.Z;
            return new Point3D(x, y, z);
        }

        public static Point3D operator *(Point3D p1, double val)
        {
            var x = p1.X * val;
            var y = p1.Y * val;
            var z = p1.Z * val;
            return new Point3D(x, y, z);
        }

        public static Point3D operator *(double val, Point3D p1)
        {
            return p1 * val;
        }
    }
}