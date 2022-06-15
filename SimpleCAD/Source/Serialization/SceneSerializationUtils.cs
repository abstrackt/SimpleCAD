using SimpleCAD.Source.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO = SharpSceneSerializer.DTOs;
using Geom = SharpSceneSerializer.DTOs.GeometryObjects;
using Enums = SharpSceneSerializer.DTOs.Enums;
using Types = SharpSceneSerializer.DTOs.Types;
using SharpSceneSerializer.DTOs.Interfaces;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;
using OpenTK.Graphics.OpenGL4;

namespace SimpleCAD.Source.Serialization
{
    public class SceneSerializationUtils
    {
        public static DTO.Scene ConvertTo()
        {
            var scene = Scene.Instance;

            List<Geom.Point> points = new List<Geom.Point>();
            List<IGeometryObject> geometry = new List<IGeometryObject>();

            // Serialize all points
            foreach (var element in scene.pointModels)
            {
                element.Serialize(out var data);
                points.Add(data);
            }

            // Serialize all basic models
            foreach (var element in scene.basicModels)
            {
                if (element.TrySerialize(out var data))
                {
                    geometry.Add(data);
                }
            }

            foreach (var element in scene.complexModels)
            {
                if (element.TrySerialize(out var data))
                {
                    geometry.Add(data);
                }
            }

            return new DTO.Scene(points, geometry);
        }

        public static void ConvertFrom(DTO.Scene data)
        {
            var scene = Scene.Instance;

            scene.ResetScene();

            foreach(var point in data.Points)
            {
                DeserializePoint(point, out var model);
                scene.AddModel(model, false);
            }

            foreach (var geom in data.Geometry)
            {
                if (TryDeserializeObject(geom, out var model))
                {
                    if (model is SimpleSceneModel simple)
                        scene.AddModel(simple, false);

                    if (model is CurveSceneModel curve)
                        scene.AddModel(curve);

                    if (model is SurfaceSceneModel surface)
                        scene.AddModel(surface);
                }
            }
        }

        private static void DeserializePoint(Geom.Point data, out PointSceneModel model)
        {
            var pointModel = new PointSceneModel(data.Name);
            pointModel.Translate(data.Position.AsVector3());
            model = pointModel;
            model.id = data.Id;
        }

