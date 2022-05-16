using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public abstract class Surface : IControlPointGeometry, IColorable
    {
        protected Color4 color;
        protected Vector3[,] controlPoints;
        protected List<Vertex> vertexCache;
        protected int patchesU, patchesV;

        public int PatchesU => patchesU;
        public int PatchesV => patchesV;

        public int PointsU => PatchSize + (PatchOffset) * (PatchesU - 1);
        public int PointsV => PatchSize + (PatchOffset) * (PatchesV - 1);

        public abstract int PatchSize { get; }
        public abstract int PatchOffset { get; }

        public abstract (int u, int v) TesselationLevel { get; }

        public bool GeometryChanged() => true;

        public Surface(int patchesU, int patchesV)
        {
            if (patchesU < 1 || patchesV < 1)
            {
                throw new InvalidOperationException("Cannot create surface of width or height 0");
            }

            this.patchesU = patchesU;
            this.patchesV = patchesV;
            controlPoints = new Vector3[PointsU, PointsV];
            vertexCache = new List<Vertex>();
        }

        public void SetControlPoints(List<Vector3> positions)
        {
            if (positions.Count != controlPoints.Length)
                return;

            for (int x = 0; x < PointsU; x++)
            {
                for (int y = 0; y < PointsV; y++)
                {
                    controlPoints[x, y] = positions[y * PointsU + x]; 
                }
            }
        }

        public virtual List<Line> GetLines() { return new List<Line>(); }

        protected virtual void BeforeMeshRender() { }

        protected virtual Vector3[,] ProcessControlPoints(Vector3[,] points)
        {
            return points;
        }

        // For tesselation shader
        private void GeneratePatches(Vector3[,] points)
        {
            vertexCache.Clear();

            int currentX = 0, currentY = 0;

            while(currentX + PatchSize <= points.GetLength(0))
            {
                while (currentY + PatchSize <= points.GetLength(1))
                {
                    for (int x = currentX; x < currentX + PatchSize; x++)
                    {
                        for (int y = currentY; y < currentY + PatchSize; y++)
                        {
                            vertexCache.Add(new Vertex(points[x, y], color));
                        }
                    }

                    currentY += PatchOffset;
                }

                currentX += PatchOffset;
                currentY = 0;
            }
        }

        public virtual (Vertex[] vertices, uint[] indices) GetMesh()
        {
            BeforeMeshRender();

            // Generate bezier points
            var points = ProcessControlPoints(controlPoints);

            GeneratePatches(points);

            return (vertexCache.ToArray(), new uint[] { });
        }

        public Color4 GetColor()
        {
            return color;
        }

        public void SetColor(Color4 color)
        {
            if (color != this.color)
            {
                this.color = color;
            }
        }

        public List<Vector3> GetVirtualPoints()
        {
            return new List<Vector3>();
        }

        public List<Vector3> MoveVirtualPoint(int i, Vector3 position)
        {
            var list = new List<Vector3>();

            for (int x = 0; x < controlPoints.GetLength(0); x++)
            {
                for (int y = 0; y < controlPoints.GetLength(1); y++)
                {
                    list.Add(controlPoints[x, y]);
                }
            }

            return list;
        }
    }
}
