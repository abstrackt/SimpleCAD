using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Enums = SharpSceneSerializer.DTOs.Enums;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Utils
{
    public static class MathUtils
    {
        public static Types.Float3 AsFloat3(this Vector3 v)
        {
            return new Types.Float3(v.X, v.Y, v.Z);
        }

        public static Vector3 AsVector3(this Types.Float3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static (Vector3 translation, Vector3 xyzRot, Vector3 scale) DecomposeMatrix(Matrix4 trs)
        {
            var t = trs.Row3.Xyz;

            var s = new Vector3(trs.Row0.Xyz.Length, trs.Row1.Xyz.Length, trs.Row2.Xyz.Length);

            var R = new Matrix3(trs.Row0.Xyz.Normalized(), trs.Row1.Xyz.Normalized(), trs.Row2.Xyz.Normalized());

            var euler = Vector3.Zero;

            R.Transpose();

            if (R.M13 < +1)
            {
                if (R.M13 > -1)
                {
                    euler.Y = (float)Math.Asin(R.M13);
                    euler.X = (float)Math.Atan2(-R.M23, R.M33);
                    euler.Z = (float)Math.Atan2(-R.M12, R.M11);
                }
                else
                {
                    euler.Y = (float)(-Math.PI / 2);
                    euler.X = (float)Math.Atan2(R.M21, R.M22);
                    euler.Z = 0;
                }
            }
            else
            {
                euler.Y = (float)(Math.PI / 2);
                euler.X = (float)Math.Atan2(R.M21, R.M22);
                euler.Z = 0;
            }

            return (t, euler, s);
        }

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
