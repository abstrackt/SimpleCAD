using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Geometry.Interfaces
{
    public interface IParametricSurface
    {
        public bool WrapU { get; }
        public bool WrapV { get; }
        public (int min, int max) RangeU { get; }
        public (int min, int max) RangeV { get; }

        public Vector3 Sample(float u, float v);
        public Vector3 DerivU(float u, float v);
        public Vector3 DerivV(float u, float v);
    }
}
