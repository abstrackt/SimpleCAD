using System;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Environment 
{
    public class BezierCurveSceneModel : ControlPointSceneModel
    {
        private LineRenderer _lines;
        private AdaptiveCurve _curve;

        protected override IGeometry Geometry
        {
            set
            {
                // Very important to do that here so that control point logic does not break.
                base.Geometry = value;

                if (value is AdaptiveCurve curve)
                {
                    _curve = curve;
                }
                else
                {
                    throw new InvalidOperationException("Cannot assign non-curve geometry to a bezier curve scene model");
                }
            }
        }

        public BezierCurveSceneModel(IControlPointGeometry geometry, string name) : base(geometry, name, PrimitiveType.Patches)
        {
            _lines = new LineRenderer();

            SetVertShader("bezier.vert");
            SetFragShader("bezier.frag");
            SetTesselationShader("bezier.tesc", "bezier.tese");
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
            _lines.RenderLines(_curve.GetPolygons());
        }

        public override void Render()
        {
            base.BeforeRendering();
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);
            GL.DrawArrays(type, 0, _vertexCount);
            AfterRendering();
        }
    }
}
