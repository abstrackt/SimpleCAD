using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class C0Bezier : AdaptiveCurve, IColorable, ISceneGUIElement
    {
        private bool _drawPolygon;

        public override int SegmentSize => 4;
        public override int SegmentOffset => 3;

        public C0Bezier() : base() { }

        public override List<Line> GetPolygons()
        {
            var lines = new List<Line>();

            if (_drawPolygon)
            {
                var bezier = new Line();

                for (int i = 0; i < controlPoints.Count; i++)
                {
                    bezier.points.Add(controlPoints[i]);
                }
                bezier.color = ColorPalette.BernsteinPolygonColor;
                lines.Add(bezier);
            }

            return lines;
        }

        public void DrawElementGUI()
        {
            ImGui.Checkbox("Show Bezier polygon", ref _drawPolygon);
        }
    }
}
