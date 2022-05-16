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
        public int PointsU => _surface.PointsU;
        public int PointsV => _surface.PointsV;
        
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

            SetVertShader("bezierSurface.vert");
            SetFragShader("bezierSurface.frag");
            SetTesselationShader("bezierSurface.tesc", "bezierSurface.tese");
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
            _lines.RenderLines(_surface.GetLines());
        }

        public void SetTesselationLevels(int u, int v)
        {
            shader.SetInt("tess_u", u);
            shader.SetInt("tess_v", v);
        }

        public override void Render()
        {
            base.BeforeRendering();
            SetTesselationLevels(_surface.TesselationLevel.u, _surface.TesselationLevel.v);
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.PatchParameter(PatchParameterInt.PatchVertices, _surface.PatchSize * _surface.PatchSize);
            GL.DrawArrays(type, 0, _vertexCount);
            AfterRendering();
        }
    }
}
