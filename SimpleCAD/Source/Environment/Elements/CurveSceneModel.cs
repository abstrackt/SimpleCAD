using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Environment 
{
    public class CurveSceneModel : ComplexSceneModel
    {
        private LineRenderer _lines;
        private AdaptiveCurve _curve;

        public CurveSceneModel(AdaptiveCurve geometry, string name) : base(geometry, name, PrimitiveType.Patches)
        {
            _lines = new LineRenderer(new LineSet());
            _curve = geometry;

            SetVertShader("bezierCurve.vert");
            SetFragShader("bezierCurve.frag");
            SetTesselationShader("bezierCurve.tesc", "bezierCurve.tese");
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
            _lines.RenderLines(_curve.GetLines());
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

        public override bool TrySerialize(out IGeometryObject serialized)
        {
            if (Geometry is C0BezierCurve)
            {
                var refs = new List<Geom.PointRef>();

                foreach (var point in ControlPoints)
                {
                    refs.Add(new Geom.PointRef() { Id = point.id });
                }

                var data = new Geom.BezierC0()
                {
                    Id = id,
                    Name = Name,
                    ControlPoints = refs.ToArray()
                };

                serialized = data;
                return true;
            }

            if (Geometry is C2SplineCurve)
            {
                var refs = new List<Geom.PointRef>();

                foreach (var point in ControlPoints)
                {
                    refs.Add(new Geom.PointRef() { Id = point.id });
                }

                var data = new Geom.BezierC2()
                {
                    Id = id,
                    Name = Name,
                    DeBoorPoints = refs.ToArray()
                };

                serialized = data;
                return true;
            }

            if (Geometry is C2InterpolatingCurve)
            {
                var refs = new List<Geom.PointRef>();

                foreach (var point in ControlPoints)
                {
                    refs.Add(new Geom.PointRef() { Id = point.id });
                }

                var data = new Geom.InterpolatedC2()
                {
                    Id = id,
                    Name = Name,
                    ControlPoints = refs.ToArray()
                };

                serialized = data;
                return true;
            }

            serialized = null;
            return false;
        }
    }
}
