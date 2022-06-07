using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;

namespace SimpleCAD.Source.Utils
{
    public static class MathUtils
    {
        public static Vector3 Rad2Deg(Vector3 angles)
        {
            return new Vector3(angles.X * 180f / (float)Math.PI, angles.Y * 180f / (float)Math.PI, angles.Z * 180f / (float)Math.PI);
        }

        public static float Clamp(float t, float min, float max)
        {
            return Math.Min(Math.Max(t, min), max);
        }

        public static Vector3 DeCasteljau(int degree, float t, List<Vector3> controlPoints)
        {
            Vector3 f = new Vector3();

            if (controlPoints.Count != (degree + 1))
            {
                return f;
            }

            if (degree < 1)
            {
                return f;
            }

            List<Vector3> bezierPoints = new List<Vector3>();
            for (int level = degree; level >= 0; level--)
            {
                if (level == degree)
                {
                    for (int i = 0; i <= degree; i++)
                    {
                        bezierPoints.Add(controlPoints[i]);
                    }
                    continue;
                }

                int lastIdx = bezierPoints.Count;
                int levelIdx = level + 2;
                int idx = lastIdx - levelIdx;
                for (int i = 0; i <= level; i++)
                {
                    Vector3 pi = (1 - t) * bezierPoints[idx] + t * bezierPoints[idx + 1];
                    bezierPoints.Add(pi);
                    ++idx;
                }
            }

            int lastElmnt = bezierPoints.Count - 1;
            return bezierPoints[lastElmnt];
        }
    }
}
