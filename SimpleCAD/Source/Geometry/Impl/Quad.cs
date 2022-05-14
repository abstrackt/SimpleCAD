using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class Quad : IGeometry, IColorable
    {
        public Color4 color;

        public Quad(Color4 color)
        {
            this.color = color;
        }

        (Vertex[] vertices, uint[] indices) IGeometry.GetMesh()
        {
            Vertex[] verts = new Vertex[4];
            verts[0] = new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), color);
            verts[1] = new Vertex(new Vector3(0.5f, -0.5f, 0.0f), color);
            verts[2] = new Vertex(new Vector3(0.5f, 0.5f, 0.0f), color);
            verts[3] = new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), color);

            uint[] indices = {
                0, 1, 3,
                1, 2, 3 
            };

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
