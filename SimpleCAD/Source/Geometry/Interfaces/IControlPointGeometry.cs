using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;

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
