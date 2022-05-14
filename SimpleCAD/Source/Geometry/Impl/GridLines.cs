using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class GridLines : IGeometry
    {
        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            int n = 40;

            Vertex[] verts = new Vertex[n * n];
            uint[] indices = new uint[4 * n * n - 4 * n];

            int index = 0;

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    verts[n * y + x] = new Vertex(new Vector3(x - n/2, y - n/2, 0f), Color4.Gray);

                    if (x < n - 1)
                    {
                        indices[index] = (uint) (n * y + x);
                        indices[index + 1] = (uint)(n * y + x + 1);
                        index += 2;
                    }

                    if (y < n - 1)
                    {
                        indices[index] = (uint)(n * y + x);
                        indices[index + 1] = (uint)(n * (y + 1) + x);
                        index += 2;
                    }
                }
            }

            return (verts, indices);
        }

        public bool GeometryChanged()
        {
            return false;
        }
    }
}
