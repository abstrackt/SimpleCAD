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
        public SurfacePatch(List<PointSceneModel> points, int u, int v, int size)
        {
            this.points = points;
            this.u = u;
            this.v = v;
            this.size = size;

        }

        public List<PointSceneModel> points;
        public readonly int u, v;
        private readonly int size;

        public bool TryGetCorners(out List<PointSceneModel> corners)
        {
            corners = new List<PointSceneModel>();

            if (points.Count == size * size)
            {
                corners.Add(points[0]);
                corners.Add(points[size - 1]);
                corners.Add(points[size * size - 1]);
                corners.Add(points[(size - 1) * size]);
                return true;
            }

            return false;
        }

        public bool TryGetDerivativesBetween(int i1, int i2, out List<PointSceneModel> pts)
        {
            pts = new List<PointSceneModel>();

            if (i2 < i1)
            {
                var tmp = i2;
                i2 = i1;
                i1 = tmp;
            }

            var i1u = i1 % size;
            var i2u = i2 % size;
                           
            var i1v = i1 / size;
            var i2v = i2 / size;

            if (i1u == i2u && (i1u == 0 || i1u == size - 1))
            {
                if (i1u == size - 1)
                {
                    i1 -= 1;
                    i2 -= 1;
                }
                else
                {
                    i1 += 1;
                    i2 += 1;
                }

                for (int i = i1; i <= i2; i += size)
                {
                    pts.Add(points[i]);
                }
                return true;
            }

            if (i1v == i2v && (i1v == 0 || i1v == size - 1))
            {
                if (i1v == size - 1)
                {
                    i1 -= size;
                    i2 -= size;
                }
                else
                {
                    i1 += size;
                    i2 += size;
                }

                for (int i = i1; i <= i2; i += 1)
                {
                    pts.Add(points[i]);
                }
                return true;
            }

            return false;
        }

        public bool TryGetValuesBetween(int i1, int i2, out List<PointSceneModel> pts)
        {
            pts = new List<PointSceneModel>();

            if (i2 < i1)
            {
                var tmp = i2;
                i2 = i1;
                i1 = tmp;
            }

            var i1u = i1 % size;
            var i2u = i2 % size;
                           
            var i1v = i1 / size;
            var i2v = i2 / size;

            if (i1u == i2u)
            {
                for (int i = i1; i <= i2; i += size)
                {
                    pts.Add(points[i]);
                }
                return true;
            }

            if (i1v == i2v)
            {
                for (int i = i1; i <= i2; i += 1)
                {
                    pts.Add(points[i]);
                }
                return true;
            }

            return false;
        }
    }

    public class SurfaceSceneModel : ComplexSceneModel
    {
        private readonly LineRenderer _lines;
        private readonly Surface _surface;
        public Surface Surface => _surface;

        private List<SurfacePatch> _patches;

        public (int u, int v) Tesselation => _surface.TesselationLevel;
        public List<SurfacePatch> Patches => new List<SurfacePatch>(_patches);

        public override bool HasParametricGeometry => Geometry is IParametricSurface;
        public override IParametricSurface ParametricGeometry => (IParametricSurface)Geometry;

        public SurfaceSceneModel(Surface geometry, string name) : base(geometry, name, PrimitiveType.Patches, true, true)
        {
            _lines = new LineRenderer(new LineSet());
            _surface = geometry;
            _patches = new List<SurfacePatch>();

            SetPatchData(_surface.PatchesU, _surface.PatchesV);
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

        public override void ReplacePoint(PointSceneModel oldPoint, PointSceneModel newPoint)
        {
            base.ReplacePoint(oldPoint, newPoint);
            GeneratePatchData();
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
                    _patches.Add(new SurfacePatch(patchPoints, patchU, patchV, Surface.PatchSize));
                }
            }
        }

        private void SetTesselationLevels(int u, int v)
        {
            shader.SetInt("tess_u", u);
            shader.SetInt("tess_v", v);
        }

        private void SetPatchData(int u, int v)
        {
            shader.SetInt("patches_u", u);
            shader.SetInt("patches_v", v);
        }

        public override void Render()
        {
            base.BeforeRendering();
            SetTesselationLevels(_surface.TesselationLevel.u, _surface.TesselationLevel.v);
            shader.Use();
            if (_texture != null)
            {
                shader.SetSampler("mask", 0);
                _texture.Use(TextureUnit.Texture0);
            }
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
