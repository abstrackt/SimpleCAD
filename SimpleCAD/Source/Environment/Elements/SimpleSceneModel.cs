using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Types = SharpSceneSerializer.DTOs.Types;

namespace SimpleCAD.Source.Environment
{
    // Represents a single rendered object in the scene with defined geometry.
    public class SimpleSceneModel : SceneModel, ISceneGUIElement
    {
        public override Vector3 Position => _pos;
        public Vector3 Rotation => _rot;
        public Vector3 Scale => _scale;
        public override Matrix4 Transform => _transform;
        public override bool MovementLocked => Scene.Instance.IsPartOfIntersection(this);
        public override bool HasParametricGeometry => Geometry is IParametricSurface;
        public override IParametricSurface ParametricGeometry => (IParametricSurface)Geometry;

        private Vector3 _pos;
        private Vector3 _rot;
        private Vector3 _scale;
        private Matrix4 _transform;
        private bool _trimZero;
        private bool _trim;

        public SimpleSceneModel(IGeometry geometry, string name, PrimitiveType primitives) : base(geometry, name, primitives)
        {
            _transform = Matrix4.Identity;
            _scale = Vector3.One;
        }

        private void RefreshMatrices()
        {
            _transform =
                Matrix4.CreateScale(_scale) *
                Matrix4.CreateRotationZ(_rot.Z) *
                Matrix4.CreateRotationY(_rot.Y) *
                Matrix4.CreateRotationX(_rot.X) *
                Matrix4.CreateTranslation(_pos);

            Geometry.OnTransformChanged(_transform);
        }

        public override void Translate(Vector3 translation, bool additive = false)
        {
            if (MovementLocked)
                return;

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

            if (_texture != null)
            {
                ImGui.Separator();
                ImGui.Text("Intersection");
                ImGui.Image((IntPtr)_texture.Handle, new(270, 270));


                if (ImGui.Checkbox("Trim", ref _trim))
                {
                    ToggleTrimming(_trim);
                }

                if (ImGui.Button("Change trimming side"))
                {
                    if (_trimZero)
                    {
                        _trimZero = false;
                        SetTrimTarget(255);
                    }
                    else
                    {
                        _trimZero = true;
                        SetTrimTarget(0);
                    }
                }
            }
        }

        public bool TrySerialize(out IGeometryObject serialized)
        {
            if (Geometry is Torus torus)
            {
                var data = new Geom.Torus()
                {
                    Id = id,
                    Name = Name,
                    SmallRadius = torus.r,
                    LargeRadius = torus.R,
                    Position = _pos.AsFloat3(),
                    Rotation = _rot.AsFloat3(),
                    Scale = _scale.AsFloat3(),
                    Samples = new Types.Uint2((uint)torus.resU, (uint)torus.resV)
                };
                serialized = data;
                return true;
            }

            serialized = null;
            return false;
        }
    }
}
