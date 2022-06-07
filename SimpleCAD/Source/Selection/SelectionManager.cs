using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using OpenTK;
using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.GUI;

namespace SimpleCAD.Source.Utils
{
    public sealed class SelectionManager : ISceneGUIElement
    {
        private SelectionManager() {
            _selectedModels = new HashSet<BasicSceneModel>();
            _selectionCache = new Dictionary<BasicSceneModel, Matrix4>();
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
        public bool Selected => _selectedModels.Count > 0;
        public List<BasicSceneModel> SelectedControlPoints => _selectedModels.Where(x => x.IsControlPoint).ToList();
        public bool ControlPointModelSelected => _selectedControlPointModel != null;
        public int Count => _selectedModels.Count;
        

        private Vector3 _originalPivot;

        private Vector3 _currentPos;
        private Vector3 _currentRot;
        private Vector3 _currentScale;

        private HashSet<BasicSceneModel> _selectedModels;
        private Dictionary<BasicSceneModel, Matrix4> _selectionCache;
        private ControlPointSceneModel _selectedControlPointModel;

        public void Add(BasicSceneModel model)
        {
            _selectedModels.Add(model);
            ResetState();
        }

        public void Add(ControlPointSceneModel model)
        {
            _selectedControlPointModel = model;
        }

        public void Remove(BasicSceneModel model)
        {
            if (_selectedModels.Contains(model))
                _selectedModels.Remove(model);
            ResetState();
        }

        public void Remove(ControlPointSceneModel model)
        {
            _selectedControlPointModel = null;
        }

        public void Clear()
        {
            _selectedModels.Clear();
            _selectionCache.Clear();
        }

        public bool TryGetSingleSelected(out SceneModel model)
        {
            if (_selectedControlPointModel != null)
            {
                model = _selectedControlPointModel;
                return true;
            }

            model = null;
            if (Count != 1)
                return false;

            model = _selectedModels.Single();
            return true;
        }

        public bool IsSelected(SceneModel model)
        {
            return _selectedModels.Contains(model) || model == _selectedControlPointModel;
        }

        public int SelectedCount()
        {
            return _selectedModels.Count;
        }

        public void ResetState()
        {
            _selectionCache.Clear();
            _currentPos = Vector3.Zero;
            _currentRot = Vector3.Zero;
            _currentScale = Vector3.One;

            foreach (var model in _selectedModels)
            {
                _selectionCache.Add(model, model.Transform);
            }
            
            _originalPivot = RecalculatePivot();
        }

        public Vector3 RecalculatePivot()
        {
            var pivot = Vector3.Zero;

            foreach (var model in _selectedModels)
            {
                pivot += model.Position;
            }

            if (Count > 1)
            {
                pivot /= Count;
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

        private (Vector3 translation, Vector3 xyzRot, Vector3 scale) DecomposeMatrix(Matrix4 trs)
        {
            var t = trs.Row3.Xyz;

            var s = new Vector3(trs.Row0.Xyz.Length, trs.Row1.Xyz.Length, trs.Row2.Xyz.Length);

            var R = new Matrix3(trs.Row0.Xyz.Normalized(), trs.Row1.Xyz.Normalized(), trs.Row2.Xyz.Normalized());

            var euler = Vector3.Zero;

            R.Transpose();

            if(R.M13 < +1)
            {
                if(R.M13 > -1)
                {
                    euler.Y = (float)Math.Asin(R.M13);
                    euler.X = (float)Math.Atan2(-R.M23, R.M33);
                    euler.Z = (float)Math.Atan2(-R.M12, R.M11);
                }
                else
                {
                    euler.Y = (float)(-Math.PI / 2);
                    euler.X = (float)Math.Atan2(R.M21, R.M22);
                    euler.Z = 0;
                }
            }
            else
            {
                euler.Y = (float)(Math.PI / 2);
                euler.X = (float)Math.Atan2(R.M21, R.M22);
                euler.Z = 0;
            }

            return (t, euler, s);
        }

        private void MoveSelection()
        {
            foreach (var model in _selectedModels)
            {
                if (model is BasicSceneModel basicModel)
                {
                    var transform = _selectionCache[basicModel];

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

                    (var translation, var rotation, var scale) = DecomposeMatrix(transform);

                    basicModel.Translate(translation);
                    basicModel.Rotate(rotation * 180f / (float)Math.PI);
                    basicModel.Rescale(scale);
                }
            }
        }

        public void DrawElementGUI()
        {
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

            foreach (var model in _selectedModels)
            {
                ImGui.BulletText(model.Name);
            }
        }
    }
}
