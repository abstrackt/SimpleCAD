using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Intersections;
using SimpleCAD.Source.Utils;


namespace SimpleCAD.Source.Environment
{
    // Represents a model defined by a mesh controlled by external control points.
    // It also modifies render behavior to use tesselation as a default way of drawing geometry.
    public abstract class ComplexSceneModel : SceneModel, ISceneGUIElement
    {
        public List<VirtualPoint> VirtualPoints => new List<VirtualPoint>(_virtualPoints);
        public List<PointSceneModel> ControlPoints => new List<PointSceneModel>(_controlPoints);
        public override Vector3 Position => Vector3.Zero;
        public override Matrix4 Transform => Matrix4.Identity;

        public override bool MovementLocked => Scene.Instance.IsPartOfIntersection(this);

        private List<PointSceneModel> _controlPoints;
        private List<VirtualPoint> _virtualPoints;

        private IControlPointGeometry _geometry;

        private List<Vector3> _previousPos = new List<Vector3>();

        private bool _immutable;
        private bool _removePoints;
        private bool _trimZero;
        private bool _trim;

        // Control point list cannot be modified
        public bool Immutable => _immutable;
        public bool RemovePoints => _removePoints;

        // Not nice to have this here but whatever, we should probably
        // devise some better way to hold UI state later on.
        private bool _controlPointSelectionListVisible;

        // Create control point model using currently selected points.
        public ComplexSceneModel(IControlPointGeometry geometry, string name, PrimitiveType primitives, bool immutable = false, bool removePoints = false) : base(geometry, name, primitives)
        {
            _geometry = geometry;
            _controlPoints = new List<PointSceneModel>();
            _virtualPoints = new List<VirtualPoint>();
            _immutable = immutable;
            _removePoints = removePoints;
        }

        public void SetPoints(List<PointSceneModel> points, bool force = false)
        {
            if (!Immutable || force)
            {
                foreach (var model in points)
                {
                    _controlPoints.Add(model);
                }
            }
        }

        public void RemovePoint(PointSceneModel model)
        {
            if (!Immutable)
            {
                if (_controlPoints.Contains(model))
                    _controlPoints.Remove(model);
            }
        }

        public void AddPoint(PointSceneModel model, bool force = false)
        {
            if (!Immutable || force)
            {
                _controlPoints.Add(model); 
            }
        }

        public virtual void ReplacePoint(PointSceneModel oldPoint, PointSceneModel newPoint)
        {

            for (int i = 0; i < _controlPoints.Count; i++)
            {
                if (_controlPoints[i] == oldPoint)
                {
                    _controlPoints[i] = newPoint;
                }  
            }      
        }

        public override void Refresh()
        {
            var pointsChanged = false;

            if (_previousPos.Count != _controlPoints.Count)
            {
                pointsChanged = true;
            }
            else
            {
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    if (_previousPos[i] != _controlPoints[i].Position)
                    {
                        pointsChanged = true;
                        break;
                    }
                }
            }
            
            if (TryUpdateMeshColor() || Geometry.GeometryChanged() || pointsChanged)
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

            _previousPos.Clear();
            for (int i = 0; i < _controlPoints.Count; i++)
            {
                _previousPos.Add(_controlPoints[i].Position);
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

        public override void Translate(Vector3 translation, bool additive = false)
        {
            if (MovementLocked)
                return;

            foreach (var point in _controlPoints)
            {
                point.Translate(translation, additive);
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

        public override void DrawElementGUI()
        {
            ImGui.Text(Name);

            if (ImGui.Button("Select All Points"))
            {
                SelectionManager.Instance.Clear();
                SelectionManager.Instance.Remove(this);
                foreach (var point in ControlPoints)
                    SelectionManager.Instance.Add(point);
            }

            ImGui.Separator();

            if (_geometry is ISceneGUIElement element)
            {
                element.DrawElementGUI();
            }

            ImGui.Text("Control Points:");

            for (int i = 0; i < _controlPoints.Count; i++)
            {
                ImGui.BulletText(_controlPoints[i].Name);
                if (!Immutable)
                {
                    ImGui.SameLine();
                    if (ImGui.Button("Remove##" + i))
                    {
                        RemovePoint(_controlPoints[i]);
                    }
                }
            }

            if (!Immutable)
            {
                if (ImGui.Button("+", new System.Numerics.Vector2(20, 20)))
                {
                    _controlPointSelectionListVisible = !_controlPointSelectionListVisible;
                }

                if (_controlPointSelectionListVisible)
                {
                    var scene = Scene.Instance;

                    for (int i = 0; i < scene.pointModels.Count; i++)
                    {
                        var model = scene.pointModels[i];
                        if (!_controlPoints.Contains(model))
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
            
            if (_virtualPoints.Count > 0)
            {
                ImGui.Separator();
                ImGui.Text("Virtual Points:");

                for (int i = 0; i < _virtualPoints.Count; i++)
                {
                    ImGui.Separator();
                    _virtualPoints[i].DrawElementGUI();
                    ImGui.Separator();
                }
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

        public abstract bool TrySerialize(out IGeometryObject serialized);
    }
}
