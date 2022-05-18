using ImGuiNET;
using OpenTK;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Geometry
{
    public class C0BezierSurface : Surface, ISceneGUIElement
    {
        private bool _drawPolygon;

        public override int PatchSize => 4;
        public override int PatchOffset => 3;
        public override (int u, int v) TesselationLevel => (_tessU, _tessV);

        private int _tessU, _tessV;

        public C0BezierSurface(int patchesU, int patchesV, bool wrapV = false) : base(patchesU, patchesV, false, wrapV) 
        {
            _tessU = 4;
            _tessV = 4;
        } 
        
        public override List<Line> GetLines()
        {
            var lines = new List<Line>();

            if (_drawPolygon)
            {
                for (int u = 0; u < PointsU + (TopologyWrap.u ? 1 : 0); u++)
                {
                    var line = new Line();
                    line.color = ColorPalette.BernsteinPolygonColor;
                    for (int v = 0; v < PointsV + (TopologyWrap.v ? 1 : 0); v++)
                    {
                        var uWrap = u % PointsU;
                        var vWrap = v % PointsV;
                        line.points.Add(controlPoints[uWrap, vWrap]);
                    }
                    lines.Add(line);
                }

                for (int v = 0; v < PointsV + (TopologyWrap.v ? 1 : 0); v++)
                {
                    var line = new Line();
                    line.color = ColorPalette.BernsteinPolygonColor;
                    for (int u = 0; u < PointsU + (TopologyWrap.u ? 1 : 0); u++)
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
            if (!wrapU && !wrapV)
            {
                for (int u = 0; u < PointsU; u++)
                {
                    for (int v = 0; v < PointsV; v++)
                    {
                        var x = u * mU / ((float)PointsU - 1) - mU / 2f;
                        var z = v * mV / ((float)PointsV - 1) - mV / 2f;

                        points.Add(new Vector3(x, 0, z));
                    }
                }
            }

            // Create cylinder
            if (wrapV)
            {
                var n = 2 * (float)Math.PI / PatchesV;
                for (int u = 0; u < PointsU; u++)
                {
                    for (int v = 0; v < PatchesV; v++)
                    {
                        var x = u * mU / ((float)PointsU - 1) - mU / 2f;

                        Vector3 p1 = new Vector3(0, (float)Math.Cos(v * n), (float)Math.Sin(v * n));
                        Vector3 p4 = new Vector3(0, (float)Math.Cos((v + 1) * n), (float)Math.Sin((v + 1) * n));

                        var t1 = new Vector3(0, -p1.Z, p1.Y);
                        var t2 = new Vector3(0, p4.Z, -p4.Y);

                        Vector3 p2 = p1 + t1 * 4 / 3f * (float)Math.Tan(Math.PI / (2 * PatchesV));
                        Vector3 p3 = p4 + t2 * 4 / 3f * (float)Math.Tan(Math.PI / (2 * PatchesV));

                        p1 *= mV;
                        p2 *= mV;
                        p3 *= mV;
                        p4 *= mV;

                        p1.X = x;
                        p2.X = x;
                        p3.X = x;
                        p4.X = x;

                        points.Add(p1);
                        points.Add(p2);
                        points.Add(p3);
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
