using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Intersections;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Environment
{
    public class IntersectionSceneModel : SceneModel, ISceneGUIElement
    {
        private SceneModel _m1, _m2;

        public SceneModel M1 => _m1;
        public SceneModel M2 => _m2;

        public override Vector3 Position => Vector3.Zero;
        public override Matrix4 Transform => Matrix4.Identity;

        public override bool MovementLocked => true;
        public override bool HasParametricGeometry => false;
        public override IParametricSurface ParametricGeometry => null;

        public C2InterpolatingCurve _curve;

        public IntersectionData _data;

        public IntersectionSceneModel(
            C2InterpolatingCurve curve,
            string name,
            IntersectionData data,
            SceneModel m1, 
            SceneModel m2) : base(curve, name, PrimitiveType.Patches)
        {
            _m1 = m1;
            _m2 = m2;
            _data = data;
            _curve = curve;
            _curve.SetControlPoints(data.points);
        }

        // We cannot translate intersections
        public override void Translate(Vector3 translation, bool additive = false)
        {
            return;
        }

        public override void Render()
        {
            shader.SetMatrix4("model", Matrix4.Identity); 
            base.BeforeRendering();
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);
            GL.DrawArrays(type, 0, _vertexCount);
            AfterRendering();
        }

        protected override void AfterRendering()
        {
            base.AfterRendering();
        }

        protected override bool TryUpdateMeshColor()
        {
            var colorable = Geometry as IColorable;
            if (colorable != null)
            {
                var last = colorable.GetColor();
                colorable.SetColor(SelectionManager.Instance.IsSelected(this) ? ColorPalette.SelectedIntersectionColor : ColorPalette.IntersectionColor);
                return last != colorable.GetColor();
            }
            return false;
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        public override void DrawElementGUI()
        {
            
        }
    }
}
 