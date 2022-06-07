using OpenTK.Graphics;
using OpenTK.Mathematics;

namespace SimpleCAD.Source.Geometry
{
    public interface IColorable
    {
        Color4 GetColor();
        void SetColor(Color4 color);
    }
}
