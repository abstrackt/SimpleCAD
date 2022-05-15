using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;

namespace SimpleCAD.Source.Environment
{
    // Represents a single rendered object in the scene with defined geometry.
    public class BasicSceneModel : SceneModel, ISceneGUIElement
    {
        public Vector3 Position => _pos;
        public Vector3 Rotation => _rot;
        public Matrix4 Transform => _transform;

        private Vector3 _pos;
        private Vector3 _rot;
        private Vector3 _scale;
        private Matrix4 _transform;
        private bool _isControlPoint;

        public bool IsControlPoint => _isControlPoint;

        protected override IGeometry Geometry { get; set; }

        public BasicSceneModel(IGeometry geometry, string name, PrimitiveType primitives, bool isControlPoint = false) : base(geometry, name, primitives)
        {
            _transform = Matrix4.Identity;
            _scale = Vector3.One;
            _isControlPoint = isControlPoint;
        }

        private void RefreshMatrices()
        {
            _transform =
                Matrix4.CreateScale(_scale) * 
                Matrix4.CreateRotationZ(_rot.Z) * 
                Matrix4.CreateRotationY(_rot.Y) * 
                Matrix4.CreateRotationX(_rot.X) *
                Matrix4.CreateTranslation(_pos);
        }

        public void Translate(Vector3 translation, bool additive = false)
        {
            _pos = additive ? _pos + translation : translation;

            RefreshMatrices();
        }

        public void Rotate(Vector3 rotation, bool additive = false)
        {
            _rot = additive ? _rot + rotation * (float)Math.PI / 180f : rotation * (float)Math.PI / 180f;

            RefreshMatrices();
        }

        public void Rescale(Vector3 scale)
        {
            _scale = scale;

            RefreshMatrices();
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", _transform);
            base.BeforeRendering();
        }

        public override void DrawElementGUI()
        {
            // TOOD: Draw transform code here.
            var drawable = Geometry as ISceneGUIElement;

            if (drawable != null)
                drawable.DrawElementGUI();

            ImGui.Text(Name);

            ImGui.Separator();

            ImGui.Text("Transform");

            var pos = _pos;
            var tmp = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
            if (ImGui.DragFloat3("Position", ref tmp, 0.05f))
            {
                Translate(new Vector3(tmp.X, tmp.Y, tmp.Z));
            }

            var rot = MathUtils.Rad2Deg(_rot);
            tmp = new System.Numerics.Vector3(rot.X, rot.Y, rot.Z);
            if (ImGui.DragFloat3("Rotation", ref tmp, 2f))
            {
                Rotate(new Vector3(tmp.X, tmp.Y, tmp.Z));
            }

            var scale = _scale;
            tmp = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);
            if (ImGui.DragFloat3("Scale", ref tmp, 0.1f))
            {
                Rescale(new Vector3(tmp.X, tmp.Y, tmp.Z));
            }
        }
    }
}