        private static bool TryDeserializeObject(IGeometryObject serialized, out SceneModel model)
        {
            switch (serialized.ObjectType)
            {
                case Enums.ObjectType.torus:
                    {
                        var data = serialized as Geom.Torus;
                        var torus = new Torus(
                            (int)data.Samples.X, (int)data.Samples.Y,
                            data.LargeRadius, data.SmallRadius,
                            ColorPalette.DeselectedColor);
                        var torusModel = new SimpleSceneModel(torus, data.Name, PrimitiveType.Triangles);
                        torusModel.id = data.Id;
                        torusModel.Translate(data.Position.AsVector3());
                        torusModel.Rotate(data.Position.AsVector3());
                        torusModel.Rescale(data.Scale.AsVector3());
                        model = torusModel;
                        return true;
                    }
                case Enums.ObjectType.bezierC0:
                    {
                        var data = serialized as Geom.BezierC0;
                        var bezier = new C0BezierCurve();
                        var bezierModel = new CurveSceneModel(bezier, data.Name);
                        var points = new List<PointSceneModel>();
                        foreach (var point in data.ControlPoints)
                        {
                            if (Scene.Instance.TryFind(point.Id, out var pointModel))
                            {
                                points.Add(pointModel as PointSceneModel);
                            }
                        }
                        bezierModel.SetPoints(points, true);
                        bezierModel.id = data.Id;
                        model = bezierModel;
                        return true;
                    }
                case Enums.ObjectType.bezierC2:
                    {
                        var data = serialized as Geom.BezierC2;
                        var spline = new C2SplineCurve();
                        var splineModel = new CurveSceneModel(spline, data.Name);
                        var points = new List<PointSceneModel>();
                        foreach (var point in data.DeBoorPoints)
                        {
                            if (Scene.Instance.TryFind(point.Id, out var pointModel))
                            {
                                points.Add(pointModel as PointSceneModel);
                            }
                        }
                        splineModel.SetPoints(points, true);
                        splineModel.id = data.Id;
                        model = splineModel;
                        return true;
                    }
                case Enums.ObjectType.interpolatedC2:
                    {
                        var data = serialized as Geom.InterpolatedC2;
                        var spline = new C2InterpolatingCurve();
                        var splineModel = new CurveSceneModel(spline, data.Name);
                        var points = new List<PointSceneModel>();
                        foreach (var point in data.ControlPoints)
                        {
                            if (Scene.Instance.TryFind(point.Id, out var pointModel))
                            {
                                points.Add(pointModel as PointSceneModel);
                            }
                        }
                        splineModel.SetPoints(points, true);
                        splineModel.id = data.Id;
                        model = splineModel;
                        return true;
                    }
                case Enums.ObjectType.bezierSurfaceC0:
                    {
                        var data = serialized as Geom.BezierSurfaceC0;
                        if (data.Patches.Length < 1)
                            break;
                        int tessU = (int)data.Patches[0].Samples.X;
                        int tessV = (int)data.Patches[0].Samples.Y;
                        var surf = new C0BezierSurface((int)data.Size.X, (int)data.Size.Y, data.ParameterWrapped.U, tessU, tessV);
                        var surfModel = new SurfaceSceneModel(surf, data.Name);
                        var pointList = new List<PointSceneModel>();
                        var points = new PointSceneModel[surf.PointsU, surf.PointsV];
                        int patchU = 0, patchV = 0;
                        foreach (var patch in data.Patches)
                        {
                            var uOffset = patchU * surf.PatchOffset;
                            var vOffset = patchV * surf.PatchOffset;

                            int innerU = 0, innerV = 0;
                            foreach (var point in patch.controlPoints)
                            {
                                if (Scene.Instance.TryFind(point.Id, out var pointModel))
                                {
                                    var u = (uOffset + innerU) % surf.PointsU;
                                    var v = vOffset + innerV;
                                    points[u, v] = pointModel as PointSceneModel;
                                }

                                innerU += 1;
                                if (innerU >= surf.PatchSize)
                                {
                                    innerU = 0;
                                    innerV++;
                                }
                            }

                            patchU += 1;
                            if (patchU >= surf.PatchesU)
                            {
                                patchU = 0;
                                patchV++;
                            }
                        }
                        for (int v = 0; v < points.GetLength(1); v++)
                        {
                            for (int u = 0; u < points.GetLength(0); u++)
                            {
                                pointList.Add(points[u, v]);
                            }
                        }
                        surfModel.SetPoints(pointList, true);
                        surfModel.id = data.Id;
                        surfModel.GeneratePatchData();
                        model = surfModel;
                        return true;
                    }
                case Enums.ObjectType.bezierSurfaceC2:
                    {
                        var data = serialized as Geom.BezierSurfaceC2;
                        if (data.Patches.Length < 1)
                            break;
                        int tessU = (int)data.Patches[0].Samples.X; 
                        int tessV = (int)data.Patches[0].Samples.Y;
                        var surf = new C2SplineSurface((int)data.Size.X, (int)data.Size.Y, data.ParameterWrapped.U, tessU, tessV);
                        var surfModel = new SurfaceSceneModel(surf, data.Name);
                        var pointList = new List<PointSceneModel>();
                        var points = new PointSceneModel[surf.PointsU, surf.PointsV];
                        int patchU = 0, patchV = 0;
                        
                        foreach (var patch in data.Patches)
                        {
                            var uOffset = patchU * surf.PatchOffset;
                            var vOffset = patchV * surf.PatchOffset;

                            int innerU = 0, innerV = 0;
                            foreach (var point in patch.controlPoints)
                            {
                                if (Scene.Instance.TryFind(point.Id, out var pointModel))
                                {
                                    var u = (uOffset + innerU) % surf.PointsU;
                                    var v = vOffset + innerV;
                                    points[u, v] = pointModel as PointSceneModel;
                                }

                                innerU += 1;
                                if (innerU >= surf.PatchSize)
                                {
                                    innerU = 0;
                                    innerV++;
                                }
                            }

                            patchU += 1;
                            if (patchU >= surf.PatchesU)
                            {
                                patchU = 0;
                                patchV++;
                            }
                        }
                        for (int v = 0; v < points.GetLength(1); v++)
                        {
                            for (int u = 0; u < points.GetLength(0); u++)
                            {
                                pointList.Add(points[u, v]);
                            }
                        }
                       
                        surfModel.SetPoints(pointList, true);
                        surfModel.id = data.Id;
                        surfModel.GeneratePatchData();
                        model = surfModel;
                        return true;
                    }
            }

            model = null;
            return false;
        }
    }
}
