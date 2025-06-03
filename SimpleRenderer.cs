using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShaderSaver
{
    public class SimpleRenderer : IDisposable
    {
        private string shaderCode = "";
        private bool disposed = false;

        public void Initialize()
        {
            // Nothing to initialize for GDI+ rendering
        }

        public void LoadShader(string filePath)
        {
            try
            {
                shaderCode = ShaderLoader.LoadShaderFromFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shader: {ex.Message}");
                shaderCode = "";
            }
        }

        public void Render(Graphics graphics, float time, Size resolution)
        {
            if (disposed) return;

            // Create a simple animated effect using GDI+
            // This is a fallback when OpenGL isn't available

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Black);

            // Create a swirling effect
            int centerX = resolution.Width / 2;
            int centerY = resolution.Height / 2;
            float maxRadius = Math.Min(centerX, centerY) * 0.8f;

            // Create multiple rotating elements
            for (int i = 0; i < 8; i++)
            {
                float angle = time * 0.5f + (i * MathF.PI * 2 / 8);
                float radius = maxRadius * (0.3f + 0.7f * (i / 8.0f));

                float x = centerX + MathF.Cos(angle) * radius * 0.3f;
                float y = centerY + MathF.Sin(angle) * radius * 0.3f;

                // Color based on time and position
                int r = (int)(127 + 127 * MathF.Sin(time * 2 + i * 0.5f));
                int g = (int)(127 + 127 * MathF.Sin(time * 1.5f + i * 0.7f + MathF.PI * 2 / 3));
                int b = (int)(127 + 127 * MathF.Sin(time * 1.8f + i * 0.3f + MathF.PI * 4 / 3));

                Color color = Color.FromArgb(128, r, g, b);

                using (var brush = new SolidBrush(color))
                {
                    float size = 30 + 20 * MathF.Sin(time * 3 + i);
                    graphics.FillEllipse(brush, x - size/2, y - size/2, size, size);
                }
            }

            // Add some rotating spirals
            using (var pen = new Pen(Color.FromArgb(64, 255, 255, 255), 2))
            {
                for (int spiral = 0; spiral < 3; spiral++)
                {
                    var points = new PointF[100];
                    for (int j = 0; j < points.Length; j++)
                    {
                        float t = j / (float)(points.Length - 1);
                        float spiralAngle = time * (1 + spiral * 0.3f) + t * MathF.PI * 6;
                        float spiralRadius = t * maxRadius * 0.6f;

                        points[j] = new PointF(
                            centerX + MathF.Cos(spiralAngle) * spiralRadius,
                            centerY + MathF.Sin(spiralAngle) * spiralRadius
                        );
                    }

                    if (points.Length > 1)
                    {
                        graphics.DrawLines(pen, points);
                    }
                }
            }

            // Add some particle effects
            Random rand = new Random((int)(time * 1000) % 1000);
            for (int i = 0; i < 50; i++)
            {
                float particleTime = time + i * 0.1f;
                float x = (rand.Next(resolution.Width) + particleTime * 50) % resolution.Width;
                float y = (rand.Next(resolution.Height) + particleTime * 30) % resolution.Height;

                float alpha = 127 * (0.5f + 0.5f * MathF.Sin(particleTime * 5));
                Color particleColor = Color.FromArgb((int)alpha, 255, 255, 255);

                using (var brush = new SolidBrush(particleColor))
                {
                    graphics.FillEllipse(brush, x - 1, y - 1, 2, 2);
                }
            }

            // Add time-based color overlay
            int overlayAlpha = (int)(30 + 20 * MathF.Sin(time * 0.5f));
            using (var overlay = new SolidBrush(Color.FromArgb(overlayAlpha,
                (int)(127 + 127 * MathF.Sin(time * 0.7f)),
                (int)(127 + 127 * MathF.Sin(time * 0.9f + MathF.PI * 2 / 3)),
                (int)(127 + 127 * MathF.Sin(time * 1.1f + MathF.PI * 4 / 3)))))
            {
                graphics.FillRectangle(overlay, 0, 0, resolution.Width, resolution.Height);
            }
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}