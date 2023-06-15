using ImGuiNET;
using OpenTK.Mathematics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;

namespace SimpleCAD.Source.Geometry.Impl
{
    public class GregoryPatch : IControlPointGeometry, ISceneGUIElement, IColorable
    {
        public (int u, int v) TesselationLevel => (_tessU, _tessV);

        public string VertexShader => "gregory.vert";
        public string FragShader => "gregory.frag";
        public string TescShader => "gregory.tesc";
        public string TeseShader => "gregory.tese";

        private Color4 _color;
        private List<Vector3> _gregoryPoints;

        private List<List<Vector3>> _edge;
        private List<List<Vector3>> _deriv;

        private List<int> _patchIndices;
        private int _tessU, _tessV;

        private List<Line> _lines;
        private bool _drawVectors;
        private const float EPS = 0.001f;

        public GregoryPatch(int tessU = 4, int tessV = 4) : base()
        {
            _gregoryPoints = new List<Vector3>();
            _edge = new List<List<Vector3>>();
            _deriv = new List<List<Vector3>>();

            _tessU = tessU;
            _tessV = tessV;

            _patchIndices = new List<int>()
            {
                15, 35, 46, 48,
                16, 36, 37, 47, 39, 40,
                17, 38, 18, 20, 19, 21,
                0, 1, 2, 3,

                3, 21, 40, 48,
                4, 22, 23, 41, 42, 43,
                5, 24, 25, 27, 26, 28,
                6, 7, 8, 9,

                9, 28, 43, 48,
                10, 29, 30, 44, 45, 46,
                11, 31, 32, 34, 33, 35,
                12, 13, 14, 15
            };

            _lines = new List<Line>();
        }

        public void DrawElementGUI()
        {
            ImGui.Checkbox("Show border vectors", ref _drawVectors);

            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Divisions (U)", ref _tessU, 0.1f, 1, 64);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Divisions (V)", ref _tessV, 0.1f, 1, 64);
        }

        public bool GeometryChanged() => false;

        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            var points = GenerateGregoryPatch(_edge, _deriv);

            if (points.Count != 49)
                return (new Vertex[] { }, new uint[] { });

            var vertices = new List<Vertex>();

            foreach (var index in _patchIndices)
            {
                vertices.Add(new Vertex(points[index], _color));
            }

