using OpenTK.Mathematics;

namespace SimpleCAD.Source.Geometry
{
    public interface IParametricSurface
    {
        public bool WrapU { get; }
        public bool WrapV { get; }
        public float RangeU { get; }
        public float RangeV { get; }

        public Vector3 Sample(float u, float v, float surfaceOffset);
        public Vector3 DerivU(float u, float v, float surfaceOffset);
        public Vector3 DerivV(float u, float v, float surfaceOffset);
    }
}
