using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;

namespace ShaderSaver
{
    public class ShaderLoader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static string LoadShaderFromFile(string filePath)
        {
            try
            {
                string shaderCode = File.ReadAllText(filePath);
                return ConvertToOpenGLShader(shaderCode);
            }
            catch (Exception)
            {
                // If file loading fails, try embedded resource
                return LoadShaderFromEmbeddedResource(filePath);
            }
        }

        public static string LoadShaderFromEmbeddedResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = $"ShaderSaver.{resourceName}";

                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        Console.WriteLine($"Embedded resource not found: {resourcePath}");
                        return GetDefaultShader();
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return ConvertToOpenGLShader(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading embedded resource {resourceName}: {ex.Message}");
                return GetDefaultShader();
            }
        }

        private static string ConvertToOpenGLShader(string shaderCode)
        {
            return ProcessShaderCode(shaderCode);
        }

        public static async Task<string> LoadShadertoyShaderAsync(string shadertoyUrl)
        {
            try
            {
                // Extract shader ID from URL
                var match = Regex.Match(shadertoyUrl, @"shadertoy\.com/view/(\w+)");
                if (!match.Success)
                {
                    throw new ArgumentException("Invalid Shadertoy URL format");
                }

                string shaderId = match.Groups[1].Value;

                // Note: This would require Shadertoy API access in a real implementation
                // For now, return a placeholder that shows how to structure shaders
                return GetDefaultShader();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load Shadertoy shader: {ex.Message}");
                return GetDefaultShader();
            }
        }

        private static string ProcessShaderCode(string shaderCode)
        {
            // Convert Shadertoy format to our OpenGL format
            var sb = new StringBuilder();

            // Add version directive if not present
            if (!shaderCode.Contains("#version"))
            {
                sb.AppendLine("#version 330 core");
                sb.AppendLine();
            }

            // Add our uniform declarations
            sb.AppendLine("// Shadertoy-compatible uniforms");
            sb.AppendLine("uniform vec3 iResolution;");
            sb.AppendLine("uniform float iTime;");
            sb.AppendLine("uniform float iTimeDelta;");
            sb.AppendLine("uniform int iFrame;");
            sb.AppendLine("uniform float iFrameRate;");
            sb.AppendLine("uniform vec4 iMouse;");
            sb.AppendLine("uniform vec4 iDate;");
            sb.AppendLine("uniform float iSampleRate;");
            sb.AppendLine("uniform sampler2D iChannel0;");
            sb.AppendLine("uniform sampler2D iChannel1;");
            sb.AppendLine("uniform sampler2D iChannel2;");
            sb.AppendLine("uniform sampler2D iChannel3;");
            sb.AppendLine();
            sb.AppendLine("out vec4 fragColor;");
            sb.AppendLine();

            // Process the shader code
            string processedCode = shaderCode;

            // If it contains mainImage function (Shadertoy style), wrap it
            if (processedCode.Contains("mainImage"))
            {
                sb.AppendLine(processedCode);
                sb.AppendLine();
                sb.AppendLine("void main() {");
                sb.AppendLine("    mainImage(fragColor, gl_FragCoord.xy);");
                sb.AppendLine("}");
            }
            else if (processedCode.Contains("void main()"))
            {
                // Already has main function, just append
                sb.AppendLine(processedCode);
            }
            else
            {
                // Assume it's shader code without main, create a basic wrapper
                sb.AppendLine(processedCode);
                sb.AppendLine();
                sb.AppendLine("void main() {");
                sb.AppendLine("    vec2 uv = gl_FragCoord.xy / iResolution.xy;");
                sb.AppendLine("    fragColor = vec4(uv, 0.5 + 0.5 * sin(iTime), 1.0);");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        public static string GetDefaultShader()
        {
            return @"#version 330 core

// Shadertoy-compatible uniforms
uniform vec3 iResolution;
uniform float iTime;
uniform float iTimeDelta;
uniform int iFrame;
uniform float iFrameRate;
uniform vec4 iMouse;
uniform vec4 iDate;
uniform float iSampleRate;
uniform sampler2D iChannel0;
uniform sampler2D iChannel1;
uniform sampler2D iChannel2;
uniform sampler2D iChannel3;

out vec4 fragColor;

#define PI 3.14159265359
#define TAU 6.28318530718

// Simple colorful swirl effect
void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;

    float len = length(uv);
    float angle = atan(uv.y, uv.x);

    // Create swirling pattern
    float swirl = angle + iTime * 2.0 + sin(len * 10.0 - iTime * 3.0) * 0.5;

    // Color based on position and time
    vec3 color = vec3(
        0.5 + 0.5 * sin(swirl * 2.0),
        0.5 + 0.5 * sin(swirl * 2.0 + PI * 2.0 / 3.0),
        0.5 + 0.5 * sin(swirl * 2.0 + PI * 4.0 / 3.0)
    );

    // Add some fade from center
    color *= 1.0 - smoothstep(0.0, 1.0, len);

    // Add time-based brightness variation
    color *= 0.8 + 0.2 * sin(iTime);

    fragColor = vec4(color, 1.0);
}

void main() {
    mainImage(fragColor, gl_FragCoord.xy);
}";
        }
    }
}