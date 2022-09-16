using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry.Impl;

namespace SimpleCAD.Source.Environment
{
    public class GregoryPatchSceneModel : ComplexSceneModel
    {
        private GregoryPatch _patch;

        public GregoryPatchSceneModel(GregoryPatch geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, false)
        {
            _patch = geometry;
        }

        public void GenerateGregoryPatch(List<List<PointSceneModel>> e, List<List<PointSceneModel>> d) 
        {
            if (e.Count != 3 || d.Count != 3)
            {
                return;
            }

            Vector3[] p = new Vector3[49];
            Vector3[] dp = new Vector3[21];

            for (int i = 0; i < 3; i++)
            {
                var p1 = e[i][0].Position;
                var p2 = e[i][1].Position;
                var p3 = e[i][2].Position;
                var p4 = e[i][3].Position;

                Vector3[] half = new Vector3[6];
                half[0] = (p1 + p2) * 0.5f;
                half[1] = (p2 + p3) * 0.5f;
                half[2] = (p3 + p4) * 0.5f;
                half[3] = (half[0] + half[1]) * 0.5f;
                half[4] = (half[1] + half[2]) * 0.5f;
                half[5] = (half[3] + half[4]) * 0.5f;

                p[6 * i] = p1;
                p[6 * i + 1] = half[0];
                p[6 * i + 2] = half[3];
                p[6 * i + 3] = half[5];
                p[6 * i + 4] = half[4];
                p[6 * i + 5] = half[2];
                p[(6 * i + 6) % 18] = p4;
            }

            for (int i = 0; i < 3; i++)
            {
                var p1 = d[i][0].Position;
                var p2 = d[i][1].Position;
                var p3 = d[i][2].Position;
                var p4 = d[i][3].Position;

                Vector3[] half = new Vector3[6];
                half[0] = (p1 + p2) * 0.5f;
                half[1] = (p2 + p3) * 0.5f;
                half[2] = (p3 + p4) * 0.5f;
                half[3] = (half[0] + half[1]) * 0.5f;
                half[4] = (half[1] + half[2]) * 0.5f;
                half[5] = (half[3] + half[4]) * 0.5f;

                dp[7 * i] = p1;
                dp[7 * i + 1] = half[0];
                dp[7 * i + 2] = half[3];
                dp[7 * i + 3] = half[5];
                dp[7 * i + 4] = half[4];
                dp[7 * i + 5] = half[2];
                dp[7 * i + 6] = p4;
            }

            for (int i = 0; i < 3; i++)
            {
                p[18 + 7 * i] = p[6 * i + 1] + (p[6 * i + 1] - dp[7 * i + 1]); 
                p[19 + 7 * i] = p[6 * i + 2] + (p[6 * i + 2] - dp[7 * i + 2]); 
                p[21 + 7 * i] = p[6 * i + 3] + (p[6 * i + 3] - dp[7 * i + 3]); 
                p[23 + 7 * i] = p[6 * i + 4] + (p[6 * i + 4] - dp[7 * i + 4]); 
                p[24 + 7 * i] = p[6 * i + 5] + (p[6 * i + 5] - dp[7 * i + 5]); 
            }

            //TODO: Finish remaining points

            var pList = p.ToList();

            _patch.SetVirtualPoints(pList);
        }

        public override bool TrySerialize(out IGeometryObject serialized)
        {
            serialized = null;
            return false;
        }
    }
}
