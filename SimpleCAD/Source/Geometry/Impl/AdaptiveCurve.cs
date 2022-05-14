using OpenTK;
using OpenTK.Graphics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCAD.Source.Geometry
{
    public abstract class AdaptiveCurve : IControlPointGeometry, IColorable
    {
        protected Color4 color;
        protected List<Vector3> controlPoints;
        protected Matrix4 proj, view;
        protected Vector2 viewport;

        protected List<Vertex> vertexCache;
        protected List<uint> indexCache;

        public abstract int SegmentSize { get; }
        public abstract int SegmentOffset { get; }

        public bool GeometryChanged() => true;

        public AdaptiveCurve()
        {
            controlPoints = new List<Vector3>();
            vertexCache = new List<Vertex>();
            indexCache = new List<uint>();
        }

        public void SetControlPoints(List<Vector3> positions)
        {
            controlPoints = new List<Vector3>(positions);
        }

        public virtual List<Line> GetPolygons() { return new List<Line>(); }

        protected virtual void BeforeMeshRender() { }

        protected virtual List<Vector3> ProcessControlPoints(List<Vector3> points) 
        { 
            return points; 
        }
        
        protected virtual List<Vector3> ProcessSegment(List<Vector3> segment)
        {
            if (segment.Count < 2)
            {
                return segment;
            }
            
            if (segment.Count < 3)
            {
                segment = LineToQuad(segment);
            }

            if (segment.Count < 4)
            {
                segment = QuadToCubic(segment);
            }

            return segment;
        }

        private List<Vector3> LineToQuad(List<Vector3> points)
        {
            if (points.Count != 2)
                throw new InvalidOperationException("Not a line");

            var newPoints = new List<Vector3>();

            newPoints.Add(points[0]);
            newPoints.Add((points[0] + points[1]) / 2);
            newPoints.Add(points[1]);

            return newPoints;
        }

        private List<Vector3> QuadToCubic(List<Vector3> points)
        {
            if (points.Count != 3)
                throw new InvalidOperationException("Not a quad bezier");

            var newPoints = new List<Vector3>();

            newPoints.Add(points[0]);
            newPoints.Add(points[0] + 2 / 3f * (points[1] - points[0]));
            newPoints.Add(points[2] + 2 / 3f * (points[1] - points[2]));
            newPoints.Add(points[2]);

            return newPoints;
        }

        // For tesselation shader
        private void GenerateSegments(List<Vector3> points)
        {
            vertexCache.Clear();
            indexCache.Clear();

            int currentPoint = 0;

            while (currentPoint < points.Count) 
            {
                int i;

                var currentSegment = new List<Vector3>(SegmentSize);

                for (i = 0; i < SegmentSize; i++)
                {
                    if (currentPoint + i < points.Count)
                    {
                        currentSegment.Add(points[currentPoint + i]);
                    }
                }

                currentSegment = ProcessSegment(currentSegment);

                for (i = 0; i < currentSegment.Count; i++)
                {
                    vertexCache.Add(new Vertex(currentSegment[i], color));
                }

                currentPoint += SegmentOffset;
            }
        }

        public virtual (Vertex[] vertices, uint[] indices) GetMesh()
        {
            BeforeMeshRender();
                
            // Generate bezier points
            var points = ProcessControlPoints(controlPoints);

            GenerateSegments(points);

            return (vertexCache.ToArray(), indexCache.ToArray());
        }

        public virtual List<Vector3> GetVirtualPoints()
        {
            return new List<Vector3>();
        }

        // Return updated control points in reaction to a virtual point being moved.
        public virtual List<Vector3> MoveVirtualPoint(int i, Vector3 position) 
        {
            return new List<Vector3>(controlPoints);
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
    }
}
