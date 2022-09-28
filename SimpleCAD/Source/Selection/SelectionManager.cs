using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using OpenTK;
using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;

namespace SimpleCAD.Source.Utils
{
    public sealed class SelectionManager : ISceneGUIElement
    {
        private SelectionManager() {
            _selectedSimpleModels = new HashSet<SceneModel>();
            _selectedComplexModels = new HashSet<ComplexSceneModel>();
            _selectedIntersections = new HashSet<IntersectionSceneModel>();
            _selectionCache = new Dictionary<SceneModel, Matrix4>();
        }

        private static SelectionManager instance = null;
        public static SelectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SelectionManager();
                }
                return instance;
            }
        }

        public Vector3 Midpoint => _originalPivot + _currentPos;
        public bool Selected => _selectedSimpleModels.Count > 0;
        public List<PointSceneModel> SelectedPoints => _selectedSimpleModels
            .Where(x => x is PointSceneModel).Cast<PointSceneModel>()
            .ToList();

        public List<SurfaceSceneModel> SelectedBezierSurfaces => _selectedComplexModels
            .Where(x => (x is SurfaceSceneModel surf) && 
            (surf.Surface is C0BezierSurface) && 
            surf.Surface.PatchesU == 1 && 
            surf.Surface.PatchesV == 1)
            .Cast<SurfaceSceneModel>()
            .ToList();

        public List<ComplexSceneModel> SelectedParametricSurfaces => _selectedComplexModels
            .Where(x => x.HasParametricGeometry)
            .ToList();

        public bool ComplexModelSelected => _selectedComplexModels.Count == 1;
        public int SimpleCount => _selectedSimpleModels.Count;
        public int ComplexCount => _selectedComplexModels.Count;
        

        private Vector3 _originalPivot;

        private Vector3 _currentPos;
        private Vector3 _currentRot;
        private Vector3 _currentScale;

        private HashSet<SceneModel> _selectedSimpleModels;
        private Dictionary<SceneModel, Matrix4> _selectionCache;
        private HashSet<ComplexSceneModel> _selectedComplexModels;
        private HashSet<IntersectionSceneModel> _selectedIntersections;

        public void Add(SceneModel model)
        {
            _selectedSimpleModels.Add(model);
            ResetState();
        }

        public void Add(ComplexSceneModel model)
        {
            _selectedComplexModels.Add(model);
            ResetState();
        }

        public void Add(IntersectionSceneModel model)
        {
            _selectedIntersections.Add(model);
            ResetState();
        }

        public void Remove(SceneModel model)
        {
            if (_selectedSimpleModels.Contains(model))
                _selectedSimpleModels.Remove(model);
            ResetState();
        }

        public void Remove(ComplexSceneModel model)
        {
            if (_selectedComplexModels.Contains(model))
                _selectedComplexModels.Remove(model);
        }

        public void Remove(IntersectionSceneModel model)
        {
            if (_selectedIntersections.Contains(model))
                _selectedIntersections.Remove(model);
        }

        public void Clear()
        {
            _selectedComplexModels.Clear();
            _selectedSimpleModels.Clear();
            _selectedIntersections.Clear();
            _selectionCache.Clear();
        }

        public bool TryGetSingleSelected(out SceneModel model)
        {
            model = null;
            if (ComplexCount == 1)
            {
                model = _selectedComplexModels.Single();
                return true;
            } 
            else if (ComplexCount > 1)
            {
                return false;
            }

            if (SimpleCount != 1)
            {
                return false;   
            }

            model = _selectedSimpleModels.Single();
            return true;
        }

        public bool IsSelected(SceneModel model)
        {
            return _selectedSimpleModels.Contains(model) || _selectedComplexModels.Contains(model) || _selectedIntersections.Contains(model);
        }

        public int SelectedCount()
        {
            return _selectedSimpleModels.Count;
        }

        public void ResetState()
        {
            _selectionCache.Clear();
            _currentPos = Vector3.Zero;
            _currentRot = Vector3.Zero;
            _currentScale = Vector3.One;

            foreach (var model in _selectedSimpleModels)
            {
                _selectionCache.Add(model, model.Transform);
            }
            
            _originalPivot = RecalculatePivot();
        }

        public Vector3 RecalculatePivot()
        {
            var pivot = Vector3.Zero;

            foreach (var model in _selectedSimpleModels)
            {
                pivot += model.Position;
            }

            if (SimpleCount > 1)
            {
                pivot /= SimpleCount;
            }

            return pivot;
        }

        public void TranslateSelection(Vector3 translation)
        {
            _currentPos = translation;

            MoveSelection();
        }

        public void RotateSelection(Vector3 rotation)
        {
            rotation = rotation * (float)Math.PI / 180f;
            _currentRot = rotation;

            MoveSelection();
        }

        public void RescaleSelection(Vector3 scale)
        {
            _currentScale = scale;

            MoveSelection();
        }

        private void MoveSelection()
        {
            foreach (var model in _selectedSimpleModels)
            {
                var transform = _selectionCache[model];

                var translatePivot = Matrix4.CreateTranslation(-_originalPivot);

                transform *= translatePivot;

                var groupRotation =
                    Matrix4.CreateRotationZ(_currentRot.Z) *
                    Matrix4.CreateRotationY(_currentRot.Y) *
                    Matrix4.CreateRotationX(_currentRot.X);

                transform *= groupRotation;

                var groupTranslation =
                    Matrix4.CreateTranslation(_currentPos);

                transform *= groupTranslation;

                var groupScale =
                    Matrix4.CreateScale(_currentScale);

                transform *= groupScale;

                var translateBack = Matrix4.CreateTranslation(_originalPivot);

                transform *= translateBack;

                (var translation, var rotation, var scale) = MathUtils.DecomposeMatrix(transform);

                model.Translate(translation);

                if (model is SimpleSceneModel basicModel)
                {
                    basicModel.Rotate(rotation * 180f / (float)Math.PI);
                    basicModel.Rescale(scale);
                }
            }
        }

        public void DrawElementGUI()
        {
            var bezierSurfaces = SelectedBezierSurfaces;

            if (bezierSurfaces.Count == 3)
            {
                if (ImGui.Button("Create Gregory Patch"))
                {
                    Scene.Instance.SetupGregoryPatch(
                        bezierSurfaces[0], 
                        bezierSurfaces[1], 
                        bezierSurfaces[2]);
                }
            }

            var parametricSurfaces = SelectedParametricSurfaces;

            if (parametricSurfaces.Count == 2)
            {
                if (ImGui.Button("Find intersection"))
                {
                    Scene.Instance.SetupIntersection(parametricSurfaces[0], parametricSurfaces[1]);
                }
            }

            if (_selectedComplexModels.Count == 0)
            {
                if (SelectedPoints.Count == 2)
                {
                    if (ImGui.Button("Merge Points"))
                    {
                        Scene.Instance.MergePoints(SelectedPoints[0], SelectedPoints[1]);
                    }
                    ImGui.Separator();
                }

                var pos = _currentPos;
                var tmp = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
                if (ImGui.DragFloat3("Position", ref tmp, 0.05f))
                {
                    TranslateSelection(new Vector3(tmp.X, tmp.Y, tmp.Z));
                }

                var rot = MathUtils.Rad2Deg(_currentRot);
                tmp = new System.Numerics.Vector3(rot.X, rot.Y, rot.Z);
                if (ImGui.DragFloat3("Rotation", ref tmp, 2f))
                {
                    RotateSelection(new Vector3(tmp.X, tmp.Y, tmp.Z));
                }

                var scale = _currentScale;
                tmp = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);
                if (ImGui.DragFloat3("Scale", ref tmp, 0.1f))
                {
                    RescaleSelection(new Vector3(tmp.X, tmp.Y, tmp.Z));
                }

                ImGui.Separator();

                ImGui.Text("Now editing");

                foreach (var model in _selectedSimpleModels)
                {
                    ImGui.BulletText(model.Name);
                }
            }
            else if (_selectedIntersections.Count != 0)
            {
                ImGui.Separator();

                ImGui.Text("Now editing");

                foreach (var model in _selectedIntersections)
                {
                    ImGui.BulletText(model.Name);
                }
            }
            else
            {
                ImGui.Separator();

                ImGui.Text("Now editing");

                foreach (var model in _selectedComplexModels)
                {
                    ImGui.BulletText(model.Name);
                }
            }
        }
    }
}
