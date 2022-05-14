using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Environment
{
    public class Grid : RenderableElement
    {
        private Matrix4 _model, _view, _proj;
        private float _falloff;
        private Color4 _bg;

        protected override IGeometry Geometry { get; set; }

        public Grid() : base(new GridLines()) 
        {
            type = PrimitiveType.Lines;
            SetVertShader("gridShader.vert");
        }

        public void UpdateGrid(Matrix4 transform, float falloff, Color4 background)
        {
            _model = transform;
            _falloff = falloff;
            _bg = background;
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", _model);
            shader.SetMatrix4("view", Scene.Instance.camera.viewM);
            shader.SetMatrix4("projection", Scene.Instance.camera.projectionM);
            shader.SetFloat("falloff_factor", _falloff);
            shader.SetVector3("background", new Vector3(_bg.R, _bg.G, _bg.B));
        }
    }
}
