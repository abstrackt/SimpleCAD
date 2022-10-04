using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    class Torus : IGeometry, ISceneGUIElement, IColorable, IParametricSurface
    {
        #region Draw data
        public string VertexShader => "torus.vert";
        public string FragShader => "torus.frag";
        public string TescShader => "";
        public string TeseShader => "";

        public int resU, resV;
        public float R, r;
        public Color4 color;

        public const int MinimumResolution = 3;

        private bool _changed = false;
        private Matrix4 _transform = Matrix4.Identity;
        #endregion

        public ObjectType DTOType => ObjectType.torus;

        #region Parametric interface
        public bool WrapU => true;
        public bool WrapV => true;

        public float RangeU => 1;
        public float RangeV => 1;
        #endregion

        public Torus(int resU, int resV, float R, float r, Color4 color)
        {
            this.resU = Math.Max(resU, MinimumResolution);
            this.resV = Math.Max(resV, MinimumResolution);
            this.R = R;
            this.r = r;
            this.color = color;
        }

        public (Vertex[] vertices, uint[] indices) GetMesh() 
        {
            int nVerts = resU * resV;
            int nPrimitives = nVerts * 2;
            int nIndices = nPrimitives * 3;

            float pi = (float)Math.PI;

            List<Vertex> verticesList = new List<Vertex>(nVerts);
            List<uint> indicesList = new List<uint>(nIndices);

            float deltaU = 2 * pi / resU;
            float deltaV = 2 * pi / resV;

            float x = 0;
            float y = 0;
            float z = 0;

            float rInner = (R - r) / 2;
            float rMiddle = R - rInner;

            for (int i = 0; i < resU + 1; i++)
            {
                var cosSeg = (float)Math.Cos(i * deltaU);
                var sinSeg = (float)Math.Sin(i * deltaU);

                for (int j = 0; j < resV + 1; j++)
                {
                    var cosTube = (float)Math.Cos(j * deltaV);
                    var sinTube = (float)Math.Sin(j * deltaV);

                    x = (rMiddle + rInner * cosTube) * cosSeg;
                    y = rInner * sinTube;
                    z = (rMiddle + rInner * cosTube) * sinSeg;

                    var v = new Vertex(new Vector3(x, y, z), color, new Vector2(i / (float)resU, j / (float)resV));

                    verticesList.Add(v);
                }
            }

            for (int i = 0; i < resU; i++)
            {
                int n = (i + 1) % (resU + 1);

                for (int j = 0; j < resV; j++)
                {
                    int m = (j + 1) % (resV + 1);

                    var i1 = i * (resV + 1) + j;
                    var i2 = i * (resV + 1) + m;
                    var i3 = n * (resV + 1) + m;
                    var i4 = n * (resV + 1) + j;

                    indicesList.Add((uint)i1);
                    indicesList.Add((uint)i2);
                    indicesList.Add((uint)i3);

                    indicesList.Add((uint)i3);
                    indicesList.Add((uint)i4);
                    indicesList.Add((uint)i1);
                }
            }

            _changed = false;

            return (verticesList.ToArray(), indicesList.ToArray());
        }

        // Return whether the mesh parameters have changed since last GetMesh() call for this geometry.
        public bool GeometryChanged() => _changed;

        public void DrawElementGUI()
        {
            ImGui.Text("Mesh parameters");
            ImGui.PushItemWidth(150);
            if (ImGui.SliderInt("Resolution (U)", ref resU, 3, 400, null, ImGuiSliderFlags.Logarithmic))
            {
                _changed = true;
            }

            if (ImGui.SliderInt("Resolution (V)", ref resV, 3, 400, null, ImGuiSliderFlags.Logarithmic))
            {
                _changed = true;
            }

            if (ImGui.SliderFloat("Inner radius", ref r, 0.1f, 50, null, ImGuiSliderFlags.Logarithmic))
            {
                _changed = true;
            }

            if (ImGui.SliderFloat("Outer radius", ref R, 0.1f, 50, null, ImGuiSliderFlags.Logarithmic))
            {
                _changed = true;
            }

            ImGui.Separator();
        }

        public Color4 GetColor()
        {
            return color;
        }

        public void SetColor(Color4 color)
        {
            this.color = color;
        }

        public Vector3 Sample(float u, float v)
        {
            var cosSeg = Math.Cos(u * 2 * Math.PI);
            var sinSeg = Math.Sin(u * 2 * Math.PI);

            var cosTube = Math.Cos(v * 2 * Math.PI);
            var sinTube = Math.Sin(v * 2 * Math.PI);

            double rInner = (R - r) / 2;
            double rMiddle = R - rInner;

            var pos = new Vector4(
                (float)((rMiddle + rInner * cosTube) * cosSeg),
                (float)(rInner * sinTube),
                (float)((rMiddle + rInner * cosTube) * sinSeg),
                1);

            pos = pos * _transform;

            return pos.Xyz;
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

        public void OnTransformChanged(Matrix4 transform)
        {
            _transform = transform;
        }
    }
}
