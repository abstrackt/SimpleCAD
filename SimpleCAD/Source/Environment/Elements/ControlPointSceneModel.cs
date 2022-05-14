﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Environment
{
    // Represents a model defined by a mesh controlled by external control points.
    // It also modifies render behavior to use tesselation as a default way of drawing geometry.
    public class ControlPointSceneModel : SceneModel, ISceneGUIElement
    {
        public List<VirtualPoint> VirtualPoints => new List<VirtualPoint>(_virtualPoints);

        private List<BasicSceneModel> _controlPoints;
        private List<VirtualPoint> _virtualPoints;

        private IControlPointGeometry _geometry;

        protected override IGeometry Geometry {
            get 
            { 
                return _geometry; 
            }
            set 
            {
                if (value is IControlPointGeometry geometry)
                {
                    _geometry = geometry;
                }
                else
                {
                    throw new InvalidOperationException("Cannot assign non-control point geometry to a control point model");
                }
            }
        }

        // Not nice to have this here but whatever, we should probably
        // devise some better way to hold UI state later on.
        private bool _controlPointSelectionListVisible;

        // Create control point model using currently selected points.
        public ControlPointSceneModel(IControlPointGeometry geometry, string name, PrimitiveType primitives) : base(geometry, name, primitives)
        {
            _controlPoints = new List<BasicSceneModel>();
            _virtualPoints = new List<VirtualPoint>();
            AddSelected();
        }

        public void AddSelected()
        {
            foreach (var model in Scene.Instance.basicModels)
            {
                if (SelectionManager.Instance.IsSelected(model))
                    AddPoint(model);
            }
        }

        public void RemovePoint(BasicSceneModel model)
        {
            if (_controlPoints.Contains(model))
                _controlPoints.Remove(model);
        }

        public void AddPoint(BasicSceneModel model)
        {
            if (model.IsControlPoint)
            {
                _controlPoints.Add(model);
            }
        }

        public override void Refresh()
        {
            if (TryUpdateMeshColor() || Geometry.GeometryChanged())
            {
                List<Vector3> positions = new List<Vector3>();

                foreach (var model in _controlPoints)
                {
                    positions.Add(model.Position);
                }

                _geometry.SetControlPoints(positions);
                RegenMesh();
                UpdateVirtualPoints(_geometry.GetVirtualPoints());
                
            }
        }

        private void UpdateVirtualPoints(List<Vector3> positions)
        {
            if (_virtualPoints.Count != positions.Count)
            {
                _virtualPoints.Clear();
                for (int i = 0; i < positions.Count; i++)
                    _virtualPoints.Add(new VirtualPoint(ColorPalette.VirtualPointColor, this));
            }

            for (int i = 0; i < _virtualPoints.Count; i++)
            {
                _virtualPoints[i].SetPosition(positions[i]);
            }
        }

        public void MoveVirtualPoint(VirtualPoint point, Vector3 position)
        {
            if (_virtualPoints.Contains(point))
            {
                int index = _virtualPoints.FindIndex(x => { return point == x; });

                var changedControlPoints = _geometry.MoveVirtualPoint(index, position);
                    
                for (int i = 0; i < changedControlPoints.Count; i++)
                {
                    _controlPoints[i].Translate(changedControlPoints[i]);
                }
                
            }
        }

        protected override void BeforeRendering()
        {
            shader.SetMatrix4("model", Matrix4.Identity);
            base.BeforeRendering();
        }

        protected override void AfterRendering()
        {
            if (_virtualPoints.Count > 0 && SelectionManager.Instance.IsSelected(this))
            {
                foreach (var point in _virtualPoints)
                {
                    point.Render();
                }
            }
            base.AfterRendering();
        }

        protected override bool TryUpdateMeshColor()
        {
            var colorable = _geometry as IColorable;
            if (colorable != null)
            {
                var last = colorable.GetColor();
                var mainColor = SelectionManager.Instance.IsSelected(this) ? ColorPalette.SelectedColor : ColorPalette.DeselectedColor;
                colorable.SetColor(mainColor);
                return last != colorable.GetColor();
            }
            return false;
        }

        public override void DrawElementGUI()
        {
            ImGui.Text(Name);

            ImGui.Separator();

            if (_geometry is ISceneGUIElement element)
            {
                element.DrawElementGUI();
            }

            ImGui.Separator();

            ImGui.Text("Control Points:");

            for (int i = 0; i < _controlPoints.Count; i++)
            {
                ImGui.BulletText(_controlPoints[i].Name);
                ImGui.SameLine();
                if (ImGui.Button("Remove##" + i))
                {
                    RemovePoint(_controlPoints[i]);
                }
            }

            if (_virtualPoints.Count > 0)
            {
                ImGui.Text("Virtual Points:");

                for (int i = 0; i < _virtualPoints.Count; i++)
                {
                    ImGui.Separator();
                    _virtualPoints[i].DrawElementGUI();
                    ImGui.Separator();
                }
            }

            if (ImGui.Button("+", new System.Numerics.Vector2(20, 20)))
            {
                _controlPointSelectionListVisible = !_controlPointSelectionListVisible;
            }

            if (_controlPointSelectionListVisible)
            {
                var scene = Scene.Instance;

                for (int i = 0; i < scene.basicModels.Count; i++)
                {
                    var model = scene.basicModels[i];
                    if (model.IsControlPoint && !_controlPoints.Contains(model))
                    {
                        if (ImGui.Button(model.Name, new System.Numerics.Vector2(100, 20)))
                        {
                            _controlPoints.Add(model);
                            _controlPointSelectionListVisible = false;
                        }
                    }
                }
            }
        }
    }
}
