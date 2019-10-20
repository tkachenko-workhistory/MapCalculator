namespace Core
{
    /// <summary>
    /// Основа для точек в 3D, в идеале я хочу тут увидеть операции сравнения, нахождения расстояний, перегрузку операторов и тд
    /// </summary>
    public class Point3D
    {
        public Point3D (double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}