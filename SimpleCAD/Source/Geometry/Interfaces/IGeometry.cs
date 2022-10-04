using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public interface IGeometry
    {
        public string VertexShader { get; }
        public string FragShader { get; }
        public string TescShader { get; }
        public string TeseShader { get; }

        (Vertex[] vertices, uint[] indices) GetMesh(); // Leave empty if uses default shaders.
        bool GeometryChanged();
        void OnTransformChanged(Matrix4 transform);
    }
}
