using SharpSceneSerializer.DTOs.GeometryObjects;
using SharpSceneSerializer.DTOs.Interfaces;

namespace SharpSceneSerializer.DTOs;

public class Scene
{
    public Scene(List<Point> points = null, List<IGeometryObject> geometry = null)
    {
        Points = points;
        Geometry = geometry;
    }
    public List<Point> Points { get; init; }
    public List<IGeometryObject> Geometry { get; init; }
}