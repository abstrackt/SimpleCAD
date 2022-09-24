using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public abstract class Surface : IControlPointGeometry, IColorable
    {
        protected Color4 color;
        protected Vector3[,] controlPoints;
        protected List<Vertex> vertexCache;
        protected int patchesU, patchesV;
        protected readonly bool wrapU;

        public int PatchesU => patchesU;
        public int PatchesV => patchesV;

        public int PointsU => PatchSize + (PatchOffset) * (PatchesU - 1) - (Wrap ? 1 : 0);
        public int PointsV => PatchSize + (PatchOffset) * (PatchesV - 1);

        public abstract string VertexShader { get; }
        public abstract string FragShader { get; }
        public abstract string TescShader { get; }
        public abstract string TeseShader { get; }

        public abstract int PatchSize { get; }
        public abstract int PatchOffset { get; }

        public abstract (int u, int v) TesselationLevel { get; }
        public bool Wrap => wrapU;

        public bool GeometryChanged() => true;

        public Surface(int patchesU, int patchesV, bool wrapU = false)
        {
            if (patchesU < 1 || patchesV < 1)
            {
                throw new InvalidOperationException("Cannot create surface of width or height 0");
            }

            this.wrapU = wrapU;
            this.patchesU = patchesU;
            this.patchesV = patchesV;
            controlPoints = new Vector3[PointsU, PointsV];
            vertexCache = new List<Vertex>();
        }

        public abstract List<Vector3> GenerateControlPoints(float mU, float mV);

        protected Vector3[,] GetPatchPoints(int uPatch, int vPatch)
        {
            if (Wrap)
                uPatch = (uPatch + PatchesU) % PatchesU;

            if (uPatch >= PatchesU || vPatch >= PatchesV || uPatch < 0 || vPatch < 0)
                throw new InvalidOperationException("Trying to obtain patch outside of permitted range");

            var points = new Vector3[PatchSize, PatchSize];

            int startU = uPatch * PatchOffset;
            int startV = vPatch * PatchOffset;

            for (int u = 0; u < PatchSize; u++)
            {
                for (int v = 0; v < PatchSize; v++)
                {
                    points[u, v] = controlPoints[startU + u, startV + v];
                }
            }

            return points;
        }

        public void SetControlPoints(List<Vector3> positions)
        {
            if (positions.Count != controlPoints.Length)
                return;

            for (int u = 0; u < PointsU; u++)
            {
                for (int v = 0; v < PointsV; v++)
                {
                    controlPoints[u, v] = positions[u + v * PointsU]; 
                }
            }
        }

        public virtual List<Line> GetLines() { return new List<Line>(); }

        protected virtual void BeforeMeshRender() { }

        // For tesselation shader
        private void GeneratePatches(Vector3[,] points)
        {
            vertexCache.Clear();

            int currentU = 0, currentV = 0;

            while(currentU + PatchSize <= (PointsU + (Wrap ? (PatchSize - PatchOffset) : 0)))
            {
                while (currentV + PatchSize <= PointsV)
                {
                    for (int u = currentU; u < currentU + PatchSize; u++)
                    {
                        for (int v = currentV; v < currentV + PatchSize; v++)
                        {
                            var uWrap = u % PointsU;
                            var vWrap = v % PointsV;
                            vertexCache.Add(new Vertex(points[uWrap, vWrap], color));
                        }
                    }

                    currentV += PatchOffset;
                }

                currentV = 0;
                currentU += PatchOffset;
            }
        }

        public virtual (Vertex[] vertices, uint[] indices) GetMesh()
        {
            BeforeMeshRender();

            GeneratePatches(controlPoints);

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
            return new List<Vector3>();
        }
    }
}
