﻿using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry.Impl;
using SimpleCAD.Source.Intersections;

namespace SimpleCAD.Source.Environment
{
    public class Scene
    {
        private Scene() 
        {
            camera = new Camera();
            pointModels = new List<PointSceneModel>();
            basicModels = new List<SimpleSceneModel>();
            complexModels = new List<ComplexSceneModel>();
            intersections = new List<IntersectionSceneModel>();

            camera.TranslateCamera(new Vector3(0, 3, 4));
            camera.ChangeViewportConfig(60, 1, 40, false);

            _cursor = new Gizmo(new CursorLines(), PrimitiveType.Lines);
            _selectionMidpoint = new Gizmo(new Point(ColorPalette.MidpointColor), PrimitiveType.Points);
            _gridX = new Grid();
            _gridY = new Grid();
            _gridZ = new Grid();

            _id = 0;
            _modelDict = new Dictionary<uint, SceneModel>(); 

            cursorPos = Vector3.Zero;
        }

        public bool IsPartOfIntersection(SimpleSceneModel simpleSceneModel)
        {
            foreach (var intersect in intersections)
            {
                if (intersect.M1 == simpleSceneModel || intersect.M2 == simpleSceneModel)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsPartOfIntersection(ComplexSceneModel complexSceneModel)
        {
            foreach (var intersect in intersections)
            {
                if (intersect.M1 == complexSceneModel || intersect.M2 == complexSceneModel)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsPartOfIntersection(PointSceneModel simpleSceneModel)
        {
            foreach (var complex in complexModels)
            {
                if (complex.ControlPoints.Contains(simpleSceneModel) && IsPartOfIntersection(complex))
                {
                    return true;
                }
            }

            return false;
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

        public List<PointSceneModel> pointModels;
        public List<SimpleSceneModel> basicModels;
        public List<ComplexSceneModel> complexModels;
        public List<IntersectionSceneModel> intersections;
        public SceneModel capturedModel;
        public VirtualPoint capturedPoint;
        public Vector3 cursorPos;
        public float cursorRaycastDist;

        private Color4 _bgColor;
        private Gizmo _cursor;
        private Gizmo _selectionMidpoint;
        private Grid _gridX, _gridY, _gridZ;
        private uint _id;
        private Dictionary<uint, SceneModel> _modelDict;

        public uint NextId()
        {
            return _id++;
        }

        public void ForceSetId(uint value)
        {
            _id = value;
        }

        public bool TryFind(uint id, out SceneModel model)
        {
            return _modelDict.TryGetValue(id, out model);
        }

        public void SetBackgroundColor(Color4 bgColor)
        {
            _bgColor = bgColor;
            _gridX.UpdateGrid(Matrix4.CreateRotationX((float)Math.PI / 2), 20f, _bgColor);
            _gridY.UpdateGrid(Matrix4.CreateRotationY((float)Math.PI / 2), 20f, _bgColor);
            _gridZ.UpdateGrid(Matrix4.CreateRotationZ((float)Math.PI / 2), 20f, _bgColor);
        }

        public void ResetScene()
        {
            pointModels.Clear();
            basicModels.Clear();
            complexModels.Clear();
            intersections.Clear();
            SelectionManager.Instance.Clear();
            _id = 0;
            _modelDict.Clear();
        }
        
        // Recalculate meshes if needed, recalculate selection pivot, etc.
        public void RefreshScene()
        {
            for (int i = 0; i < pointModels.Count; i++)
            {
                pointModels[i].Refresh();
            }

            for (int i = 0; i < basicModels.Count; i++)
            {
                basicModels[i].Refresh();
            }

            for (int i = 0; i < complexModels.Count; i++)
            {
                complexModels[i].Refresh();
            }

            for (int i = 0; i < intersections.Count; i++)
            {
                intersections[i].Refresh();
            }
        }

        public void MergePoints(PointSceneModel p1, PointSceneModel p2)
        {
            var pos = (p1.Position + p2.Position) / 2;

            foreach (var model in complexModels)
            {
                model.ReplacePoint(p1, p2);
            }

            RemoveModel(p1, true);

            p2.Translate(pos, false);
        }

        public void SetupGregoryPatch(List<SurfaceSceneModel> surfs)
        {
            var patches = new List<SurfacePatch>();

            // Extract all patches for the selected surfaces
            foreach (var surf in surfs)
            {
                patches.AddRange(surf.Patches);
            }

            if (patches.Count < 3)
            {
                return;
            }

            foreach (var c in patches.Combinations(3))
            {
                var cl = c.ToList();

                var s1 = cl[0];
                var s2 = cl[1];
                var s3 = cl[2];

                // Find valid holes
                if (s1.TryGetCorners(out var c1) && s2.TryGetCorners(out var c2) && s3.TryGetCorners(out var c3))
                {
                    // Common corners between s1 and s2
                    var c1c2s = c1.Where(x => c2.Contains(x)).ToList();

                    // Common corners between s2 and s3
                    var c2c3s = c2.Where(x => c3.Contains(x)).ToList();

                    // Common corners between s2 and s3
                    var c3c1s = c3.Where(x => c1.Contains(x)).ToList();

                    // Check if they only contain one common corner each
                    for (int i1 = 0; i1 < c1c2s.Count; i1++)
                    {
                        for (int i2 = 0; i2 < c2c3s.Count; i2++)
                        {
                            for (int i3 = 0; i3 < c3c1s.Count; i3++)
                            {
                                // Check if the points always lie on the same edge within a bezier patch

                                var c1c2 = c1c2s[i1];
                                var c2c3 = c2c3s[i2];
                                var c3c1 = c3c1s[i3];

                                var i11 = c1.IndexOf(c3c1);
                                var i12 = c1.IndexOf(c1c2);
                                var i21 = c2.IndexOf(c1c2);
                                var i22 = c2.IndexOf(c2c3);
                                var i31 = c3.IndexOf(c2c3);
                                var i32 = c3.IndexOf(c3c1);

                                if ((((i11 + 1) % c1.Count) == i12 || ((i12 + 1) % c1.Count) == i11) &&
                                    (((i21 + 1) % c2.Count) == i22 || ((i22 + 1) % c2.Count) == i21) &&
                                    (((i31 + 1) % c3.Count) == i32 || ((i32 + 1) % c3.Count) == i31))
                                {
                                    // Get points on borders of hole
                                    var s1c = s1.points;
                                    var s2c = s2.points;
                                    var s3c = s3.points;

                                    var i1s1 = s1c.IndexOf(c3c1);
                                    var i2s1 = s1c.IndexOf(c1c2);
                                    var i1s2 = s2c.IndexOf(c1c2);
                                    var i2s2 = s2c.IndexOf(c2c3);
                                    var i1s3 = s3c.IndexOf(c2c3);
                                    var i2s3 = s3c.IndexOf(c3c1);
                                    if (s1.TryGetValuesBetween(i1s1, i2s1, out var p1) &&
                                        s2.TryGetValuesBetween(i1s2, i2s2, out var p2) &&
                                        s3.TryGetValuesBetween(i1s3, i2s3, out var p3) &&
                                        s1.TryGetDerivativesBetween(i1s1, i2s1, out var d1) &&
                                        s2.TryGetDerivativesBetween(i1s2, i2s2, out var d2) &&
                                        s3.TryGetDerivativesBetween(i1s3, i2s3, out var d3))
                                    {
                                        // Find correct edge ordering (better algorithm surely exists but its just 8 cases)
                                        
                                        for (int o1 = 0; o1 <= 1; o1++)
                                        {
                                            for (int o2 = 0; o2 <= 1; o2++)
                                            {
                                                for (int o3 = 0; o3 <= 1; o3++)
                                                {
                                                    var pt1 = new List<PointSceneModel>(p1);
                                                    var dt1 = new List<PointSceneModel>(d1);
                                                    var pt2 = new List<PointSceneModel>(p2);
                                                    var dt2 = new List<PointSceneModel>(d2);
                                                    var pt3 = new List<PointSceneModel>(p3);
                                                    var dt3 = new List<PointSceneModel>(d3);
                                                          
                                                    if (o1 > 0)
                                                    {
                                                        pt1.Reverse();
                                                        dt1.Reverse();
                                                    }

                                                    if (o2 > 0)
                                                    {
                                                        pt2.Reverse();
                                                        dt2.Reverse();
                                                    }

                                                    if (o3 > 0)
                                                    {
                                                        pt3.Reverse();
                                                        dt3.Reverse();
                                                    }

                                                    if (pt1[pt1.Count - 1] == pt2[0] &&
                                                        pt2[pt2.Count - 1] == pt3[0] &&
                                                        pt3[pt3.Count - 1] == pt1[0])
                                                    {
                                                        p1 = pt1;
                                                        d1 = dt1;
                                                        p2 = pt2;
                                                        d2 = dt2;
                                                        p3 = pt3;
                                                        d3 = dt3;
                                                    }
                                                }
                                            }
                                        }

                                        var model = new GregoryPatchSceneModel(new GregoryPatch(), "Gregory patch");
                                        var points = new List<PointSceneModel>();
                                        points.AddRange(p1);
                                        points.AddRange(p2);
                                        points.AddRange(p3);
                                        points.AddRange(d1);
                                        points.AddRange(d2);
                                        points.AddRange(d3);
                                        model.SetPoints(points, true);
                                        AddModel(model);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetupIntersection(SceneModel m1,SceneModel m2, bool useCursor)
        {
            var intersections = IntersectionManager.Instance;

            IntersectionData result;
            bool found;

            if (useCursor) {
                found = intersections.TryFindIntersection(m1.ParametricGeometry, m2.ParametricGeometry, cursorPos, out result);
            }
            else
            {
                found = intersections.TryFindIntersection(m1.ParametricGeometry, m2.ParametricGeometry, out result);
            }

            if (found)
            {
                var model = new IntersectionSceneModel(new C2InterpolatingCurve(), "Intersection", result, m1, m2);

                var textures = intersections.GetIntersectTexture(m1.ParametricGeometry, m2.ParametricGeometry, result.parameters);

                m1.SetIntersectTexture(textures.t1, IntersectionManager.DEFAULT_TEXTURE_RES);
                m2.SetIntersectTexture(textures.t2, IntersectionManager.DEFAULT_TEXTURE_RES);

                m1.ToggleTrimming(false);
                m2.ToggleTrimming(false);

                m1.SetTrimTarget(255);
                m2.SetTrimTarget(255);

                AddModel(model);
            }
        }

        public void AddModel(GregoryPatchSceneModel model)
        {
            complexModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        public void AddModel(PointSceneModel model, bool translate = true)
        {
            if (translate)
                model.Translate(cursorPos);
            pointModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        public void AddModel(SimpleSceneModel model, bool translate = true)
        {
            if (translate)
                model.Translate(cursorPos);
            basicModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        public void AddModel(CurveSceneModel model, bool setPoints = true)
        {
            if (setPoints)
                model.SetPoints(SelectionManager.Instance.SelectedPoints);
            complexModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        public void AddModel(IntersectionSceneModel model)
        {
            intersections.Add(model);
            _modelDict.Add(model.id, model);
        }

        // Custom behavior since we need to add points along with the model.
        public void AddModel(SurfaceSceneModel model, float dimU, float dimV)
        {
            List<PointSceneModel> points = new List<PointSceneModel>();

            var generated = model.GenerateControlPoints(dimU, dimV);

            foreach (var pos in generated)
            {
                var point = new PointSceneModel(model.Name + " Point",  false);

                point.Translate(cursorPos);
                point.Translate(pos, true);

                pointModels.Add(point);
                points.Add(point);
                _modelDict.Add(point.id, point);
            }
            
            model.SetPoints(points, true);
            model.GeneratePatchData();
            complexModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        // For serialization
        public void AddModel(SurfaceSceneModel model)
        {
            complexModels.Add(model);
            _modelDict.Add(model.id, model);
        }

        public void RemoveModel(PointSceneModel model, bool force = false)
        {
            if ((model.Deletable || force) && pointModels.Contains(model))
            {
                foreach (var complexModel in complexModels)
                {
                    if (complexModel.ControlPoints.Contains(model))
                        return;
                }
                SelectionManager.Instance.Remove(model);
                pointModels.Remove(model);
                _modelDict.Remove(model.id);
            }
        }

        // For surfaces
        private void RemoveModel(PointSceneModel model, ComplexSceneModel parent)
        {
            if (pointModels.Contains(model))
            {
                foreach (var complexModel in complexModels)
                {
                    if (complexModel.ControlPoints.Contains(model) && complexModel != parent)
                        return;
                }
                SelectionManager.Instance.Remove(model);
                pointModels.Remove(model);
                _modelDict.Remove(model.id);
            }
        }

        public void RemoveModel(SimpleSceneModel model)
        {
            if (basicModels.Contains(model))
            {
                SelectionManager.Instance.Remove(model);
                basicModels.Remove(model);
                _modelDict.Remove(model.id);
            }
        }

        public void RemoveModel(ComplexSceneModel model)
        {
            if (complexModels.Contains(model))
            {
                if (model.RemovePoints)
                {
                    foreach (var point in model.ControlPoints)
                    {
                        RemoveModel(point, model);
                        _modelDict.Remove(point.id);
                    }
                }

                SelectionManager.Instance.Remove(model);
                complexModels.Remove(model);
                _modelDict.Remove(model.id);
            }
        }

        public void RemoveModel(IntersectionSceneModel model)
        {
            if (intersections.Contains(model))
            {
                model.M1.ClearTexture();
                model.M2.ClearTexture();
                model.M1.ToggleTrimming(false);
                model.M2.ToggleTrimming(false);
                SelectionManager.Instance.Remove(model);
                intersections.Remove(model);
                _modelDict.Remove(model.id);
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

            for (int i = 0; i < pointModels.Count; i++)
            {
                elementList.Add(pointModels[i]);
            }

            for (int i = 0; i < basicModels.Count; i++)
            {
                elementList.Add(basicModels[i]);
            }

            for (int i = 0; i < complexModels.Count; i++)
            {
                elementList.Add(complexModels[i]);
            }

            for (int i = 0; i < intersections.Count; i++)
            {
                elementList.Add(intersections[i]);
            }

            if (SelectionManager.Instance.SimpleCount > 1)
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

            for (int i = 0; i < intersections.Count; i++)
            {
                intersections[i].Dispose();
            }
        }
    }
}
