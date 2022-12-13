using ImGuiNET;
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Selection;
using SimpleCAD.Source.Utils;
using SimpleCAD.Source.Serialization;
using SharpSceneSerializer;
using SimpleCAD.Source.Paths;

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

        public AppWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {
            var scene = Scene.Instance;

            _uiState = new GUIState()
            {
                settingsEnabled = false,
                useCursorStartPoint = false,
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

            scene.camera.ChangeAspect(Size);
            scene.camera.ChangeViewportConfig(_uiState.fov, _uiState.nearZ, _uiState.farZ, _uiState.ortho);
            scene.camera.ChangeAnaglyphConfig(_uiState.eyeDistance, _uiState.focusDistance);
            scene.camera.SetRenderMode(Camera.RenderMode.Default);
            scene.camera.RefreshMatrices();
        }

        
        private void DrawMenuBar()
        {
            bool openLoad = false;
            bool openSave = false;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    
                    if (ImGui.MenuItem("Save as", "Ctrl+S"))
                    {
                        openSave = true;
                    }

                    if (ImGui.MenuItem("Load", "Ctrl+L"))
                    {
                        openLoad = true;
                    }

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

                if (openSave)
                    ImGui.OpenPopup("Save File");

                if (openLoad)
                    ImGui.OpenPopup("Load File");

                var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

                if (ImGui.BeginPopup("Save File"))
                {
                    var picker = FilePicker.GetFilePicker(this, path, ".json", false, true);
                    if (picker.Draw())
                    {
                        if (picker.SelectedFile != null)
                        {
                            var data = SceneSerializationUtils.ConvertTo();
                            SceneSerializer.Serialize(data, picker.SelectedFile);
                        }
                        FilePicker.RemoveFilePicker(this);
                    }
                    ImGui.EndPopup();
                }

                if (ImGui.BeginPopup("Load File"))
                {
                    var picker = FilePicker.GetFilePicker(this, path, ".json");
                    if (picker.Draw())
                    {
                        if (picker.SelectedFile != null)
                        {
                            (var success, var data) = SceneSerializer.Deserialize(picker.SelectedFile, AppContext.BaseDirectory + "/schema.json");

                            if (success)
                            {
                                SceneSerializationUtils.ConvertFrom(data);
                            }
                        }
                        FilePicker.RemoveFilePicker(this);
                    }
                    ImGui.EndPopup();
                }

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

                if (ImGui.Checkbox("Refresh", ref _uiState.refreshCamera))
                {
                    Scene.Instance.camera.ToggleRefresh(_uiState.refreshCamera);
                }

                // This should probably contain some other menu that will allow us to traverse scene hierarchy, create new objects 

                ImGui.End();
            }
        }

        private void DrawPointModelGUI(PointSceneModel model, int index)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;

            if (model.Deletable)
            {
                if (ImGui.Button("Delete##" + index + "P"))
                {
                    scene.RemoveModel(model);
                }
                ImGui.SameLine();
            }

            if (!selection.IsSelected(model))
            {
                if (ImGui.Button("Select##" + index + "P"))
                {
                    selection.Add(model);
                }
            }
            else
            {
                if (ImGui.Button("Deselect##" + index + "P"))
                {
                    selection.Remove(model);
                }
            }

            ImGui.SameLine();
            ImGui.InputText("##SceneModel" + index + "P", model.name, SceneModel.MAX_NAME_LEN);

            ImGui.Separator();
        }

        private void DrawBasicModelGUI(SimpleSceneModel model, int index)
        {
            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;

            if (ImGui.Button("Delete##" + index + "B"))
            {
                scene.RemoveModel(model);
            }
            ImGui.SameLine();
            
            if (!selection.IsSelected(model))
            {
                if (ImGui.Button("Select##" + index + "B"))
                {
                    selection.Add(model);
                }
            }
            else
            {
                if (ImGui.Button("Deselect##" + index + "B"))
                {
                    selection.Remove(model);
                }
            }

            ImGui.SameLine();
            ImGui.InputText("##SceneModel" + index + "B", model.name, SceneModel.MAX_NAME_LEN);
            
            ImGui.Separator();
        }

        private void DrawComplexModelGUI(ComplexSceneModel model, int index)
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
                    if (ImGui.Button("Select##" + index + "C"))
                    {
                        selection.Add(model);
                    }
                }
                else
                {
                    if (ImGui.Button("Deselect##" + index + "C"))
                    {
                        selection.Remove(model);
                    }
                }
                ImGui.SameLine();
                ImGui.InputText("##SceneModel" + index + "C", model.name, SceneModel.MAX_NAME_LEN);
            }
            ImGui.Separator();
        }

        private void DrawIntersectionModelGUI(IntersectionSceneModel model, int index)
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
                    if (ImGui.Button("Select##" + index + "I"))
                    {
                        selection.Add(model);
                    }
                }
                else
                {
                    if (ImGui.Button("Deselect##" + index + "I"))
                    {
                        selection.Remove(model);
                    }
                }
                ImGui.SameLine();
                ImGui.InputText("##SceneModel" + index + "I", model.name, SceneModel.MAX_NAME_LEN);
            }
            ImGui.Separator();
        }

        private void DrawMillingPanel()
        {
            var creator = MillingPathCreator.Instance;

            if (ImGui.Begin("Milling", ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {
                if (ImGui.Button("Generate rough path"))
                {
                    creator.SetUpFramebuffer(15, 15, 5);
                    var path = creator.GenerateRoughMillingPath(1.6f);
                    path.Save(Directory.GetCurrentDirectory(), "rough");
                }

                if (ImGui.Button("Generate perimeter path"))
                {
                    var path = creator.GeneratePerimeterMillingPath(1.2f);
                    path.Save(Directory.GetCurrentDirectory(), "perimeter");
                }

                if (ImGui.Button("Generate precise path"))
                {
                    var path = creator.GeneratePreciseMillingPath(0.8f);
                    path.Save(Directory.GetCurrentDirectory(), "precise");
                }

                if (creator.HeightmapHandle != default) {
                    ImGui.Image((IntPtr)creator.HeightmapHandle, new System.Numerics.Vector2(300, 300));
                }
                ImGui.End();
            }
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
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, Size.Y / 3));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Size.X - 300, 18));

                ImGui.Checkbox("Show control points", ref _uiState.showControlPoints);

                ImGui.Separator();

                if (_uiState.showControlPoints)
                {
                    for (int i = 0; i < scene.pointModels.Count; i++)
                    {
                        DrawPointModelGUI(scene.pointModels[i], i);
                    }
                }

                for (int i = 0; i < scene.basicModels.Count; i++)
                {
                    DrawBasicModelGUI(scene.basicModels[i], i);
                }

                for (int i = 0; i < scene.complexModels.Count; i++)
                {
                    DrawComplexModelGUI(scene.complexModels[i], scene.basicModels.Count + i);
                }

                for (int i = 0; i < scene.intersections.Count; i++)
                {
                    DrawIntersectionModelGUI(scene.intersections[i], scene.basicModels.Count + scene.complexModels.Count + i);
                }

                if (ImGui.Button("+", new System.Numerics.Vector2(20, 20)))
                {
                    _uiState.createMenuVisible = !_uiState.createMenuVisible;
                }

                if (_uiState.createMenuVisible)
                {
                    if (ImGui.Button("Point", new System.Numerics.Vector2(100, 20)))
                    {
                        var point = new PointSceneModel("Point " + (scene.basicModels.Count + 1));
                        scene.AddModel(point);
                        _uiState.createMenuVisible = false;
                        if (selection.ComplexModelSelected &&
                            selection.TryGetSingleSelected(out SceneModel model) &&
                            model is ComplexSceneModel controlPointModel)
                        {
                            controlPointModel.AddPoint(point);
                        }
                    }

                    if (ImGui.Button("Torus", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new SimpleSceneModel(
                            new Torus(40, 40, 2, 1, ColorPalette.DeselectedColor), 
                            "Torus " + (scene.basicModels.Count + 1), 
                            PrimitiveType.Lines));
                        _uiState.createMenuVisible = false;
                    }

                    if (ImGui.Button("Quad", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new SimpleSceneModel(
                            new Quad(ColorPalette.DeselectedColor), 
                            "Quad " + (scene.basicModels.Count + 1), 
                            PrimitiveType.Triangles));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedPoints.Count > 0 && ImGui.Button("Bezier (C0)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new CurveSceneModel(
                            new C0BezierCurve(), 
                            "C0 Bezier " + (scene.complexModels.Count + 1)));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedPoints.Count > 3 && ImGui.Button("Spline (C2)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new CurveSceneModel(
                            new C2SplineCurve(), 
                            "C2 Spline " + (scene.complexModels.Count + 1)));
                        _uiState.createMenuVisible = false;
                    }

                    if (selection.SelectedPoints.Count > 0 && ImGui.Button("Interpol. (C2)", new System.Numerics.Vector2(100, 20)))
                    {
                        scene.AddModel(new CurveSceneModel(
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

                    if (ImGui.BeginPopup("Surface Creator"))
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
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            ImGui.CloseCurrentPopup();
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
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, Size.Y / 9));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Size.X - 300, 18 + Size.Y / 3));
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
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, 5 * Size.Y / 9 - 18));
                ImGui.SetWindowPos(new System.Numerics.Vector2(Size.X - 300, 18 + 4 * Size.Y / 9));

                var parametricSurfaces = SelectionManager.Instance.SelectedParametricSurfaces;

                if (parametricSurfaces.Count == 2)
                {
                    ImGui.Checkbox("Near cursor", ref _uiState.useCursorStartPoint);
                    if (ImGui.Button("Find intersection"))
                    {
                        Scene.Instance.SetupIntersection(parametricSurfaces[0], parametricSurfaces[1], _uiState.useCursorStartPoint);
                    }
                    ImGui.Separator();
                }

                if (selection.TryGetSingleSelected(out var model))
                { 
                    model.DrawElementGUI();
                } else if (selection.SimpleCount > 1 || selection.ComplexCount > 1)
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
            DrawMillingPanel();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            var c = (char)e.Key;
            if (char.IsAscii(c))
            {
                _controller.PressChar(IsKeyDown(Keys.LeftShift) ? c : char.ToLower(c));
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var scene = Scene.Instance;
            var selection = SelectionManager.Instance;
            var raycaster = SceneRaycaster.Instance;

            if (!_uiState.ortho)
            {
                if (e.Button == MouseButton.Left)
                {
                    if (raycaster.RaycastBasicSearch(MousePosition, Size, out var model))
                    {
                        if (IsKeyDown(Keys.X))
                        {
                            selection.Remove(model);
                        }
                        else
                        {
                            scene.capturedModel = model;
                            if (!IsKeyDown(Keys.LeftShift))
                            {
                                selection.Clear();
                            }
                            selection.Add(model);
                        }
                    }
                    else if (raycaster.RaycastVirtualSearch(MousePosition, Size, out var point))
                    {
                        scene.capturedPoint = point;
                    }
                }

                if (e.Button == MouseButton.Left && IsKeyDown(Keys.P))
                {
                    var ray = raycaster.Raycast(MousePosition, Size);
                    var cameraPos = scene.camera.Position;
                    scene.cursorPos = ray * scene.cursorRaycastDist + cameraPos;
                    var point = new PointSceneModel("Point " + (scene.basicModels.Count + 1), true);
                    scene.AddModel(point);
                    _uiState.createMenuVisible = false;
                    if (selection.ComplexModelSelected &&
                        selection.TryGetSingleSelected(out SceneModel model) &&
                        model is ComplexSceneModel controlPointModel)
                    {
                        controlPointModel.AddPoint(point);
                    }
                }

                if (e.Button == MouseButton.Left && IsKeyDown(Keys.LeftAlt))
                {
                    var ray = raycaster.Raycast(MousePosition, Size);
                    var cameraPos = scene.camera.Position;
                    scene.cursorPos = ray * scene.cursorRaycastDist + cameraPos;
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

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

            if (IsKeyDown(Keys.Escape))
            {
                this.Close();
                return;
            }

            if (IsKeyDown(Keys.LeftControl) && IsKeyDown(Keys.A))
            {
                var selection = SelectionManager.Instance;
                selection.Clear();

                foreach (var model in scene.pointModels)
                {
                    selection.Add(model);
                }

                foreach (var model in scene.basicModels)
                {
                    selection.Add(model);
                }
                return;
            }

            var d = _uiState.cameraSpeed;

            if (IsKeyDown(Keys.W))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, 0, -d, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, 0, -d, 0)).Xyz, true);
            }

            if (IsKeyDown(Keys.A))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(-d, 0, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(-d, 0, 0, 0)).Xyz, true);
            }

            if (IsKeyDown(Keys.S))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, 0, d, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, 0, d, 0)).Xyz, true);
            }

            if (IsKeyDown(Keys.D))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(d, 0, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(d, 0, 0, 0)).Xyz, true);
            }

            if (IsKeyDown(Keys.Q))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, d, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, d, 0, 0)).Xyz, true);
            }

            if (IsKeyDown(Keys.E))
            {
                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0, -d, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0, -d, 0, 0)).Xyz, true);
            }
        }

        private void HandleMouseInput()
        {
            var scene = Scene.Instance;
            var raycaster = SceneRaycaster.Instance;
            var selection = SelectionManager.Instance;

            if (IsMouseButtonDown(MouseButton.Right) && IsKeyDown(Keys.LeftControl))
            {
                var delta = MouseState.Delta;

                scene.camera.RotateAround((scene.camera.viewM * new Vector4(Vector3.UnitY, 0)).Xyz.Normalized(), 0.005f * -delta.X);
                scene.camera.RotateAround((scene.camera.viewM * new Vector4(Vector3.UnitX, 0)).Xyz.Normalized(), 0.005f * -delta.Y);
            }

            if (IsMouseButtonDown(MouseButton.Left) && IsKeyDown(Keys.LeftControl))
            {
                var delta = MouseState.Delta;

                scene.camera.MoveCameraTarget((scene.camera.viewM * new Vector4(0.01f * delta.X, 0.01f * delta.Y, 0, 0)).Xyz, true);
                scene.camera.TranslateCamera((scene.camera.viewM * new Vector4(0.01f * delta.X, 0.01f * delta.Y, 0, 0)).Xyz, true);
            }

            var zoomDelta = MouseState.ScrollDelta.Y;

            if (zoomDelta != 0)
            {
                scene.camera.ZoomCamera(-zoomDelta);
            }

            if (!_uiState.ortho)
            {
                var ray = raycaster.Raycast(MousePosition, Size);
                var cameraPos = scene.camera.Position;

                if (scene.capturedPoint != null)
                {
                    var dist = (cameraPos - scene.capturedPoint.Position).Length;
                    var pos = cameraPos + ray * dist;

                    if (IsKeyDown(Keys.Z))
                    {
                        pos = new Vector3((int)(2 * pos.X) / 2f, (int)(2 * pos.Y) / 2f, (int)(2 * pos.Z) / 2f);
                    }

                    scene.capturedPoint.parent.MoveVirtualPoint(scene.capturedPoint, pos);
                }
                else if (scene.capturedModel != null)
                {
                    var dist = (cameraPos - scene.capturedModel.Position).Length;
                    var pos = cameraPos + ray * dist;

                    if (IsKeyDown(Keys.Z))
                    {
                        pos = new Vector3((int)(2 * pos.X) / 2f, (int)(2 * pos.Y) / 2f, (int)(2 * pos.Z) / 2f);
                    }

                    scene.capturedModel.Translate(pos);
                    selection.ResetState();
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var scene = Scene.Instance;

            HandleKeyboardInput();
            HandleMouseInput();

            scene.RefreshScene();

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            var scene = Scene.Instance;

            _bgColor = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            _controller = new ImGuiController(this);
           
            scene.cursorRaycastDist = 10f;
            scene.SetBackgroundColor(_bgColor);

            VSync = VSyncMode.On;

            GL.ClearColor(_bgColor);
            GL.PointSize(5);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            var scene = Scene.Instance;

            scene.Unload();
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

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            var scene = Scene.Instance;

            GL.Viewport(0, 0, Size.X, Size.Y);

            scene.camera.ChangeAspect(Size);
        }

        private struct GUIState
        {
            public bool useCursorStartPoint;
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
            internal bool refreshCamera;
        }
    }
}
