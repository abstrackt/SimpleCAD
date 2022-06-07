using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class Point : IGeometry, IColorable
    {
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

        public bool GeometryChanged()
        {
            return false;
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
