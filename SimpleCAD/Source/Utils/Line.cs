using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Utils
{
    public class Line
    {
        public List<Vector3> points;
        public Color4 color;

        public Line()
        {
            points = new List<Vector3>();
            color = ColorPalette.DebugColor;
        }
    }
}
