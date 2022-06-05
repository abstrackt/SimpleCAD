using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Environment
{
    public class SurfaceSceneModel : ControlPointSceneModel
    {
        private LineRenderer _lines;
        private Surface _surface;
        
        protected override IGeometry Geometry
        {
            set
            {
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

        public SurfaceSceneModel(Surface geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, true)
        {
            _lines = new LineRenderer();

            SetVertShader(geometry.VertexShader);
            SetFragShader(geometry.FragShader);
            SetTesselationShader(geometry.TescShader, geometry.TeseShader);
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
            _lines.RenderLines(_surface.GetLines());
        }

        public List<Vector3> GenerateControlPoints(float mU, float mV)
        {
            return _surface.GenerateControlPoints(mU, mV);
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
