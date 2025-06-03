using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ShaderSaver
{
    public class ShaderRenderer : IDisposable
    {
        private int shaderProgram;
        private int vertexShader;
        private int fragmentShader;
        private int VAO, VBO, EBO;

        // Uniform locations
        private int iResolutionLocation;
        private int iTimeLocation;
        private int iTimeDeltaLocation;
        private int iFrameLocation;
        private int iFrameRateLocation;
        private int iMouseLocation;
        private int iDateLocation;
        private int iSampleRateLocation;

        private int frameCount = 0;
        private float lastTime = 0.0f;

        // Fullscreen quad vertices
        private readonly float[] vertices = {
            // positions        // texture coords
            -1.0f, -1.0f, 0.0f,  0.0f, 0.0f,
             1.0f, -1.0f, 0.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,  1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f,  0.0f, 1.0f
        };

        private readonly int[] indices = {
            0, 1, 2,
            2, 3, 0
        };

        public void Initialize()
        {
            SetupQuad();
            LoadDefaultShader();
        }

        private void SetupQuad()
        {
            // Generate and bind VAO
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // Generate and bind VBO
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Generate and bind EBO
            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Texture coordinate attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        public void LoadShader(string filePath)
        {
            try
            {
                string fragmentShaderSource = ShaderLoader.LoadShaderFromFile(filePath);
                CompileShaders(fragmentShaderSource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shader: {ex.Message}");
                LoadDefaultShader();
            }
        }

        private void LoadDefaultShader()
        {
            string fragmentShaderSource = ShaderLoader.GetDefaultShader();
            CompileShaders(fragmentShaderSource);
        }

        private void CompileShaders(string fragmentShaderSource)
        {
            // Clean up previous shaders
            if (shaderProgram != 0)
            {
                GL.DeleteProgram(shaderProgram);
            }

            // Vertex shader source
            string vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

void main()
{
    gl_Position = vec4(aPos, 1.0);
}";

            // Compile vertex shader
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            // Check vertex shader compilation
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexSuccess);
            if (vertexSuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                throw new Exception($"Vertex shader compilation failed: {infoLog}");
            }

            // Compile fragment shader
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            // Check fragment shader compilation
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentSuccess);
            if (fragmentSuccess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                throw new Exception($"Fragment shader compilation failed: {infoLog}");
            }

            // Create shader program
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            // Check program linking
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int linkSuccess);
            if (linkSuccess == 0)
            {
                string infoLog = GL.GetProgramInfoLog(shaderProgram);
                throw new Exception($"Shader program linking failed: {infoLog}");
            }

            // Clean up individual shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Get uniform locations
            GetUniformLocations();
        }

        private void GetUniformLocations()
        {
            iResolutionLocation = GL.GetUniformLocation(shaderProgram, "iResolution");
            iTimeLocation = GL.GetUniformLocation(shaderProgram, "iTime");
            iTimeDeltaLocation = GL.GetUniformLocation(shaderProgram, "iTimeDelta");
            iFrameLocation = GL.GetUniformLocation(shaderProgram, "iFrame");
            iFrameRateLocation = GL.GetUniformLocation(shaderProgram, "iFrameRate");
            iMouseLocation = GL.GetUniformLocation(shaderProgram, "iMouse");
            iDateLocation = GL.GetUniformLocation(shaderProgram, "iDate");
            iSampleRateLocation = GL.GetUniformLocation(shaderProgram, "iSampleRate");
        }

        public void Render(float time, Vector3 resolution)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(shaderProgram);

            // Set Shadertoy-compatible uniforms
            if (iResolutionLocation >= 0)
                GL.Uniform3(iResolutionLocation, resolution);

            if (iTimeLocation >= 0)
                GL.Uniform1(iTimeLocation, time);

            float deltaTime = time - lastTime;
            if (iTimeDeltaLocation >= 0)
                GL.Uniform1(iTimeDeltaLocation, deltaTime);

            if (iFrameLocation >= 0)
                GL.Uniform1(iFrameLocation, frameCount);

            float frameRate = deltaTime > 0 ? 1.0f / deltaTime : 60.0f;
            if (iFrameRateLocation >= 0)
                GL.Uniform1(iFrameRateLocation, frameRate);

            // Mouse (not implemented for screensaver)
            if (iMouseLocation >= 0)
                GL.Uniform4(iMouseLocation, 0.0f, 0.0f, 0.0f, 0.0f);

            // Date
            DateTime now = DateTime.Now;
            if (iDateLocation >= 0)
            {
                Vector4 date = new Vector4(
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour * 3600 + now.Minute * 60 + now.Second
                );
                GL.Uniform4(iDateLocation, date);
            }

            // Sample rate (audio - default 44100)
            if (iSampleRateLocation >= 0)
                GL.Uniform1(iSampleRateLocation, 44100.0f);

            // Draw fullscreen quad
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            lastTime = time;
            frameCount++;
        }

        public void Dispose()
        {
            if (shaderProgram != 0)
            {
                GL.DeleteProgram(shaderProgram);
            }
            if (VAO != 0)
            {
                GL.DeleteVertexArray(VAO);
            }
            if (VBO != 0)
            {
                GL.DeleteBuffer(VBO);
            }
            if (EBO != 0)
            {
                GL.DeleteBuffer(EBO);
            }
        }
    }
}