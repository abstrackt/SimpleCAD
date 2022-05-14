using OpenTK.Graphics;
using System.Collections.Generic;
using OpenTK;
using System;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;
using OpenTK.Graphics.OpenGL4;

namespace SimpleCAD.Source.Environment
{
    public class Scene
    {
        private Scene() 
        {
            camera = new Camera();
            basicModels = new List<BasicSceneModel>();
            complexModels = new List<ControlPointSceneModel>();

            camera.TranslateCamera(new Vector3(0, 3, 4));
            camera.ChangeViewportConfig(60, 1, 40, false);

            _cursor = new Gizmo(new CursorLines(), PrimitiveType.Lines);
            _selectionMidpoint = new Gizmo(new Point(ColorPalette.MidpointColor), PrimitiveType.Points);
            _gridX = new Grid();
            _gridY = new Grid();
            _gridZ = new Grid();

            cursorPos = Vector3.Zero;
        }

        private static Scene instance = null;
        public static Scene Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scene();
                }
                return instance;
            }
        }

        public Camera camera;

        // This should probably be divided into pointModels and complexGeometryModels in the future

        public List<BasicSceneModel> basicModels;
        public List<ControlPointSceneModel> complexModels;
        public BasicSceneModel capturedModel;
        public VirtualPoint capturedPoint;
        public Vector3 cursorPos;
        public float cursorRaycastDist;

        private Color4 _bgColor;
        private Gizmo _cursor;
        private Gizmo _selectionMidpoint;
        private Grid _gridX, _gridY, _gridZ;

        public void SetBackgroundColor(Color4 bgColor)
        {
            _bgColor = bgColor;
            _gridX.UpdateGrid(Matrix4.CreateRotationX((float)Math.PI / 2), 20f, _bgColor);
            _gridY.UpdateGrid(Matrix4.CreateRotationY((float)Math.PI / 2), 20f, _bgColor);
            _gridZ.UpdateGrid(Matrix4.CreateRotationZ((float)Math.PI / 2), 20f, _bgColor);
        }
        
        // Recalculate meshes if needed, recalculate selection pivot, etc.
        public void RefreshScene()
        {
            for (int i = 0; i < basicModels.Count; i++)
            {
                basicModels[i].Refresh();
            }

            for (int i = 0; i < complexModels.Count; i++)
            {
                complexModels[i].Refresh();
            }
        }

        public void AddModel(BasicSceneModel model)
        {
            model.Translate(cursorPos);
            basicModels.Add(model);
        }

        public void AddModel(ControlPointSceneModel model)
        {
            complexModels.Add(model);
        }

        public void RemoveModel(BasicSceneModel model)
        {
            if (basicModels.Contains(model))
            {
                foreach (var complexModel in complexModels)
                {
                    complexModel.RemovePoint(model); 
                }
                SelectionManager.Instance.Remove(model);
                basicModels.Remove(model);
            }
        }

        public void RemoveModel(ControlPointSceneModel model)
        {
            if (complexModels.Contains(model))
            {
                SelectionManager.Instance.Remove(model);
                complexModels.Remove(model);
            }
        }

        public void RenderScene(bool gridX, bool gridY, bool gridZ)
        {
            var elementList = new List<RenderableElement>();
            var instanceList = new List<(Gizmo, Vector3)>();

            if (gridX)
                elementList.Add(_gridX);
            if (gridY)
                elementList.Add(_gridY);
            if (gridZ)
                elementList.Add(_gridZ);

            for (int i = 0; i < basicModels.Count; i++)
            {
                elementList.Add(basicModels[i]);
            }

            for (int i = 0; i < complexModels.Count; i++)
            {
                elementList.Add(complexModels[i]);
            }

            if (SelectionManager.Instance.Count > 1)
            {
                instanceList.Add((_selectionMidpoint, SelectionManager.Instance.Midpoint));
            }

            instanceList.Add((_cursor, cursorPos));

            camera.Render(elementList, instanceList);
        }

        public void Unload()
        {
            for (int i = 0; i < basicModels.Count; i++)
            {
                basicModels[i].Dispose();
            }

            for (int i = 0; i < complexModels.Count; i++)
            {
                complexModels[i].Dispose();
            }
        }
    }
}
