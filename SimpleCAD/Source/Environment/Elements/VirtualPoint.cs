using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;

namespace SimpleCAD.Source.Environment
{
    // Represents a parented control point that can affect the parent scene model
    public class VirtualPoint : RenderableElement, ISceneGUIElement
    {
        public ComplexSceneModel parent;
        public Vector3 Position => _pos;

        private Vector3 _pos;
        private Matrix4 _transform;

        public VirtualPoint(Color4 color, ComplexSceneModel model) : base(new Point(color))
        {
            _transform = Matrix4.Identity;
            type = PrimitiveType.Points;
            parent = model;
        }

        private void RefreshMatrices()
        {
            _transform = Matrix4.CreateTranslation(_pos);
        }

        public void SetPosition(Vector3 position)
        {
            _pos = position;
            RefreshMatrices();
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", _transform);
            shader.SetMatrix4("view", Scene.Instance.camera.viewM);
            shader.SetMatrix4("projection", Scene.Instance.camera.projectionM);
        }

        public void DrawElementGUI()
        {
            var pos = _pos;
            var tmp = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
            if (ImGui.DragFloat3("Position##" + this.GetHashCode(), ref tmp, 0.05f))
            {
                parent.MoveVirtualPoint(this, new Vector3(tmp.X, tmp.Y, tmp.Z));
            }
        }
    }
}
