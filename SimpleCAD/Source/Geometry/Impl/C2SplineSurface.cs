using ImGuiNET;
using OpenTK.Mathematics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class C2SplineSurface : Surface, ISceneGUIElement, IParametricSurface
    {
        private bool _drawPolygon;

        #region Draw data
        public override int PatchSize => 4;
        public override int PatchOffset => 1;
        public override (int u, int v) TesselationLevel => (_tessU, _tessV);

        public override string VertexShader => "splineSurface.vert";
        public override string FragShader => "splineSurface.frag";
        public override string TescShader => "splineSurface.tesc";
        public override string TeseShader => "splineSurface.tese";
        #endregion

        #region Parametric interface
        public bool WrapU => Wrap;
        public bool WrapV => false;
        public float RangeU => patchesU + (Wrap ? 2 : 0);
        public float RangeV => patchesV;
        #endregion

        private int _tessU, _tessV;

        public C2SplineSurface(int patchesU, int patchesV, bool wrapU, int tessU = 4, int tessV = 4) : base(patchesU, patchesV, wrapU)
        {
            _tessU = tessU;
            _tessV = tessV;
        }

        public override List<Line> GetLines()
        {
            var lines = new List<Line>();

            if (_drawPolygon)
            {
                for (int u = 0; u < PointsU + (Wrap ? 1 : 0); u++)
                {
                    var line = new Line();
                    line.color = ColorPalette.BernsteinPolygonColor;
                    for (int v = 0; v < PointsV; v++)
                    {
                        var uWrap = u % PointsU;
                        var vWrap = v % PointsV;
                        line.points.Add(controlPoints[uWrap, vWrap]);
                    }
                    lines.Add(line);
                }

                for (int v = 0; v < PointsV; v++)
                {
                    var line = new Line();
                    line.color = ColorPalette.BernsteinPolygonColor;
                    for (int u = 0; u < PointsU + (Wrap ? 1 : 0); u++)
                    {
                        var uWrap = u % PointsU;
                        var vWrap = v % PointsV;
                        line.points.Add(controlPoints[uWrap, vWrap]);
                    }
                    lines.Add(line);
                }
            }

            return lines;
        }

        public override List<Vector3> GenerateControlPoints(float mU, float mV)
        {
            List<Vector3> points = new List<Vector3>();

            // Create plane
            if (!wrapU)
            {
                for (int v = 0; v < PointsV; v++)
                {
                    for (int u = 0; u < PointsU; u++)
                    {
                        var x = u * mU / ((float)PointsU - 1) - mU / 2f;
                        var z = v * mV / ((float)PointsV - 1) - mV / 2f;

                        points.Add(new Vector3(x, 0, z));
                    }
                }
            }

            // Create cylinder
            if (wrapU)
            {
                var n = 2 * (float)Math.PI / PointsU;
                for (int v = 0; v < PointsV; v++)
                {
                    for (int u = 0; u < PointsU; u++)
                    {
                        var z = v * mV / ((float)PointsV - 1) - mV / 2f;

                        Vector3 p1 = new Vector3(mU * (float)Math.Cos(u * n), mU * (float)Math.Sin(u * n), 0);

                        p1.Z = z;

                        points.Add(p1);
                    }
                }
            }

            return points;
        }

        public void DrawElementGUI()
        {
            ImGui.Checkbox("Draw bezier grid", ref _drawPolygon);

            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Divisions (U)", ref _tessU, 0.1f, 1, 64);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Divisions (V)", ref _tessV, 0.1f, 1, 64);
        }

        public Vector3 Sample(float u, float v)
        {
            if (WrapU)
                u = (u + RangeU) % RangeU;

            int patchU = (int)u;
            int patchV = (int)v;

            if (u != 0 && u == (int)u)
                patchU--;
            if (v != 0 && v == (int)v)
                patchV--;

            // Clamp u and v to 0-1 range;
            while (u > 1)
            {
                u -= 1;
            }
            while (v > 1)
            {
                v -= 1;
            }

            var p = GetPatchPoints(patchU, patchV);

            Vector3 u0 = MathUtils.DeBoor(u, new List<Vector3>() { p[0, 0], p[1, 0], p[2, 0], p[3, 0] });
            Vector3 u1 = MathUtils.DeBoor(u, new List<Vector3>() { p[0, 1], p[1, 1], p[2, 1], p[3, 1] });
            Vector3 u2 = MathUtils.DeBoor(u, new List<Vector3>() { p[0, 2], p[1, 2], p[2, 2], p[3, 2] });
            Vector3 u3 = MathUtils.DeBoor(u, new List<Vector3>() { p[0, 3], p[1, 3], p[2, 3], p[3, 3] });

            Vector3 v0 = MathUtils.DeBoor(v, new List<Vector3>() { u0, u1, u2, u3 });

            return v0;
        }

        public Vector3 DerivU(float u, float v)
        {
            var h = 0.001f;

            var p1 = Sample(u, v);
            var p2 = Sample(u + h, v);

            var d = (p2 - p1) / h;

            return d;
        }

        public Vector3 DerivV(float u, float v)
        {
            var h = 0.001f;

            var p1 = Sample(u, v);
            var p2 = Sample(u, v + h);

            var d = (p2 - p1) / h;

            return d;
        }
    }
}
