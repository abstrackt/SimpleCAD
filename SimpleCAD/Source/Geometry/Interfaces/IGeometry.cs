using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public interface IGeometry
    {
        (Vertex[] vertices, uint[] indices) GetMesh(); // Leave empty if uses default shaders.

        bool GeometryChanged();
    }
}
