using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Environment
{
    // Renderable gizmo
    public class Gizmo : RenderableElement
    {
        private Matrix4 _model;

        public Gizmo(IGeometry geom, PrimitiveType primitives) : base(geom) 
        {
            type = primitives;
            _model = Matrix4.Identity;
        }

        public void RenderGizmo(Vector3 worldPosition)
        {
            GL.LineWidth(2);
            _model = Matrix4.CreateTranslation(worldPosition);
            base.Render();
            GL.LineWidth(1);
        }

        // Render shouldn't be used directly, as a gizmo is generally expected to be bound to an object / position and refreshed each frame.
        public override void Render() {}

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", _model);
            shader.SetMatrix4("view", Scene.Instance.camera.viewM);
            shader.SetMatrix4("projection", Scene.Instance.camera.projectionM);
        }
    }
}
