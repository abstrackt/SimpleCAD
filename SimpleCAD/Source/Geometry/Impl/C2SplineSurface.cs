using ImGuiNET;
using OpenTK;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class C2SplineSurface : Surface, ISceneGUIElement
    {
        private bool _drawPolygon;

        public override int PatchSize => 4;
        public override int PatchOffset => 1;
        public override (int u, int v) TesselationLevel => (_tessU, _tessV);

        public override string VertexShader => "splineSurface.vert";
        public override string FragShader => "splineSurface.frag";
        public override string TescShader => "splineSurface.tesc";
        public override string TeseShader => "splineSurface.tese";

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
    }
}
