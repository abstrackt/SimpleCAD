using OpenTK.Graphics.OpenGL4;

namespace SimpleCAD.Source.Utils
{
    public class Texture
    {
        public readonly int Handle;

        public static Texture Load (byte[] pixels, PixelFormat format, int size)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // Texture for water calculations
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba8,
                size,
                size,
                0,
                format,
                PixelType.UnsignedByte,
                pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TextureStorage2D(handle, 0, SizedInternalFormat.Rgba8, size, size);

            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
