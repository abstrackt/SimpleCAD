using OpenTK.Mathematics;

namespace SimpleCAD.Source.Intersections
{
    public struct IntersectionData
    {
        public List<Vector3> points;
        public List<Solution> parameters;

        public static (int i1, int i2) FindClosestPair(IntersectionData d1, IntersectionData d2, int excl1 = -1, int excl2 = -1) 
        {
            var min = float.MaxValue;
            var i1 = 0;
            var i2 = 0;
            
            for (int i = 0; i < d1.points.Count; i++)
            {
                for (int j = 0; j < d2.points.Count; j++)
                {
                    var d = (d1.points[i] - d2.points[j]).Length;
                    if (d < min && (i != excl1 || j != excl2))
                    {
                        min = d;
                        i1 = i;
                        i2 = j;
                    }
                }
            }

            return (i1, i2);
        }

        public static Vector3 Extrapolate(IntersectionData d1, IntersectionData d2, bool s1, bool s2)
        {
            if (d1.points.Count < 2 || d2.points.Count < 2)
                return Vector3.Zero;

            var v1 = s1 ? d1.points[0] - d1.points[1] : d1.points[^1] - d1.points[^2];
            var v2 = s2 ? d2.points[0] - d2.points[1] : d2.points[^1] - d2.points[^2];

            var p11 = s1 ? d1.points[0] : d1.points[^1];
            var p12 = p11 + v1;

            var p21 = s2 ? d2.points[0] : d2.points[^1];
            var p22 = p21 + v2;

            var a1 = p12.Z - p11.Z;
            var b1 = p11.X - p12.X;
            var c1 = a1 * p11.X + b1 * p11.Z;

            var a2 = p22.Z - p21.Z;
            var b2 = p21.X - p22.X;
            var c2 = a2 * p21.X + b2 * p21.Z;

            float delta = a1 * b2 - a2 * b1;


            if (delta == 0)
                return Vector3.Zero;

            float x = (b2 * c1 - b1 * c2) / delta;
            float z = (a1 * c2 - a2 * c1) / delta;

            return new Vector3(x, p11.Y, z);
        }
    }
}
