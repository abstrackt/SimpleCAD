using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    class Torus : IGeometry, ISceneGUIElement, IColorable
    {
        public int resolution;
        public float R, r;
        public Color4 color;

        public const int MinimumResolution = 3;

        private bool _changed = false;

        public Torus(int resolution, float R, float r, Color4 color)
        {
            this.resolution = Math.Max(resolution, MinimumResolution);
            this.R = R;
            this.r = r;
            this.color = color;
        }

        public (Vertex[] vertices, uint[] indices) GetMesh() 
        {
            int nVerts = resolution * resolution;
            int nPrimitives = nVerts * 2;
            int nIndices = nPrimitives * 3;

            float pi = (float)Math.PI;

            List<Vertex> verticesList = new List<Vertex>(nVerts);
            List<uint> indicesList = new List<uint>(nIndices);

            float delta = 2 * pi / resolution;

            float x = 0;
            float y = 0;
            float z = 0;

            for (int i = 0; i < resolution; i++)
            {
                var cosSeg = (float)Math.Cos(i * delta);
                var sinSeg = (float)Math.Sin(i * delta);

                for (int j = 0; j < resolution; j++)
                {
                    var cosTube = (float)Math.Cos(j * delta);
                    var sinTube = (float)Math.Sin(j * delta);

                    x = (R + r * cosTube) * cosSeg;
                    y = r * sinTube;
                    z = (R + r * cosTube) * sinSeg;

                    var v = new Vertex(new Vector3(x, y, z), color);

                    verticesList.Add(v);
                }
            }

            for (int i = 0; i < resolution; i++)
            {
                int n = (i + 1) % resolution;

                for (int j = 0; j < resolution; j++)
                {
                    int m = (j + 1) % resolution;

                    var i1 = i * resolution + j;
                    var i2 = i * resolution + m;
                    var i3 = n * resolution + m;
                    var i4 = n * resolution + j;

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
        public bool GeometryChanged()
        {
            return _changed;
        }

        public void DrawElementGUI()
        {
            ImGui.Text("Mesh parameters");
            ImGui.PushItemWidth(150);
            if (ImGui.SliderInt("Mesh resolution", ref resolution, 3, 400, null, ImGuiSliderFlags.Logarithmic))
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
