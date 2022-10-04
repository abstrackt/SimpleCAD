using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleCAD.Source.Utils
{
    public class Shader : IDisposable
    {
        private readonly int handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertexPath, string fragmentPath, string tessControlPath = "", string tessEvalPath = "")
        {
            string vertexShaderSource;
            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                vertexShaderSource = reader.ReadToEnd();
            }

            string fragmentShaderSource;
            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                fragmentShaderSource = reader.ReadToEnd();
            }

            int tessControlShader = -1;
            int tessEvalShader = -1;

            if (tessControlPath != "" && tessEvalPath != "")
            {
                string tessControlShaderSource;
                using (StreamReader reader = new StreamReader(tessControlPath, Encoding.UTF8))
                {
                    tessControlShaderSource = reader.ReadToEnd();
                }

                string tessEvalShaderSource;
                using (StreamReader reader = new StreamReader(tessEvalPath, Encoding.UTF8))
                {
                    tessEvalShaderSource = reader.ReadToEnd();
                }

                tessControlShader = GL.CreateShader(ShaderType.TessControlShader);
                GL.ShaderSource(tessControlShader, tessControlShaderSource);

                tessEvalShader = GL.CreateShader(ShaderType.TessEvaluationShader);
                GL.ShaderSource(tessEvalShader, tessEvalShaderSource);
            }

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            GL.CompileShader(vertexShader);

            string infoLogVert = GL.GetShaderInfoLog(vertexShader);
            if (infoLogVert != System.String.Empty)
                System.Console.WriteLine(infoLogVert);

            GL.CompileShader(fragmentShader);

            string infoLogFrag = GL.GetShaderInfoLog(fragmentShader);

            if (infoLogFrag != System.String.Empty)
                System.Console.WriteLine(infoLogFrag);

            bool hasTesselation = tessControlShader != -1 && tessEvalShader != -1;

            if (hasTesselation)
            {
                GL.CompileShader(tessControlShader);

                string infoLogTessControl = GL.GetShaderInfoLog(tessControlShader);

                if (infoLogTessControl != System.String.Empty)
                    System.Console.WriteLine(infoLogTessControl);

                GL.CompileShader(tessEvalShader);

                string infoLogTessEval = GL.GetShaderInfoLog(tessEvalShader);

                if (infoLogTessEval != System.String.Empty)
                    System.Console.WriteLine(infoLogTessEval);
            }

            handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            if (hasTesselation)
            {
                GL.AttachShader(handle, tessControlShader);
                GL.AttachShader(handle, tessEvalShader);
            }

            GL.LinkProgram(handle);

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            if (hasTesselation)
            {
                GL.DetachShader(handle, tessControlShader);
                GL.DetachShader(handle, tessEvalShader);
            }
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            if (hasTesselation)
            {
                GL.DeleteShader(tessControlShader);
                GL.DeleteShader(tessEvalShader);
            }

            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(handle, i, out _, out _);
                var location = GL.GetUniformLocation(handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        public void SetInt(string name, int data)
        {
            if (!_uniformLocations.ContainsKey(name))
                return;
            GL.UseProgram(handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            if (!_uniformLocations.ContainsKey(name))
                return;
            GL.UseProgram(handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            if (!_uniformLocations.ContainsKey(name))
                return;
            GL.UseProgram(handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            if (!_uniformLocations.ContainsKey(name))
                return;
            GL.UseProgram(handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        public void SetSampler(string name, int unit)
        {
            if (!_uniformLocations.ContainsKey(name))
                return;
            GL.UseProgram(handle);
            GL.Uniform1(_uniformLocations[name], unit);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(handle);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
