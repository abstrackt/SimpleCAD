using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Intersections
{
    public struct Solution
    {
        public float u1, v1, u2, v2;

        public void Wrap(IParametricSurface s1, IParametricSurface s2)
        {
            if ((u1 < 0 || u1 > s1.RangeU) && !s1.WrapU)
            {
                u1 = Math.Clamp(u1, 0, s1.RangeU);
            }

            if ((v1 < 0 || v1 > s1.RangeV) && !s1.WrapV)
            {
                v1 = Math.Clamp(v1, 0, s1.RangeV);
            }

            if ((u2 < 0 || u2 > s2.RangeU) && !s2.WrapU)
            {
                u2 = Math.Clamp(u2, 0, s2.RangeU);
            }

            if ((v2 < 0 || v2 > s2.RangeV) && !s2.WrapV)
            {
                v2 = Math.Clamp(v2, 0, s2.RangeV);
            }
        }

        public Vector2 FirstScaled(float scaleU, float scaleV)
        {
            return new Vector2(u1 / scaleU, v1 / scaleV);
        }

        public Vector2 SecondScaled(float scaleU, float scaleV)
        {
            return new Vector2(u2 / scaleU, v2 / scaleV);
        }

        public static Solution operator -(Solution a, Solution b) => new Solution()
        {
            u1 = a.u1 - b.u1,
            u2 = a.u2 - b.u2,
            v1 = a.v1 - b.v1,
            v2 = a.v2 - b.v2
        };

        public static Solution operator +(Solution a, Solution b) => new Solution()
        {
            u1 = a.u1 + b.u1,
            u2 = a.u2 + b.u2,
            v1 = a.v1 + b.v1,
            v2 = a.v2 + b.v2
        };

        public static Solution operator *(Solution a, float b) => new Solution()
        {
            u1 = a.u1 * b,
            u2 = a.u2 * b,
            v1 = a.v1 * b,
            v2 = a.v2 * b
        };

        public static Solution operator /(Solution a, float b) => new Solution()
        {
            u1 = a.u1 / b,
            u2 = a.u2 / b,
            v1 = a.v1 / b,
            v2 = a.v2 / b
        };

        public void Clamp(IParametricSurface s1, IParametricSurface s2)
        {
            u1 = Math.Clamp(u1, 0, s1.RangeU);
            v1 = Math.Clamp(v1, 0, s1.RangeV);
            u2 = Math.Clamp(u2, 0, s2.RangeU);
            v2 = Math.Clamp(v2, 0, s2.RangeV);
        }
    }
}
