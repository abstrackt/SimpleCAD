using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Environment
{
    // Render simple lines
    public class LineRenderer : RenderableElement
    {
        private LineSet _lines;

        public LineRenderer(LineSet lineSet) : base(lineSet)
        {
            _lines = lineSet;
            type = PrimitiveType.Lines;
        }

        public void RenderLines(IEnumerable<Line> lines)
        {
            _lines.ClearLines();
            foreach (var line in lines)
            {
                _lines.AddLine(line);
            }
            RegenMesh();
            Render();
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", Matrix4.Identity);
            shader.SetMatrix4("view", Scene.Instance.camera.viewM);
            shader.SetMatrix4("projection", Scene.Instance.camera.projectionM);
        }
    }
}
