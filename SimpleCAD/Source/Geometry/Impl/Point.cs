using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Utils;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Enums = SharpSceneSerializer.DTOs.Enums;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Geometry
{
    public class Point : IGeometry, IColorable
    {
        public string VertexShader => "shader.vert";
        public string FragShader => "shader.frag";
        public string TescShader => "";
        public string TeseShader => "";

        public Color4 color;

        public Point(Color4 color)
        {
            this.color = color;
        }

        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            Vertex[] verts = { new Vertex(new Vector3(0f, 0f, 0f), color) };
            uint[] indices = { 0 };

            return (verts, indices);
        }

        public bool GeometryChanged() => false;

        public Color4 GetColor()
        {
            return color;
        }

        public void SetColor(Color4 color)
        {
            this.color = color;
        }

        public void OnTransformChanged(Matrix4 transform)
        {
            // Not required
        }
    }
}
