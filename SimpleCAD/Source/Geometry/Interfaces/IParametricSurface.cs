using OpenTK.Mathematics;

namespace SimpleCAD.Source.Geometry
{
    public interface IParametricSurface
    {
        public bool WrapU { get; }
        public bool WrapV { get; }
        public int RangeU { get; }
        public int RangeV { get; }

        public Vector3 Sample(float u, float v);
        public Vector3 DerivU(float u, float v);
        public Vector3 DerivV(float u, float v);
    }
}
