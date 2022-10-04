using OpenTK.Mathematics;

namespace SimpleCAD.Source.Utils
{
    public struct Vertex
    {
        public const int Size = (4 + 4 + 2) * 4;

        private readonly Vector4 _position;
        private readonly Color4 _color;
        private readonly Vector2 _uv;

        public Vertex(Vector3 position, Color4 color)
        {
            _position = new Vector4(position, 1);
            _color = color;
            _uv = Vector2.Zero;
        }

        public Vertex(Vector3 position, Color4 color, Vector2 uv)
        {
            _position = new Vector4(position, 1);
            _color = color;
            _uv = uv;
        }
    }
}
