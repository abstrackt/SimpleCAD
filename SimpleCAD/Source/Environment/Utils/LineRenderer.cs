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

        protected override IGeometry Geometry { 
            get 
            { 
                return _lines; 
            }
            set 
            {
                if (value is LineSet geometry)
                {
                    _lines = geometry;
                }
                else
                {
                    throw new InvalidOperationException("Invalid geometry assigned to LineRenderer");
                }
            }
        }

        public LineRenderer() : base(new LineSet()) 
        {
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
