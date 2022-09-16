using System;
using OpenTK.Mathematics;

namespace SimpleCAD.Source.Geometry
{
    // Also takes projection matrix for purposes of adaptive rendering
    public interface IControlPointGeometry : IGeometry
    {
        void SetControlPoints(List<Vector3> positions);
        List<Vector3> GetVirtualPoints();
        List<Vector3> MoveVirtualPoint(int i, Vector3 position);
    }
}
