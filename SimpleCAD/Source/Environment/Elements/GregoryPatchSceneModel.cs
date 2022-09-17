using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Geometry.Impl;
using SimpleCAD.Source.Utils;
using System.Runtime.CompilerServices;

namespace SimpleCAD.Source.Environment
{
    public class GregoryPatchSceneModel : ComplexSceneModel
    {
        private GregoryPatch _patch;

        public GregoryPatchSceneModel(GregoryPatch geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, false)
        {
            _patch = geometry;

            SetVertShader(geometry.VertexShader);
            SetFragShader(geometry.FragShader);
            SetTesselationShader(geometry.TescShader, geometry.TeseShader);
        }

        private void SetTesselationLevels(int u, int v)
        {
            shader.SetInt("tess_u", u);
            shader.SetInt("tess_v", v);
        }

        public override void Render()
        {
            base.BeforeRendering();
            SetTesselationLevels(_patch.TesselationLevel.u, _patch.TesselationLevel.v);
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 20);
            GL.DrawArrays(type, 0, _vertexCount);
            AfterRendering();
        }

        public override bool TrySerialize(out IGeometryObject serialized)
        {
            serialized = null;
            return false;
        }
    }
}
