using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class CursorLines : IGeometry
    {
        public string VertexShader => "shader.vert";
        public string FragShader => "shader.frag";
        public string TescShader => "";
        public string TeseShader => "";

        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            Vertex[] verts = new Vertex[6];
            verts[0] = new Vertex(new Vector3(0f, 0f, 0f), Color4.Red);
            verts[1] = new Vertex(new Vector3(1f, 0f, 0f), Color4.Red);
            verts[2] = new Vertex(new Vector3(0f, 0f, 0f), Color4.Green);
            verts[3] = new Vertex(new Vector3(0f, 1f, 0f), Color4.Green);
            verts[4] = new Vertex(new Vector3(0f, 0f, 0f), Color4.Blue);
            verts[5] = new Vertex(new Vector3(0f, 0f, 1f), Color4.Blue);

            uint[] indices = {
                0, 1,
                2, 3,
                4, 5,
            };

            return (verts, indices);
        }

        public bool GeometryChanged() => false;

        public void OnTransformChanged(Matrix4 transform)
        {
            // Not required
        }
    }
}
