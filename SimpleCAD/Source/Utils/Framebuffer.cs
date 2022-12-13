using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Environment;
using System.Drawing;

namespace SimpleCAD.Source.Utils
{
    public class Framebuffer
    {
        public readonly int Handle;
        public readonly Rectangle Viewport;

        private int _depth;

        public Framebuffer(int sizeX, int sizeY)
        {
            Handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            Viewport = new Rectangle(0, 0, sizeX, sizeY);

            // Generate depth renderbuffer
            _depth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, sizeX, sizeY);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depth);

            UseDefault();
        }

        public void Use(Texture tex)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, tex.Handle, 0);
            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException("Framebuffer binding failed with code " + status);
            GL.Viewport(Viewport);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        public static void UseDefault()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(Scene.Instance.camera.Viewport);
        }
    }
}
