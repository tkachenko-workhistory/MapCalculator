using Core;
using System.Collections.Generic;

namespace Kriging
{
    public class KrigingInterpolator : IInterpolator
    {
        public bool Interpolate(Point3D[][] map, List<Point3D> points, bool[][] calculatingMask, IInterpolationOptions options)
        {
            return false;
        }
    }
}