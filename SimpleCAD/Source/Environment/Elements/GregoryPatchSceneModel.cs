using OpenTK.Graphics.OpenGL4;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Geometry.Impl;

namespace SimpleCAD.Source.Environment
{
    public class GregoryPatchSceneModel : ComplexSceneModel
    {
        private LineRenderer _lines;
        private GregoryPatch _patch;

        public override bool HasParametricGeometry => false;
        public override IParametricSurface ParametricGeometry => null;

        public GregoryPatchSceneModel(GregoryPatch geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, false)
        {
            _patch = geometry;
            _lines = new LineRenderer(new LineSet());
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
            _lines.RenderLines(_patch.GetLines());
            AfterRendering();
        }

        public override bool TrySerialize(out IGeometryObject serialized)
        {
            serialized = null;
            return false;
        }
    }
}
