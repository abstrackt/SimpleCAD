using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Intersections
{
    public class IntersectionManager
    {
        private const float PRECISION = 0.001f;
        private const int MAX_ITERATIONS = 6;

        private static IntersectionManager instance = null;
        public static IntersectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IntersectionManager();
                }
                return instance;
            }
        }

        private struct Solution
        {
            public float u1, v1, u2, v2;
        }

        public Vector3 FindIntersection(IParametricSurface s1, IParametricSurface s2)
        {
            var sol = FindStartingPoint(s1, s2);

            return s1.Sample(sol.u1, sol.v1);
        }

        private Solution FindStartingPoint(IParametricSurface s1, IParametricSurface s2)
        {
            Solution min = new Solution()
            {
                u1 = 0,
                v1 = 0,
                u2 = 0,
                v2 = 0
            };

            var it = 0;
            var minDist = float.MaxValue;
            var bestMin = min;
            var bestSol = min;

            float incrU1 = s1.RangeU;
            float incrV1 = s1.RangeV;
            float incrU2 = s2.RangeU;
            float incrV2 = s2.RangeV;

            while (it < MAX_ITERATIONS && minDist > PRECISION)
            {
                var currMin = bestMin;

                incrU1 /= 4;
                incrV1 /= 4;
                incrU2 /= 4;
                incrV2 /= 4;

                for (int u1 = 0; u1 < 4; u1++)
                {
                    var minU1 = currMin.u1 + u1 * incrU1;
                    var maxU1 = minU1 + incrU1;

                    for (int v1 = 0; v1 < 4; v1++)
                    {
                        var minV1 = currMin.v1 + v1 * incrV1;
                        var maxV1 = minV1 + incrV1;

                        for (int u2 = 0; u2 < 4; u2++)
                        {
                            var minU2 = currMin.u2 + u2 * incrU2;
                            var maxU2 = minU2 + incrU2;

                            for (int v2 = 0; v2 < 4; v2++)
                            {
                                var minV2 = currMin.v2 + v2 * incrV2;
                                var maxV2 = minV2 + incrV2;

                                var sol = new Solution()
                                {
                                    u1 = (minU1 + maxU1) / 2,
                                    v1 = (minV1 + maxV1) / 2,
                                    u2 = (minU2 + maxU2) / 2,
                                    v2 = (minV2 + maxV2) / 2
                                };

                                var p1 = s1.Sample(sol.u1, sol.v1);
                                var p2 = s2.Sample(sol.u2, sol.v2);

                                var dist = (p2 - p1).Length;

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    bestMin = new Solution()
                                    {
                                        u1 = minU1,
                                        v1 = minV1,
                                        u2 = minU2,
                                        v2 = minV2
                                    };
                                    bestSol = sol;
                                }
                            }
                        }
                    }
                }
                it++;
            }

            return bestSol;
        }
    }
}
