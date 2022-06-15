using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Enums = SharpSceneSerializer.DTOs.Enums;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Environment
{
    public class PointSceneModel : SceneModel, ISceneGUIElement
    {
        private Vector3 _pos;
        private Matrix4 _transform;
        private bool _deletable;

        // Can be deleted via scene methods
        public bool Deletable => _deletable;
        public override Vector3 Position => _pos;
        public override Matrix4 Transform => _transform;

        public PointSceneModel(string name, bool deletable = true) : base(new Point(ColorPalette.DeselectedColor), name, PrimitiveType.Points)
        {
            _transform = Matrix4.Identity;
            _deletable = deletable;
        }

        private void RefreshMatrices()
        {
            _transform = Matrix4.CreateTranslation(_pos);
        }

        public override void Translate(Vector3 translation, bool additive = false)
        {
            _pos = additive ? _pos + translation : translation;

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
        }

        public void Serialize(out Geom.Point serialized)
        {
            var data = new Geom.Point()
            {
                Position = _pos.AsFloat3(),
                Id = id,
                Name = Name
            };
            serialized = data;
        }
    }
}
