using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Mathematics;

namespace SimpleCAD.Source.Utils
{
    public static class ColorPalette
    {
        public readonly static Color4 DeselectedColor = Color4.LightGray;
        public readonly static Color4 SelectedColor = Color4.Orange;
        public readonly static Color4 MidpointColor = Color4.Yellow;
        public readonly static Color4 BernsteinPolygonColor = Color4.CornflowerBlue;
        public readonly static Color4 DeBoorPolygonColor = Color4.MediumPurple;
        public readonly static Color4 VirtualPointColor = Color4.CornflowerBlue;
        public readonly static Color4 DebugColor = Color4.Magenta;
    }
}
