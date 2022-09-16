using OpenTK.Mathematics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Geometry.Impl
{
    public class GregoryPatch : IControlPointGeometry, ISceneGUIElement
    {
        private List<Vector3> _gregoryPoints;

        public GregoryPatch() : base()
        {
            _gregoryPoints = new List<Vector3>();
        }

        public void DrawElementGUI()
        {
            
        }

        public bool GeometryChanged()
        {
            return true;
        }

        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            return (new Vertex[] { }, new uint[] { });
        }

        public List<Vector3> GetVirtualPoints()
        {
            return _gregoryPoints;
        }

        public void SetVirtualPoints(List<Vector3> positions)
        {
            _gregoryPoints = positions;
        }

        public List<Vector3> MoveVirtualPoint(int i, Vector3 position)
        {
            return _gregoryPoints;
        }

        public void SetControlPoints(List<Vector3> positions)
        {
            return;
        }
    }
}
