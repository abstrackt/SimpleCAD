using System.Text;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using OpenTK.Mathematics;

namespace SimpleCAD.Source.Environment
{
    // Represents a single visible object in the scene.
    public abstract class SceneModel : RenderableElement, ISceneGUIElement, IDisposable
    {
        public const int MAX_NAME_LEN = 30;

        public byte[] name = new byte[MAX_NAME_LEN];
        public uint id;

        public string Name => Encoding.ASCII.GetString(name).Trim('\0');
        public abstract Vector3 Position { get; }
        public abstract Matrix4 Transform { get; }

        public abstract bool MovementLocked { get; }

        public abstract bool HasParametricGeometry { get; }
        public abstract IParametricSurface ParametricGeometry { get; }


        public SceneModel(IGeometry geometry, string name, PrimitiveType primitives) : base(geometry)
        {
            id = Scene.Instance.NextId();

            if (name.Length > MAX_NAME_LEN)
                throw new InvalidOperationException("Cannot assign a model name longer than 30 characters.");

            type = primitives;

            SetVertShader(geometry.VertexShader);
            SetFragShader(geometry.FragShader);
            if (geometry.TescShader != String.Empty && geometry.TeseShader != String.Empty)  
                SetTesselationShader(geometry.TescShader, geometry.TeseShader);

            TryUpdateMeshColor();

            Encoding.ASCII.GetBytes(name).CopyTo(this.name, 0);
        }

        public virtual void Refresh()
        {
            if (TryUpdateMeshColor() || Geometry.GeometryChanged())
            {
                RegenMesh();
            }
        }

        protected virtual bool TryUpdateMeshColor()
        {
            var colorable = Geometry as IColorable;
            if (colorable != null)
            {
                var last = colorable.GetColor();
                colorable.SetColor(SelectionManager.Instance.IsSelected(this) ? ColorPalette.SelectedColor : ColorPalette.DeselectedColor);
                return last != colorable.GetColor();
            }
            return false;
        }

        public virtual void Translate(Vector3 translation, bool additive = false)
        {
            if (MovementLocked)
                return;
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("view", Scene.Instance.camera.viewM);
            shader.SetMatrix4("projection", Scene.Instance.camera.projectionM);
        }

        public abstract void DrawElementGUI();
    }
}
