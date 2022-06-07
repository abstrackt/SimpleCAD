using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Selection
{
    public class SceneRaycaster
    {
        private SceneRaycaster() { }

        private static SceneRaycaster instance = null;
        public static SceneRaycaster Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneRaycaster();
                }
                return instance;
            }
        }

        private const float SELECTION_RADIUS = 0.2f;

        public Vector3 Raycast(Vector2 mouse, Vector2 viewportSize)
        {
            var scene = Scene.Instance;

            var normalisedX = 2 * mouse.X / viewportSize.X - 1;
            var normalisedY = 1 - 2 * mouse.Y / viewportSize.Y;

            var view = scene.camera.viewM;

            var unview = Matrix4.Transpose(Matrix4.Invert(view));
            var unproj = Matrix4.Invert(scene.camera.projectionM);

            var nearPoint = new Vector4(normalisedX, normalisedY, -1, 1);
            var farPoint = new Vector4(normalisedX, normalisedY, 1, 1);
            nearPoint = unproj * nearPoint;
            farPoint = unproj * farPoint;
            nearPoint /= nearPoint.W;
            farPoint /= farPoint.W;
            nearPoint = unview * nearPoint;
            farPoint = unview * farPoint;

            var ray = (farPoint.Xyz - nearPoint.Xyz).Normalized();

            return ray;
        }

        public bool RaycastVirtualSearch(Vector2 mouse, Vector2 viewportSize, out VirtualPoint found)
        {
            var scene = Scene.Instance;

            var ray = Raycast(mouse, viewportSize);

            var cameraPos = scene.camera.Position;

            foreach (var model in scene.complexModels)
            {
                if (SelectionManager.Instance.IsSelected(model)) 
                {
                    foreach (var point in model.VirtualPoints)
                    {
                        var dist = (point.Position - cameraPos).Length;

                        if (Intersect(cameraPos, ray, point.Position, dist))
                        {
                            found = point;
                            return true;
                        }
                    }
                }
            }

            found = null;
            return false;
        }

        public bool RaycastBasicSearch(Vector2 mouse, Vector2 viewportSize, out BasicSceneModel found)
        {
            var scene = Scene.Instance;

            var ray = Raycast(mouse, viewportSize);

            var cameraPos = scene.camera.Position;

            foreach (var model in scene.basicModels)
            {
                var dist = (model.Position - cameraPos).Length;

                if (Intersect(cameraPos, ray, model.Position, dist))
                {
                    found = model;
                    return true;
                }  
            }

            found = null;
            return false;
        }

        private bool Intersect(Vector3 origin, Vector3 direction, Vector3 center, float distance)
        {
            Vector3 L = center - origin;
            float tc = Vector3.Dot(L, direction);

            if (tc < 0.0) return false;

            float d = (float)Math.Sqrt((L.Length * L.Length) - (tc * tc));

            return d < SELECTION_RADIUS;
        }
    }
}
