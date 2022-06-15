using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Environment
{
    public class SurfacePatch
    {
        public SurfacePatch(List<PointSceneModel> points, int u, int v)
        {
            this.points = points;
        }

        public List<PointSceneModel> points;
        public uint u, v;
    }

    public class SurfaceSceneModel : ComplexSceneModel
    {
        private readonly LineRenderer _lines;
        private readonly Surface _surface;
        public Surface Surface => _surface;

        private List<SurfacePatch> _patches;

        public (int u, int v) Tesselation => _surface.TesselationLevel;
        public List<SurfacePatch> Patches => new List<SurfacePatch>(_patches);

        public SurfaceSceneModel(Surface geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, true)
        {
            _lines = new LineRenderer(new LineSet());
            _surface = geometry;
            _patches = new List<SurfacePatch>();

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

        public bool TryGetCorners(out List<PointSceneModel> corners)
        {
            corners = new List<PointSceneModel>();

            if (ControlPoints.Count == Surface.PointsU * Surface.PointsV && !Surface.Wrap)
            {
                corners.Add(ControlPoints[0]);
                corners.Add(ControlPoints[Surface.PointsU - 1]);
                corners.Add(ControlPoints[Surface.PointsU * Surface.PointsV - 1]);
                corners.Add(ControlPoints[(Surface.PointsU - 1) * Surface.PointsV]);
                return true;
            }

            return false;
        }

        public bool TryGetLineBetween(int i1, int i2, out List<PointSceneModel> border)
        {
            border = new List<PointSceneModel>();

            if (i2 < i1)
            {
                var tmp = i2;
                i2 = i1;
                i1 = tmp;
            }

            var i1u = i1 % Surface.PointsU;
            var i2u = i2 % Surface.PointsU;

            var i1v = i1 / Surface.PointsU;
            var i2v = i2 / Surface.PointsU;
                
            if (i1u == i2u)
            {
                for (int i = i1; i <= i2; i += Surface.PointsU)
                {
                    border.Add(ControlPoints[i]);
                }
                return true;
            }

            if (i1v == i2v)
            {
                for (int i = i1; i <= i2; i += 1)
                {
                    border.Add(ControlPoints[i]);
                }
                return true;
            }

            return false;
        }

        public void GeneratePatchData()
        {
            if (ControlPoints.Count != _surface.PointsU * _surface.PointsV)
                return;

            _patches.Clear();

            for (int patchV = 0; patchV < _surface.PatchesV; patchV++)
            {
                for (int patchU = 0; patchU < _surface.PatchesU; patchU++)
                {
                    var patchPoints = new List<PointSceneModel>();
                    var rowOffset = patchV * (_surface.PointsU * _surface.PatchOffset);
                    var columnOffset = patchU * _surface.PatchOffset;

                    for (int innerV = 0; innerV < _surface.PatchSize; innerV++)
                    {
                        for (int innerU = 0; innerU < _surface.PatchSize; innerU++)
                        {
                            var u = (columnOffset + innerU) % _surface.PointsU;
                            var v = rowOffset + innerV * _surface.PointsU;
                            patchPoints.Add(ControlPoints[u + v]);
                            
                        }
                    }
                    _patches.Add(new SurfacePatch(patchPoints, patchU, patchV));
                }
            }
        }

        private void SetTesselationLevels(int u, int v)
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

        public override bool TrySerialize(out IGeometryObject serialized)
        {
            if (Geometry is C0BezierSurface)
            {
                var patches = new List<Geom.BezierPatchC0>();

                var surf = Geometry as C0BezierSurface;

                foreach (var patch in Patches)
                {
                    var refs = new List<Geom.PointRef>();

                    foreach (var point in patch.points)
                    {
                        refs.Add(new Geom.PointRef() { Id = point.id });
                    }   

                    patches.Add(new Geom.BezierPatchC0()
                        {
                            controlPoints = refs.ToArray(),
                            Samples = new Types.Uint2(
                                (uint)surf.TesselationLevel.u,
                                (uint)surf.TesselationLevel.v)
                        });
                }

                var data = new Geom.BezierSurfaceC0()
                {
                    Id = id,
                    Name = Name,
                    Patches = patches.ToArray(),
                    Size = new Types.Uint2(
                        (uint)_surface.PatchesU,
                        (uint)_surface.PatchesV),
                    ParameterWrapped = new Types.Bool2(
                        _surface.Wrap, false)
                };

                serialized = data;
                return true;
            }

            if (Geometry is C2SplineSurface)
            {
                var patches = new List<Geom.BezierPatchC2>();

                var surf = Geometry as C2SplineSurface;

                foreach (var patch in Patches)
                {
                    var refs = new List<Geom.PointRef>();

                    foreach (var point in patch.points)
                    {
                        refs.Add(new Geom.PointRef() { Id = point.id });
                    }

                    patches.Add(new Geom.BezierPatchC2()
                    {
                        controlPoints = refs.ToArray(),
                        Samples = new Types.Uint2(
                                (uint)surf.TesselationLevel.u,
                                (uint)surf.TesselationLevel.v)
                    });
                }

                var data = new Geom.BezierSurfaceC2()
                {
                    Id = id,
                    Name = Name,
                    Patches = patches.ToArray(),
                    Size = new Types.Uint2(
                        (uint)_surface.PatchesU,
                        (uint)_surface.PatchesV),
                    ParameterWrapped = new Types.Bool2(
                        _surface.Wrap, false)
                };

                serialized = data;
                return true;
            }

            serialized = null;
            return false;
        }
    }
}
