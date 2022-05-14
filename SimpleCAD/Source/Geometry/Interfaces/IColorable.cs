using OpenTK.Graphics;

namespace SimpleCAD.Source.Geometry
{
    public interface IColorable
    {
        Color4 GetColor();
        void SetColor(Color4 color);
    }
}
