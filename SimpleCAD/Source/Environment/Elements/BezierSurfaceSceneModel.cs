using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Environment
{
    public class BezierSurfaceSceneModel : ControlPointSceneModel
    {
        private LineRenderer _lines;
        private Surface _surface;

        // For model generation
        public int PointsU => _surface.PatchSize + (_surface.PatchOffset) * (_surface.PatchesU - 1);
        public int PointsV => _surface.PatchSize + (_surface.PatchOffset) * (_surface.PatchesV - 1);
        
        protected override IGeometry Geometry
        {
            set
            {
                // Very important to do that here so that control point logic does not break.
                base.Geometry = value;

                if (value is Surface surface)
                {
                    _surface = surface;
                }
                else
                {
                    throw new InvalidOperationException("Cannot assign non-surface geometry to a surface scene model");
                }
            }
        }

        public BezierSurfaceSceneModel(IControlPointGeometry geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, true)
        {
            _lines = new LineRenderer();

            SetVertShader("bezier.vert");
            SetFragShader("bezier.frag");
            SetTesselationShader("bezier.tesc", "bezier.tese");
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
            _lines.RenderLines(_surface.GetLines());
        }

        public override void Render()
        {
            base.BeforeRendering();
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.PatchParameter(PatchParameterInt.PatchVertices, _surface.PatchSize * _surface.PatchSize);
            GL.DrawArrays(type, 0, _vertexCount);
            AfterRendering();
        }
    }
}
