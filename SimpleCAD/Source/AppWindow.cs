using System;
using System.Text;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Selection;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source
{


    class AppWindow : GameWindow
    {
        private ImGuiController _controller;
        private Color4 _bgColor;
        private GUIState _uiState;

        private enum SurfaceType
        {
            C0,
            C2
        }

        public AppWindow(int width, int height, string title) : base(width, height, GraphicsMode.Default) 
        {
            Title = title;

            var scene = Scene.Instance;

            _uiState = new GUIState()
            {
                settingsEnabled = false,
                fov = 60,
                nearZ = 0.5f,
                farZ = 100,
                cameraSpeed = 0.03f,
                focusDistance = 10f,
                eyeDistance = 0.5f,
                stereo = false,
                gridX = true,
                gridY = false,
                gridZ = false,
                surfWidth = 5f,
                surfHeight = 5f,
                cylinderHeight = 5f,
                cylinderRadius = 1.5f,
                nSurfaceU = 1,
                nSurfaceV = 1,
                surfaceType = SurfaceType.C0
            };

            scene.camera.ChangeAspect(Width, Height);
            scene.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
            scene.camera.ChangeAnaglyphConfig(_uiState.eyeDistance, _uiState.focusDistance);
            scene.camera.SetRenderMode(Camera.RenderMode.Default);
            scene.camera.RefreshMatrices();
        }

        
        private void DrawMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit editor", "Esc"))
                    {
                        this.Close();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("View"))
                {
                    ImGui.Checkbox("Settings", ref _uiState.settingsEnabled);
                    ImGui.Text("Grid");
                    ImGui.SameLine();
                    ImGui.Checkbox("X", ref _uiState.gridX);
                    ImGui.SameLine();
                    ImGui.Checkbox("Y", ref _uiState.gridY);
                    ImGui.SameLine();
                    ImGui.Checkbox("Z", ref _uiState.gridZ);
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        private void DrawCameraSettings()
        {
            if (_uiState.settingsEnabled && ImGui.Begin("Settings", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Camera");

                if (ImGui.Checkbox("Orthographic projection", ref _uiState.ortho))
                {
                    Scene.Instance.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
                }

                if (ImGui.SliderFloat("Field of view", ref _uiState.fov, 5, 120, null, ImGuiSliderFlags.Logarithmic))
                {
                    Scene.Instance.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
                }

                if (ImGui.SliderFloat("Near clipping plane", ref _uiState.nearZ, 0.1f, _uiState.farZ / 2, null, ImGuiSliderFlags.Logarithmic))
                {
                    Scene.Instance.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
                }

                if (ImGui.SliderFloat("Far clipping plane", ref _uiState.farZ, 2, 500, null, ImGuiSliderFlags.Logarithmic))
                {
                    if (_uiState.farZ / 2 <= _uiState.nearZ)
                        _uiState.nearZ = _uiState.farZ / 2;
                    Scene.Instance.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
                }

                ImGui.SliderFloat("Camera movement speed", ref _uiState.cameraSpeed, 0.02f, 0.5f, null, ImGuiSliderFlags.Logarithmic);

                ImGui.Separator();

                ImGui.Text("Stereoscopy");

                if (ImGui.Checkbox("Stereoscopy", ref _uiState.stereo))
                {
                    Scene.Instance.camera.SetRenderMode(_uiState.stereo ? Camera.RenderMode.Anaglyph : Camera.RenderMode.Default);
                }
                if (ImGui.DragFloat("Eye separation", ref _uiState.eyeDistance, 0.02f, 0.0f, 10f)) 
                {
                    Scene.Instance.camera.ChangeAnaglyphConfig(_uiState.eyeDistance, _uiState.focusDistance);
                }
                if (ImGui.DragFloat("Focus distance", ref _uiState.focusDistance, 0.02f, 0.1f, 10f))
                {
                    Scene.Instance.camera.ChangeAnaglyphConfig(_uiState.eyeDistance, _uiState.focusDistance);
                }

                // This should probably contain some other menu that will allow us to traverse scene hierarchy, create new objects 

                ImGui.End();
            }
        }

        private void DrawBasicModelGUI(BasicSceneModel model, int index)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;

            if (model.Deletable)
            {
                if (ImGui.Button("Delete##" + index))
                {
                    scene.RemoveModel(model);
                }
                ImGui.SameLine();
            }
            
            if (!selection.IsSelected(model))
            {
                if (ImGui.Button("Select##" + index))
                {
                    selection.Add(model);
                }
            }
            else
            {
                if (ImGui.Button("Deselect##" + index))
                {
                    selection.Remove(model);
                }
            }

            ImGui.SameLine();
            ImGui.InputText("##SceneModel" + index, model.name, SceneModel.MAX_NAME_LEN);
            
            ImGui.Separator();
        }

        private void DrawComplexModelGUI(ControlPointSceneModel model, int index)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;

            if (ImGui.Button("Delete##" + index))
            {
                scene.RemoveModel(model);
            }
            else
            {
                ImGui.SameLine();
                if (!selection.IsSelected(model))
                {
                    if (ImGui.Button("Select##" + index))
                    {
                        selection.Add(model);
                    }
                }
                else
                {
                    if (ImGui.Button("Deselect##" + index))
                    {
                        selection.Remove(model);
                    }
                }
                ImGui.SameLine();
                ImGui.InputText("##SceneModel" + index, model.name, SceneModel.MAX_NAME_LEN);
            }
            ImGui.Separator();
        }

        private void DrawScenePanel()
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;
             
            if (ImGui.Begin("Scene", 
                ImGuiWindowFlags.AlwaysVerticalScrollbar | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove | 
                ImGuiWindowFlags.NoCollapse))
            {
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, Height / 3));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Width - 300, 18));

                ImGui.Checkbox("Show control points", ref _uiState.showControlPoints);

                ImGui.Separator();

                for (int i = 0; i < scene.basicModels.Count; i++)
                {
                    if (!scene.basicModels[i].IsControlPoint || _uiState.showControlPoints)
                        DrawBasicModelGUI(scene.basicModels[i], i);
                }

                for (int i = 0; i < scene.complexModels.Count; i++)
                {
                    DrawComplexModelGUI(scene.complexModels[i], scene.basicModels.Count + i);
                }

                if (ImGui.Button("+", new System.Numerics.Vector2(20, 20)))
                {
                    _uiState.createMenuVisible = !_uiState.createMenuVisible;
                }

                if (_uiState.createMenuVisible)
                {
                    if (ImGui.Button("Point", new System.Numerics.Vector2(100, 20)))
                    {
                        var point = new BasicSceneModel(
                            new Point(ColorPalette.DeselectedColor), 
                            "Point " + (scene.basicModels.Count + 1), 
                            PrimitiveType.Points, true);
                        scene.AddModel(point);
                        _uiState.createMenuVisible = false;
                        if (selection.ControlPointModelSelected &&
                            selection.TryGetSingleSelected(out SceneModel model) &&
                            model is ControlPointSceneModel controlPointModel)
                        {
                            controlPointModel.AddPoint(point);
                        }
                    }

                    if (ImGui.Button("Torus", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new BasicSceneModel(
                            new Torus(40, 2, 1, ColorPalette.DeselectedColor), 
                            "Torus " + (scene.basicModels.Count + 1), 
                            PrimitiveType.Lines));
                        _uiState.createMenuVisible = false;
                    }

                    if (ImGui.Button("Quad", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new BasicSceneModel(
                            new Quad(ColorPalette.DeselectedColor), 
                            "Quad " + (scene.basicModels.Count + 1), 
                            PrimitiveType.Triangles));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedControlPoints.Count > 0 && ImGui.Button("Bezier (C0)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new BezierCurveSceneModel(
                            new C0BezierCurve(), 
                            "C0 Bezier " + (scene.complexModels.Count + 1)));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedControlPoints.Count > 3 && ImGui.Button("Spline (C2)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new BezierCurveSceneModel(
                            new C2SplineCurve(), 
                            "C2 Spline " + (scene.complexModels.Count + 1)));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedControlPoints.Count > 0 && ImGui.Button("Interpol. (C2)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new BezierCurveSceneModel(
                            new C2InterpolatingCurve(), 
                            "Interpolation" + (scene.complexModels.Count + 1)));
                        _uiState.createMenuVisible = false;
                    }

                    if (ImGui.Button("Surface (C0)", new System.Numerics.Vector2(100, 20)))
                    {
                        _uiState.surfaceType = SurfaceType.C0;
                        ImGui.OpenPopup("Surface Creator");
                    }

                    if (ImGui.Button("Surface (C2)", new System.Numerics.Vector2(100, 20)))
                    {
                        _uiState.surfaceType = SurfaceType.C2;
                        ImGui.OpenPopup("Surface Creator");
                    }

                    var open = true;

                    if (ImGui.BeginPopupModal("Surface Creator", ref open, ImGuiWindowFlags.NoResize))
                    {
                        ImGui.Text("Surface wrap mode");
                        if (ImGui.RadioButton("Wrap", _uiState.cylinder))
                        {
                            _uiState.cylinder = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.RadioButton("Flat", !_uiState.cylinder))
                        {
                            _uiState.cylinder = false;
                        }

                        ImGui.Separator();

                        if (!_uiState.cylinder)
                        {
                            ImGui.SetNextItemWidth(100f);
                            ImGui.DragFloat("Width", ref _uiState.surfWidth, 0.1f, 0.1f, 100);
                            ImGui.SetNextItemWidth(100f);
                            ImGui.DragFloat("Height", ref _uiState.surfHeight, 0.1f, 0.1f, 100);
                        } 
                        else
                        {
                            ImGui.SetNextItemWidth(100f);
                            ImGui.DragFloat("Radius", ref _uiState.cylinderRadius, 0.1f, 0.1f, 100);
                            ImGui.SetNextItemWidth(100f);
                            ImGui.DragFloat("Height", ref _uiState.cylinderHeight, 0.1f, 0.1f, 100);
                        }

                        ImGui.Separator();
                        
                        ImGui.SetNextItemWidth(100f);
                        ImGui.InputInt("Segments (u)", ref _uiState.nSurfaceU, 1);
                        _uiState.nSurfaceU = Math.Max(_uiState.nSurfaceU, _uiState.cylinder ? 2 : 1);
                        _uiState.nSurfaceU = Math.Min(_uiState.nSurfaceU, 8);
                        
                        ImGui.SetNextItemWidth(100f);
                        ImGui.InputInt("Segments (v)", ref _uiState.nSurfaceV, 1);
                        _uiState.nSurfaceV = Math.Max(_uiState.nSurfaceV, 1);
                        _uiState.nSurfaceV = Math.Min(_uiState.nSurfaceV, 8);

                        if (ImGui.Button("Accept"))
                        {
                            ImGui.CloseCurrentPopup();

                            Surface geom = null;

                            if (_uiState.surfaceType == SurfaceType.C0) 
                            {
                                geom = new C0BezierSurface(_uiState.nSurfaceU, _uiState.nSurfaceV, _uiState.cylinder);
                            } 
                            else if (_uiState.surfaceType == SurfaceType.C2)
                            {
                                geom = new C2SplineSurface(_uiState.nSurfaceU, _uiState.nSurfaceV, _uiState.cylinder);
                            }

                            if (!_uiState.cylinder)
                            {
                                scene.AddModel(new SurfaceSceneModel(geom,
                                "Surface " + (scene.complexModels.Count + 1)),
                                _uiState.surfWidth, _uiState.surfHeight);
                            }
                            else
                            {
                                scene.AddModel(new SurfaceSceneModel(geom,
                                "Cylinder " + (scene.complexModels.Count + 1)),
                                _uiState.cylinderRadius, _uiState.cylinderHeight);
                            }
                            
                            _uiState.createMenuVisible = false;
                        }

                        ImGui.SetWindowSize(new System.Numerics.Vector2(200, ImGui.GetContentRegionAvail().Y));
                        ImGui.EndPopup();
                    }
                }

                ImGui.End();
            }
        }

        private void DrawCursorPanel()
        {
            var scene = Scene.Instance;

            if (ImGui.Begin("Cursor", 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove | 
                ImGuiWindowFlags.NoCollapse))
            {
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, Height / 9));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Width - 300, 18 + Height / 3));
                var tmp = new System.Numerics.Vector3(scene.cursorPos.X, scene.cursorPos.Y, scene.cursorPos.Z);
                if (ImGui.DragFloat3("Position##Cursor", ref tmp, 0.05f))
                {
                    scene.cursorPos = new Vector3(tmp.X, tmp.Y, tmp.Z);
                }
                ImGui.SliderFloat("Distance", ref scene.cursorRaycastDist, 0.5f, 40f);

                ImGui.End();
            }
        }

        private void DrawInspectorPanel()
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;

            if (ImGui.Begin("Inspector", 
                ImGuiWindowFlags.AlwaysVerticalScrollbar | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove | 
                ImGuiWindowFlags.NoCollapse))
            {
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, 5 * Height / 9 - 18));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Width - 300, 18 + 4 * Height / 9));

                if (selection.TryGetSingleSelected(out var model))
                { 
                    model.DrawElementGUI();
                } else if (selection.Count > 1)
                {
                    SelectionManager.Instance.DrawElementGUI();
                }

                ImGui.End();
            }
        }
        private void DrawGUI()
        {
            DrawMenuBar();
            DrawCameraSettings();
            DrawScenePanel();
            DrawCursorPanel();
            DrawInspectorPanel();
        }

        void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            _controller.PressChar(e.KeyChar);
        }

        void OnMousePress(object sender, MouseButtonEventArgs e)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;
            var raycaster = SceneRaycaster.Instance;

            if (!_uiState.ortho)
            {
                if (e.Button == MouseButton.Left)
                {
                    if (raycaster.RaycastBasicSearch(new Vector2(e.X, e.Y), new Vector2(Width, Height), out var model))
                    {
                        if (InputWrapper.KeyDown(Key.X))
                        {
                            selection.Remove(model);
                        }
                        else
                        {
                            scene.capturedModel = model;
                            if (!InputWrapper.KeyDown(Key.ShiftLeft))
                            {
                                selection.Clear();
                            }
                            selection.Add(model);
                        }
                    }
                    else if (raycaster.RaycastVirtualSearch(new Vector2(e.X, e.Y), new Vector2(Width, Height), out var point))
                    {
                        scene.capturedPoint = point;
                    }
                }

                if (e.Button == MouseButton.Left && InputWrapper.KeyDown(Key.P))
                {
                    var ray = raycaster.Raycast(new Vector2(e.X, e.Y), new Vector2(Width, Height));
                    var cameraPos = scene.camera.Position;
                    scene.cursorPos = ray * scene.cursorRaycastDist + cameraPos;
                    var point = new BasicSceneModel(new Point(ColorPalette.DeselectedColor), "Point " + (scene.basicModels.Count + 1), PrimitiveType.Points, true);
                    scene.AddModel(point);
                    _uiState.createMenuVisible = false;
                    if (selection.ControlPointModelSelected &&
                        selection.TryGetSingleSelected(out SceneModel model) &&
                        model is ControlPointSceneModel controlPointModel)
                    {
                        controlPointModel.AddPoint(point);
                    }
                }

                if (e.Button == MouseButton.Left && InputWrapper.KeyDown(Key.AltLeft))
                {
                    var ray = raycaster.Raycast(new Vector2(e.X, e.Y), new Vector2(Width, Height));
                    var cameraPos = scene.camera.Position;
                    scene.cursorPos = ray * scene.cursorRaycastDist + cameraPos;
                }
            }
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;
            var raycaster = SceneRaycaster.Instance;

            if (!_uiState.ortho)
            {
                var ray = raycaster.Raycast(new Vector2(e.X, e.Y), new Vector2(Width, Height));
                var cameraPos = scene.camera.Position;
                
                if (scene.capturedPoint != null)
                {
                    var dist = (cameraPos - scene.capturedPoint.Position).Length;
                    var pos = cameraPos + ray * dist;

                    if (InputWrapper.KeyDown(Key.Z))
                    {
                        pos = new Vector3((int)(2 * pos.X) / 2f, (int)(2 * pos.Y) / 2f, (int)(2 * pos.Z) / 2f);
                    }

                    scene.capturedPoint.parent.MoveVirtualPoint(scene.capturedPoint, pos);
                } 
                else if (scene.capturedModel != null)
                {
                    var dist = (cameraPos - scene.capturedModel.Position).Length;
                    var pos = cameraPos + ray * dist;

                    if (InputWrapper.KeyDown(Key.Z))
                    {
                        pos = new Vector3((int)(2 * pos.X) / 2f, (int)(2 * pos.Y) / 2f, (int)(2 * pos.Z) / 2f);
                    }

                    scene.capturedModel.Translate(pos);
                    selection.ResetState();
                }
            }
        }
        
        void OnMouseRelease(object sender, MouseButtonEventArgs e)
        {
            var scene = Scene.Instance;

            if (!_uiState.ortho)
            {
                scene.capturedModel = null;
                scene.capturedPoint = null;
            }
        }

        private void ResetScene()
        {
            var scene = Scene.Instance;

            scene.complexModels.Clear();
            scene.basicModels.Clear();
        }

        private void HandleKeyboardInput()
        {
            var scene = Scene.Instance;

            if (InputWrapper.GetKeyDown(Key.Escape))
            {
                this.Close();
                return;
            }

            if (InputWrapper.KeyDown(Key.ControlLeft) && InputWrapper.KeyDown(Key.A))
            {
                var selection = SelectionManager.Instance;
                selection.Clear();
                foreach (var model in scene.basicModels)
                {
                    selection.Add(model);
                }
                return;
            }

            var d = _uiState.cameraSpeed;

            if (InputWrapper.KeyDown(Key.W))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, 0, -d, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, 0, -d, 0)).Xyz, true);
            }

            if (InputWrapper.KeyDown(Key.A))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(-d, 0, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(-d, 0, 0, 0)).Xyz, true);
            }

            if (InputWrapper.KeyDown(Key.S))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, 0, d, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, 0, d, 0)).Xyz, true);
            }

            if (InputWrapper.KeyDown(Key.D))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(d, 0, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(d, 0, 0, 0)).Xyz, true);
            }

            if (InputWrapper.KeyDown(Key.Q))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, d, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, d, 0, 0)).Xyz, true);
            }

            if (InputWrapper.KeyDown(Key.E))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, -d, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, -d, 0, 0)).Xyz, true);
            }
        }

        private void HandleMouseInput()
        {
            var scene = Scene.Instance;

            if (InputWrapper.KeyDown(MouseButton.Right) && InputWrapper.KeyDown(Key.ControlLeft))
            {
                var delta = InputWrapper.MouseDeltaPos();

                scene.camera.RotateAround((scene.camera.viewM * new Vector4(Vector3.UnitY, 0)).Xyz.Normalized(), 0.005f * -delta.X);
                scene.camera.RotateAround((scene.camera.viewM * new Vector4(Vector3.UnitX, 0)).Xyz.Normalized(), 0.005f * -delta.Y);
            }

            if (InputWrapper.KeyDown(MouseButton.Left) && InputWrapper.KeyDown(Key.ControlLeft))
            {
                var delta = InputWrapper.MouseDeltaPos();

                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0.01f * delta.X, 0.01f * delta.Y, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0.01f * delta.X, 0.01f * delta.Y, 0, 0)).Xyz, true);
            }

            var zoomDelta = InputWrapper.MouseWheel();

            if (zoomDelta != 0)
            {
                scene.camera.ZoomCamera(-zoomDelta);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
           
            var scene = Scene.Instance;

            InputWrapper.OnUpdateFrame();

            HandleKeyboardInput();
            HandleMouseInput();

            scene.RefreshScene();

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            var scene = Scene.Instance;

            _bgColor = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            _controller = new ImGuiController(Width, Height);
           
            scene.cursorRaycastDist = 10f;
            scene.SetBackgroundColor(_bgColor);

            KeyPress += OnKeyPress;
            MouseDown += OnMousePress;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseRelease;

            CursorVisible = true;
            VSync = VSyncMode.On;

            GL.ClearColor(_bgColor);
            GL.PointSize(5);
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            var scene = Scene.Instance;

            scene.Unload();

            base.OnUnload(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            var scene = Scene.Instance;

            _controller.Update(this, 0.05f);

            GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.Enable(EnableCap.DepthTest);

            scene.camera.RefreshMatrices();
            scene.RenderScene(_uiState.gridX, _uiState.gridY, _uiState.gridZ);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            DrawGUI();
            _controller.RenderGUI();

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            var scene = Scene.Instance;

            GL.Viewport(0, 0, Width, Height);

            scene.camera.ChangeAspect(Width, Height);
            _controller.WindowResized(Width, Height);

            base.OnResize(e);
        }

        private struct GUIState
        {
            public bool settingsEnabled;
            public bool createMenuVisible;
            public float fov;
            public float nearZ;
            public float farZ;
            public float eyeDistance;
            public float focusDistance;
            public bool stereo;
            public bool ortho;
            public float cameraSpeed;
            public bool gridX, gridY, gridZ;
            public float surfWidth, surfHeight;
            public float cylinderRadius, cylinderHeight;
            public int nSurfaceU, nSurfaceV;
            public bool showControlPoints;
            public bool cylinder;
            public SurfaceType surfaceType;
        }
    }
}
