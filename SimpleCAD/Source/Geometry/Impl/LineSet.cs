﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using SharpSceneSerializer.DTOs.Enums;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class LineSet : IGeometry
    {
        public string VertexShader => "shader.vert";
        public string FragShader => "shader.frag";
        public string TescShader => "";
        public string TeseShader => "";

        private List<Vertex> _vertexCache;
        private List<uint> _indexCache;

        public LineSet()
        {
            _vertexCache = new List<Vertex>();
            _indexCache = new List<uint>();
        }

        public void ClearLines()
        {
            _vertexCache.Clear();
            _indexCache.Clear();
        }

        public void AddLine(Line line)
        {
            // process line into the cache;
            uint currentVertex = (uint)_vertexCache.Count;

            for (int i = 0; i < line.points.Count; i++)
            {
                _vertexCache.Add(new Vertex(line.points[i],line.color));

                if (i != 0)
                {
                    _indexCache.Add(currentVertex - 1);
                    _indexCache.Add(currentVertex);
                }

                currentVertex++;
            }
        }

        // Only changed externally
        public bool GeometryChanged() => false;

        public (Vertex[] vertices, uint[] indices) GetMesh()
        {
            return (_vertexCache.ToArray(), _indexCache.ToArray());
        }

        public void OnTransformChanged(Matrix4 transform)
        {
            // Not required
        }
    }
}
