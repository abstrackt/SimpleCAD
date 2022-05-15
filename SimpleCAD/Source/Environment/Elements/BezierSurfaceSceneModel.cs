using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Environment
{
    public class BezierSurfaceSceneModel : ControlPointSceneModel
    {
        public BezierSurfaceSceneModel(IControlPointGeometry geometry, string name, PrimitiveType primitives) : base(geometry, name, primitives)
        {
            
        }
    }
}
