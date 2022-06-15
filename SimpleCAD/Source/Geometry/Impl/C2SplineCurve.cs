using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;
using System.Collections.Generic;

namespace SimpleCAD.Source.Geometry
{
    public class C2SplineCurve : AdaptiveCurve, ISceneGUIElement
    {
        public override int SegmentSize => 4;
        public override int SegmentOffset => 1;

        private List<Vector3> _bezierPoints;
        private bool _drawBezierPolygon, _drawDeBoorPolygon;

        public C2SplineCurve() : base() {
            _bezierPoints = new List<Vector3>();
        }

        protected override List<Vector3> ProcessSegment(List<Vector3> deBoorPoints)
        {
            if (deBoorPoints.Count != 4)
                return new List<Vector3>();

            var bezierPoints = new List<Vector3>(4);

            var v1 = deBoorPoints[1] - deBoorPoints[0];
            var v2 = deBoorPoints[2] - deBoorPoints[1];
            var v3 = deBoorPoints[3] - deBoorPoints[2];

            var w1 = deBoorPoints[0] + 2 * v1 / 3f;
            var w2 = deBoorPoints[1] + v2 / 3f;
            var w3 = deBoorPoints[1] + 2 * v2 / 3f;
            var w4 = deBoorPoints[2] + v3 / 3f;

            bezierPoints.Add(w1 + (w2 - w1) / 2f);
            bezierPoints.Add(w2);
            bezierPoints.Add(w3);
            bezierPoints.Add(w3 + (w4 - w3) / 2f);

            if (_bezierPoints.Count > 0)
            {
                _bezierPoints.RemoveAt(_bezierPoints.Count - 1);
            }
            
            _bezierPoints.AddRange(bezierPoints);

            return bezierPoints;
        }

        protected override void BeforeMeshGeneration()
        {
            _bezierPoints.Clear();
        }

        public override List<Line> GetLines()
        {
            var lines = new List<Line>();

            if (_drawBezierPolygon)
            {
                var bezier = new Line();

                for (int i = 0; i < _bezierPoints.Count; i++)
                {
                    bezier.points.Add(_bezierPoints[i]);
                }
                bezier.color = ColorPalette.BernsteinPolygonColor;
                lines.Add(bezier);
            }

            if (_drawDeBoorPolygon)
            {
                var deBoor = new Line();

                for (int i = 0; i < controlPoints.Count; i++)
                {
                    deBoor.points.Add(controlPoints[i]);
                }
                deBoor.color = ColorPalette.DeBoorPolygonColor;
                lines.Add(deBoor);
            }

            return lines;
        }

        public override List<Vector3> GetVirtualPoints()
        {
            return new List<Vector3>(_bezierPoints);
        }

        public override List<Vector3> MoveVirtualPoint(int i, Vector3 position)
        {
            // Adjust de Boor polygon

            // Determine if point lies on the polygon or in the middle
            var indexInSegment = i % (SegmentSize - 1);
            var deBoorPointIndex = i / 3 + 1;

            Vector3 db1, db2, v1, v2;

            switch (indexInSegment)
            {
                // Middle point, special case
                case 0:
                    { }
                    db1 = controlPoints[deBoorPointIndex - 1];
                    db2 = controlPoints[deBoorPointIndex + 1];
                    v1 = (db2 + db1) / 2f;
                    v2 = position - v1;
                    controlPoints[deBoorPointIndex] = v1 + 3f * v2 / 2f;
                    break;
                // Along 1/3 length
                case 1:
                    db1 = controlPoints[deBoorPointIndex + 1];
                    v1 = position - db1;
                    controlPoints[deBoorPointIndex] = db1 + 3f * v1 / 2f;
                    break;
                // Along 2/3 length
                case 2:
                    db1 = controlPoints[deBoorPointIndex + 1];
                    v1 = position - db1;
                    controlPoints[deBoorPointIndex] = db1 + 3f * v1;
                    break;
            }

            return new List<Vector3>(controlPoints);
        }

        public void DrawElementGUI()
        {
            ImGui.Checkbox("Show Bezier polygon", ref _drawBezierPolygon);
            ImGui.Checkbox("Show de Boor polygon", ref _drawDeBoorPolygon);
        }
    }
}
