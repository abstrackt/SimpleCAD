using OpenTK.Graphics.OpenGL4;

namespace SimpleCAD.Source.Utils
{
    public class Texture
    {
        public readonly int Handle;

        private PixelInternalFormat _internalFormat;
        private PixelFormat _format;
        private PixelType _pixelType;

        public Texture(PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType)
        {
            Handle = GL.GenTexture();
            _internalFormat = internalFormat;
            _format = format;
            _pixelType = pixelType;
        }

        public Texture(int glHandle, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType)
        {
            Handle = glHandle;
            _internalFormat = internalFormat;
            _format = format;
            _pixelType = pixelType;
        }

        public void LoadBytes(byte[] pixels, int sizeX, int sizeY)
        {
            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            // Texture for water calculations
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                _internalFormat,
                sizeX,
                sizeY,
                0,
                _format,
                _pixelType,
                pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public void LoadNull(int sizeX, int sizeY)
        {
            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            // Texture for water calculations
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                _internalFormat,
                sizeX,
                sizeY,
                0,
                _format,
                _pixelType,
                IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
