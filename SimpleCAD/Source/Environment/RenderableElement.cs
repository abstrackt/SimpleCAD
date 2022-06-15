using System;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Environment
{
    // Wrapper for rendering-related code
    public abstract class RenderableElement
    {
        public PrimitiveType type;
        public Shader shader;

        private int _buffer;
        private bool _initialized;

        protected int _vertexArray;
        protected int _elementBuffer;

        protected int _vertexCount;
        protected int _indexCount;

        private string _vertPath, _fragPath, _tessControlPath, _tessEvalPath;

        protected IGeometry Geometry { get; private set; }

        public RenderableElement(IGeometry geometry) 
        {
            Geometry = geometry;

            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();
            _elementBuffer = GL.GenBuffer();

            RegenMesh();

            _vertPath = "shader.vert";
            _fragPath = "shader.frag";
            _tessControlPath = "";
            _tessEvalPath = "";

            ReloadShader();

            _initialized = true;
        }

        public void SetVertShader(string vertShaderPath)
        {
            _vertPath = vertShaderPath;
            ReloadShader();
        }

        public void SetFragShader(string fragShaderPath)
        {
            _fragPath = fragShaderPath;
            ReloadShader();
        }

        public void SetTesselationShader(string tessControlShaderPath, string tessEvalShaderPath)
        {
            _tessControlPath = tessControlShaderPath;
            _tessEvalPath = tessEvalShaderPath;
            ReloadShader();
        }

        // Set shader constants, can be overriden if need be.
        public virtual void Render()
        {
            BeforeRendering();
            shader.Use();
            GL.BindVertexArray(_vertexArray);
            GL.DrawElements(type, _indexCount, DrawElementsType.UnsignedInt, (IntPtr)0);
            AfterRendering();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ReloadShader()
        {
            shader = new Shader(_vertPath, _fragPath, _tessControlPath, _tessEvalPath);
        }

        protected virtual void RegenMesh()
        {
            (var verts, var indices) = Geometry.GetMesh();

            _vertexCount = verts.Length;
            _indexCount = indices.Length;

            UpdateBuffers(verts, indices);
        }

        protected void UpdateBuffers(Vertex[] verts, uint[] indices)
        {
            // VAO
            GL.BindVertexArray(_vertexArray);

            // VBO for geometry
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * Vertex.Size, verts, BufferUsageHint.DynamicDraw);

            // Element Buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, Vertex.Size, 0); // position
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Vertex.Size, 16); // color

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
        }

        protected virtual void BeforeRendering() { }

        protected virtual void AfterRendering() { }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_initialized)
                {
                    shader.Dispose();
                    GL.DeleteVertexArray(_vertexArray);
                    GL.DeleteBuffer(_buffer);
                    _initialized = false;
                }
            }
        }
    }
}