            return (vertices.ToArray(), new uint[] { });
        }

        private List<Vector3> GenerateGregoryPatch(List<List<Vector3>> e, List<List<Vector3>> d)
        {
            if (e.Count != 3 || d.Count != 3)
            {
                return new List<Vector3>();
            }

            var rd = new Random();

            // To prevent numerical errors
            for (int i = 0; i < e.Count; i++)
            {
                for (int j = 0; j < e[i].Count; j++)
                {
                    e[i][j] += new Vector3(
                        (float)rd.NextDouble() - 0.5f,
                        (float)rd.NextDouble() - 0.5f,
                        (float)rd.NextDouble() - 0.5f) * EPS;
                }
            }

            for (int i = 0; i < d.Count; i++)
            {
                for (int j = 0; j < d[i].Count; j++)
                {
                    d[i][j] += new Vector3(
                        (float)rd.NextDouble() - 0.5f,
                        (float)rd.NextDouble() - 0.5f,
                        (float)rd.NextDouble() - 0.5f) * EPS;
                }
            }

            var p = new Vector3[49];
            var dp = new Vector3[21];
            var r = new Vector3[3];

            _lines.Clear();
            var col = ColorPalette.GregoryVectorsColor;

            // First layer
            for (int i = 0; i < 3; i++)
            {
                var p1 = e[i][0];
                var p2 = e[i][1];
                var p3 = e[i][2];
                var p4 = e[i][3];

                var half = new Vector3[6];
                half[0] = (p1 + p2) * 0.5f;
                half[1] = (p2 + p3) * 0.5f;
                half[2] = (p3 + p4) * 0.5f;
                half[3] = (half[0] + half[1]) * 0.5f;
                half[4] = (half[1] + half[2]) * 0.5f;
                half[5] = (half[3] + half[4]) * 0.5f;

                var line = new Line();
                line.color = col;

                line.points.Add(p1);
                line.points.Add(half[0]);
                line.points.Add(half[3]);
                line.points.Add(half[5]);
                line.points.Add(half[4]);
                line.points.Add(half[2]);
                line.points.Add(p4);

                _lines.Add(line);

                p[6 * i] = p1;
                p[6 * i + 1] = half[0];
                p[6 * i + 2] = half[3];
                p[6 * i + 3] = half[5];
                p[6 * i + 4] = half[4];
                p[6 * i + 5] = half[2];
                p[(6 * i + 6) % 18] = p4;
            }

            // Corresponding derivatives
            for (int i = 0; i < 3; i++)
            {
                var p1 = d[i][0];
                var p2 = d[i][1];
                var p3 = d[i][2];
                var p4 = d[i][3];

                var half = new Vector3[6];
                half[0] = (p1 + p2) * 0.5f;
                half[1] = (p2 + p3) * 0.5f;
                half[2] = (p3 + p4) * 0.5f;
                half[3] = (half[0] + half[1]) * 0.5f;
                half[4] = (half[1] + half[2]) * 0.5f;
                half[5] = (half[3] + half[4]) * 0.5f;

                dp[7 * i] = p1;
                dp[7 * i + 1] = half[0];
                dp[7 * i + 2] = half[3];
                dp[7 * i + 3] = half[5];
                dp[7 * i + 4] = half[4];
                dp[7 * i + 5] = half[2];
                dp[7 * i + 6] = p4;
            }

            for (int i = 0; i < 3; i++)
            {
                r[i] = p[3 + 6 * i] + 1.5f * (p[3 + 6 * i] - dp[3 + 7 * i]);
            }

            // Middle point
            p[48] = (r[0] + r[1] + r[2]) / 3f;

            // Common inner borders
            for (int i = 0; i < 3; i++)
            {
                p[3 * i + 40] = r[i] + (p[48] - r[i]) / 3f;
            }

            // Second layer
            for (int i = 0; i < 3; i++)
            {
                p[7 * i + 18] = p[6 * i + 1] + (p[6 * i + 1] - dp[7 * i + 1]);
                p[7 * i + 19] = p[6 * i + 2] + (p[6 * i + 2] - dp[7 * i + 2]);
                p[7 * i + 21] = p[6 * i + 3] + (p[6 * i + 3] - dp[7 * i + 3]);
                p[7 * i + 23] = p[6 * i + 4] + (p[6 * i + 4] - dp[7 * i + 4]);
                p[7 * i + 24] = p[6 * i + 5] + (p[6 * i + 5] - dp[7 * i + 5]);

                var l1 = new Line();
                l1.color = col;
                l1.points.Add(p[6 * i + 1]);
                l1.points.Add(p[7 * i + 18]);
                _lines.Add(l1);

                var l2 = new Line();
                l2.color = col;
                l2.points.Add(p[6 * i + 2]);
                l2.points.Add(p[7 * i + 19]);
                _lines.Add(l2);

                var l3 = new Line();
                l3.color = col;
                l3.points.Add(p[6 * i + 3]);
                l3.points.Add(p[7 * i + 21]);
                _lines.Add(l3);

                var l4 = new Line();
                l4.color = col;
                l4.points.Add(p[6 * i + 4]);
                l4.points.Add(p[7 * i + 23]);
                _lines.Add(l4);

                var l5 = new Line();
                l5.color = col;
                l5.points.Add(p[6 * i + 5]);
                l5.points.Add(p[7 * i + 24]);
                _lines.Add(l5);
            }

            int[] ha2 = new[] { 46, 40, 43 };
            int[] hb2 = new[] { 43, 46, 40 };

            // Solve for remaining inner points
            for (int i = 0; i < 3; i++)
            {
                var a0 = p[6 * i + 3] - p[6 * i + 2];
                var a1 = p[6 * i + 4] - p[6 * i + 3];
                var b0 = p[48] - p[ha2[i]];
                var b1 = p[hb2[i]] - p[48];

                var x0 = (a0 + a1) / 2f;
                var x2 = (b0 + b1) / 2f;
                var x1 = (x0 + x2) / 2f;

                var y0 = p[7 * i + 21] - p[6 * i + 3];
                var y1 = p[3 * i + 40] - p[7 * i + 21];
                var y2 = p[48] - p[3 * i + 40];

                var (c0, d0) = Solve(x0, y0, a1);
                var (c1, d1) = Solve(x2, y2, b1);

                var f1 = (float t) => MathUtils.DeCasteljau(2, t, new List<Vector3>() { x0, x1, x2 });
                var f2 = (float t) => MathUtils.DeCasteljau(2, t, new List<Vector3>() { y0, y1, y2 });

                var k = (float t) => c0 * (1 - t) + c1 * t;
                var h = (float t) => d0 * (1 - t) + d1 * t;

                var v2 = k(2 / 3f) * f1(2 / 3f) + h(2 / 3f) * f2(2 / 3f);

                p[3 * i + 39] = p[3 * i + 40] - v2;
                p[3 * i + 41] = p[3 * i + 40] + v2;

                p[7 * i + 20] = p[7 * i + 19];
                p[7 * i + 22] = p[7 * i + 23];
            }

            var list = p.ToList();

            SetVirtualPoints(list);
            return list;
        }

        public List<Line> GetLines()
        {
            if (_drawVectors)
            {
                return _lines;
            }
            else
            {
                return new List<Line>();
            }

        }

        // Find linear combination such that a * x + b * y = z
        private (float a, float b) Solve(Vector3 x, Vector3 y, Vector3 z)
        {
            Matrix3 m = new Matrix3(x, y, z);
            m.Transpose();
            m.Row0 = m.Row0 / m.M11;
            m.Row1 = m.Row1 - m.Row0 * (m.M21);
            m.Row2 = m.Row2 - m.Row0 * (m.M31);

            m.Row1 = m.Row1 / m.M22;
            m.Row2 = m.Row2 - m.Row1 * (m.M32);

            m.Transpose();
            var b = m.M32;
            var a = m.M31 - m.M21 * b;

            return (a, b);
        }


        public List<Vector3> GetVirtualPoints()
        {
            return _gregoryPoints;
        }

        public void SetVirtualPoints(List<Vector3> positions)
        {
            _gregoryPoints = positions;
        }

        public List<Vector3> MoveVirtualPoint(int i, Vector3 position)
        {
            return new List<Vector3>();
        }

        public void SetControlPoints(List<Vector3> positions)
        {
            _edge.Clear();
            _deriv.Clear();
            if (positions.Count == 24) {
                for (int i = 0; i < 3; i++)
                {
                    _edge.Add(positions.GetRange(4 * i, 4));
                }
                for (int i = 0; i < 3; i++)
                {
                    _deriv.Add(positions.GetRange(12 + 4 * i, 4));
                }
            }
        }

        public Color4 GetColor()
        {
            return _color;
        }

        public void SetColor(Color4 color)
        {
            if (color != _color)
            {
                _color = color;
            }
        }

        public void OnTransformChanged(Matrix4 transform)
        {
            // Not required
        }

    }
}
