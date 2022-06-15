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
    class Torus : IGeometry, ISceneGUIElement, IColorable
    {
        public int resU, resV;
        public float R, r;
        public Color4 color;

        public const int MinimumResolution = 3;

        private bool _changed = false;

        public ObjectType DTOType => ObjectType.torus;

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

            for (int i = 0; i < resU; i++)
            {
                var cosSeg = (float)Math.Cos(i * deltaU);
                var sinSeg = (float)Math.Sin(i * deltaU);

                for (int j = 0; j < resV; j++)
                {
                    var cosTube = (float)Math.Cos(j * deltaV);
                    var sinTube = (float)Math.Sin(j * deltaV);

                    x = (rMiddle + rInner * cosTube) * cosSeg;
                    y = rInner * sinTube;
                    z = (rMiddle + rInner * cosTube) * sinSeg;

                    var v = new Vertex(new Vector3(x, y, z), color);

                    verticesList.Add(v);
                }
            }

            for (int i = 0; i < resU; i++)
            {
                int n = (i + 1) % resU;

                for (int j = 0; j < resV; j++)
                {
                    int m = (j + 1) % resV;

                    var i1 = i * resV + j;
                    var i2 = i * resV + m;
                    var i3 = n * resV + m;
                    var i4 = n * resV + j;

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
    }
}
