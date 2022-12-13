using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SimpleCAD.Source.Environment
{
    public class Camera
    {
        public Matrix4 viewM;
        public Matrix4 projectionM;

        public RenderMode Mode => _renderMode;
        public Vector3 Position => _pos;
        public float NearZ => _nearZ;
        public float FarZ => _farZ;
        public float FOV => _fov;
        public float Aspect => _aspect;

        public Rectangle Viewport => new Rectangle(0, 0, (int)_width, (int)_height);

        private float _width, _height;
        private Vector3 _pos;
        private Vector3 _target;
        private float _fov;
        private float _aspect;
        private float _nearZ, _farZ;
        private bool _ortho;
        private float _focus;
        private float _eyeDistance;
        private RenderMode _renderMode;
        private bool _refresh = true;

        public enum RenderMode
        {
            Default,
            Anaglyph
        }

        public Camera()
        {
            _pos = Vector3.Zero;
            _focus = 10f;
            _eyeDistance = 0.5f;
            _renderMode = RenderMode.Default;
        }

        public void RefreshMatrices()
        {
            if (_refresh)
            {
                viewM = Matrix4.LookAt(_pos, _target, Vector3.UnitY);
                if (!_ortho)
                {
                    projectionM = Matrix4.CreatePerspectiveFieldOfView(_fov, _aspect, _nearZ, _farZ);
                }
                else
                {
                    projectionM = Matrix4.CreateOrthographic(_aspect * _fov * 20, _fov * 20, _nearZ, _farZ);
                }
            }
        }

        public void ToggleRefresh(bool refresh)
        {
            _refresh = refresh;
        }

        public void ChangeViewportConfig(float fov, float nearZ, float farZ, bool ortho)
        {
            _fov = (float)Math.PI * fov / 180f; 
            _nearZ = nearZ;
            _farZ = farZ;
            _ortho = ortho;
        }

        public void ChangeAspect(Vector2 size)
        {
            _width = size.X;
            _height = size.Y;
            _aspect = _width/_height;
        }

        public void ChangeAnaglyphConfig(float eyeDistance, float focusDistance)
        {
            _eyeDistance = eyeDistance;
            _focus = focusDistance;
        }

        public void SetRenderMode(RenderMode mode)
        {
            _renderMode = mode;
        }

        public void TranslateCamera(Vector3 translation, bool additive = false)
        {
            _pos = additive ? _pos + translation : translation;
        }

        public void MoveCameraTarget(Vector3 target, bool additive = false)
        {
            _target = additive ? _target + target : target;
        }

        public void ZoomCamera(float delta)
        {
            Vector3 dir = (_pos - _target).Normalized();

            var translated = _pos + delta * 0.5f * dir;

            if (translated.Length > (dir + translated).Length || translated.Length <= 1)
            {
                _pos = _target + dir.Normalized();
            } 
            else
            {
                _pos = translated;
            }
        }

        private void RenderSceneElements(in List<RenderableElement> elements, in List<(Gizmo gizmo, Vector3 pos)> instanced)
        {
            foreach (var element in elements)
            {
                element.Render();
            }
            foreach (var instance in instanced)
            {
                instance.gizmo.RenderGizmo(instance.pos);
            }
        }

        public void CustomRender(in List<RenderableElement> elements, Matrix4 view, Matrix4 proj)
        {
            var originalView = viewM;
            var originalProj = projectionM;

            viewM = view;
            projectionM = proj;

            foreach (var element in elements)
            {
                element.Render();
            }
        }

        public void Render(in List<RenderableElement> elements, in List<(Gizmo gizmo, Vector3 pos)> instanced) {

            if (_renderMode == RenderMode.Default)
            {
                RenderSceneElements(elements, instanced);
                return;
            }

            if (_renderMode == RenderMode.Anaglyph)
            {
                var view = viewM;

                var invView = Matrix4.Invert(viewM);

                var leftEyePos = (new Vector4(-_eyeDistance / 2f, 0, 0, 1) * invView).Xyz;
                var rightEyePos = (new Vector4(_eyeDistance / 2f, 0, 0, 1) * invView).Xyz;

                Vector3 focusPoint = _target - _pos;
                focusPoint = Vector3.Normalize(focusPoint);
                focusPoint *= _focus;
                focusPoint += _pos;

                var leftEye = Matrix4.LookAt(leftEyePos, focusPoint, Vector3.UnitY);
                var rightEye = Matrix4.LookAt(rightEyePos, focusPoint, Vector3.UnitY);

                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);

                viewM = leftEye;
                GL.ColorMask(true, false, false, false);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                RenderSceneElements(elements, instanced);

                viewM = rightEye;
                GL.ColorMask(false, true, true, false);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                RenderSceneElements(elements, instanced);

                viewM = view;

                GL.Disable(EnableCap.Blend);

                GL.ColorMask(true, true, true, true);
            }
            
        }

        public void RotateAround(Vector3 axis, float angle)
        {
            Vector3 pos = _pos;
            Quaternion rot = Quaternion.FromAxisAngle(axis, angle); // get the desired rotation
            Vector3 dir = pos - _target; // find current direction relative to center
            dir = rot * dir; // rotate the direction
            _pos = _target + dir; // define new position
                                                
            viewM = Matrix4.LookAt(_pos, _target, Vector3.UnitY);
        }
    }
}
