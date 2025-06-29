using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.GLControl;

namespace ShaderSaver
{
    public class ShaderForm : Form
    {
        private ShaderRenderer? renderer;
        private SimpleRenderer simpleRenderer;
        private GLControl? glControl;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer shaderCycleTimer;
        private DateTime startTime;
        private Point lastMousePosition;
        private bool isPreview;
        private IntPtr previewHandle;
        private bool useOpenGL = false;
        private int currentShaderIndex = 0;
        private string[] shaderFiles = { "shader.txt", "shader2.txt", "shader3.txt", "shader4.txt", "shader5.txt", "shader6.txt", "shader7.txt", "shader8.txt", "shader9.txt", "shader10.txt", "shader12.txt", "shader13.txt" };

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, ref Rectangle lpRect);

        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;

        public ShaderForm(Screen screen)
        {
            InitializeComponent();
            InitializeForScreen(screen);
            isPreview = false;
        }

        public ShaderForm(IntPtr previewHandle)
        {
            InitializeComponent();
            this.previewHandle = previewHandle;
            InitializeForPreview();
            isPreview = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.Black;
            this.ClientSize = new Size(800, 600);
            this.Name = "ShaderForm";
            this.Text = "Shader Screensaver";
            this.KeyPreview = true; // Important: allows form to receive key events even when child controls have focus
            this.ResumeLayout(false);
        }

        private void InitializeForScreen(Screen screen)
        {
            // Setup form for fullscreen screensaver
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Black;

            // Set bounds explicitly to handle DPI correctly
            this.SetBounds(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

            // Force the window to maximize on the correct screen
            this.WindowState = FormWindowState.Maximized;

            // Hide mouse cursor during screensaver
            Cursor.Hide();

            SetupRenderer();
            SetupEvents();
        }

        private void InitializeForPreview()
        {
            // Setup form for preview window
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(200, 150);
            this.BackColor = Color.Black;

            SetupRenderer();

            // Embed in preview window
            SetParent(this.Handle, previewHandle);
            SetWindowLong(this.Handle, GWL_STYLE, new IntPtr(GetWindowLong(this.Handle, GWL_STYLE) | WS_CHILD));

            Rectangle parentRect = new Rectangle();
            GetClientRect(previewHandle, ref parentRect);
            SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, parentRect.Right, parentRect.Bottom, 0);
        }

        private void SetupRenderer()
        {
            try
            {
                // Try to create OpenGL control with simpler settings
                var settings = new GLControlSettings()
                {
                    APIVersion = new Version(3, 3)
                };

                glControl = new GLControl(settings)
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };

                this.Controls.Add(glControl);

                glControl.Load += GlControl_Load;
                glControl.Paint += GlControl_Paint;
                glControl.Resize += GlControl_Resize;

                // Add input event handlers to GLControl as well
                if (!isPreview)
                {
                    glControl.KeyDown += ShaderForm_KeyDown;
                    glControl.MouseMove += ShaderForm_MouseMove;
                    glControl.MouseClick += ShaderForm_MouseClick;
                    glControl.MouseDown += ShaderForm_MouseDown;
                }

                useOpenGL = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create OpenGL control: {ex.Message}");
                // Fall back to GDI+ rendering
                useOpenGL = false;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
                this.Paint += ShaderForm_Paint;
            }

            this.Load += ShaderForm_Load;
            this.Resize += ShaderForm_Resize;
        }

        private void SetupEvents()
        {
            if (!isPreview)
            {
                // Set up input event handlers for the form
                this.KeyDown += ShaderForm_KeyDown;
                this.MouseMove += ShaderForm_MouseMove;
                this.MouseClick += ShaderForm_MouseClick;
                this.MouseDown += ShaderForm_MouseDown;

                // Also handle when form is activated (clicked)
                this.Activated += ShaderForm_Activated;

                // Start animation timer
                animationTimer = new System.Windows.Forms.Timer();
                animationTimer.Interval = 16; // ~60 FPS
                animationTimer.Tick += AnimationTimer_Tick;
                animationTimer.Start();

                // Start shader cycle timer with configurable interval
                shaderCycleTimer = new System.Windows.Forms.Timer();
                int cycleTimeSeconds = SettingsForm.GetCycleTimeSeconds();
                shaderCycleTimer.Interval = cycleTimeSeconds * 1000; // Convert seconds to milliseconds
                shaderCycleTimer.Tick += ShaderCycleTimer_Tick;
                shaderCycleTimer.Start();
            }

            startTime = DateTime.Now;
            lastMousePosition = Cursor.Position;
        }

        private void ShaderForm_Load(object sender, EventArgs e)
        {
            // Always create simple renderer as fallback
            simpleRenderer = new SimpleRenderer();
            simpleRenderer.Initialize();

            // Load the first shader from the array using embedded resources
            string shaderPath = shaderFiles[currentShaderIndex];
            try
            {
                simpleRenderer.LoadShader(shaderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load shader {shaderPath} in simple renderer: {ex.Message}");
            }

            // Store initial mouse position when screensaver starts
            lastMousePosition = Cursor.Position;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            if (glControl == null) return;

            glControl.MakeCurrent();

            // Initialize OpenGL
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            try
            {
                // Create shader renderer
                renderer = new ShaderRenderer();
                renderer.Initialize();

                // Load the first shader from the array using embedded resources
                string shaderPath = shaderFiles[currentShaderIndex];
                try
                {
                    renderer.LoadShader(shaderPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load shader {shaderPath} in OpenGL renderer: {ex.Message}");
                }

                Console.WriteLine("OpenGL shader renderer initialized successfully!");
                useOpenGL = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenGL renderer initialization failed: {ex.Message}");
                useOpenGL = false;
                renderer?.Dispose();
                renderer = null;
            }
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (glControl == null || renderer == null) return;

            glControl.MakeCurrent();

            float time = (float)(DateTime.Now - startTime).TotalSeconds;
            var resolution = new Vector3(glControl.Width, glControl.Height, 1.0f);

            // Debug output for the first few frames to help diagnose resolution issues
            if (DateTime.Now.Subtract(startTime).TotalSeconds < 5.0)
            {
                Console.WriteLine($"Rendering at resolution: {resolution.X}x{resolution.Y} on form size: {this.Width}x{this.Height}");
            }

            try
            {
                renderer.Render(time, resolution);
                glControl.SwapBuffers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenGL rendering failed: {ex.Message}");
                useOpenGL = false;
                renderer?.Dispose();
                renderer = null;
            }
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (glControl?.IsHandleCreated == true)
            {
                glControl.MakeCurrent();

                // Set viewport to match the actual control size
                int width = glControl.Width;
                int height = glControl.Height;

                // Ensure minimum size to prevent division by zero
                if (width < 1) width = 1;
                if (height < 1) height = 1;

                GL.Viewport(0, 0, width, height);

                // Debug output to help diagnose resolution issues
                Console.WriteLine($"GLControl resized to: {width}x{height}");
            }
        }

        private void ShaderForm_Paint(object sender, PaintEventArgs e)
        {
            // Fallback GDI+ rendering when OpenGL isn't available
            if (!useOpenGL)
            {
                float time = (float)(DateTime.Now - startTime).TotalSeconds;
                simpleRenderer.Render(e.Graphics, time, this.Size);
            }
        }

        private void ShaderForm_Resize(object sender, EventArgs e)
        {
            if (!useOpenGL)
            {
                this.Invalidate();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (useOpenGL)
            {
                glControl?.Invalidate();
            }
            else
            {
                this.Invalidate();
            }
        }

        private void ShaderCycleTimer_Tick(object sender, EventArgs e)
        {
            // Cycle to the next shader
            currentShaderIndex = (currentShaderIndex + 1) % shaderFiles.Length;
            string shaderPath = shaderFiles[currentShaderIndex];

            // Update the simple renderer using embedded resources
            try
            {
                simpleRenderer.LoadShader(shaderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update simple renderer: {ex.Message}");
            }

            // Update the OpenGL renderer if available using embedded resources
            if (useOpenGL && renderer != null && glControl != null)
            {
                try
                {
                    glControl.MakeCurrent();
                    renderer.LoadShader(shaderPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load shader {shaderPath} in OpenGL: {ex.Message}");
                }
            }
        }

        private void ShaderForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Any key press closes the screensaver
            if (!isPreview)
            {
                Cursor.Show(); // Show cursor before exit
                Application.Exit();
            }
        }

        private void ShaderForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPreview)
            {
                Point currentPosition = Cursor.Position;
                // Check if mouse has moved more than a few pixels
                if (Math.Abs(currentPosition.X - lastMousePosition.X) > 3 ||
                    Math.Abs(currentPosition.Y - lastMousePosition.Y) > 3)
                {
                    Cursor.Show(); // Show cursor before exit
                    Application.Exit();
                }
            }
        }

        private void ShaderForm_MouseClick(object sender, MouseEventArgs e)
        {
            // Any mouse click closes the screensaver
            if (!isPreview)
            {
                Cursor.Show(); // Show cursor before exit
                Application.Exit();
            }
        }

        private void ShaderForm_MouseDown(object sender, MouseEventArgs e)
        {
            // Any mouse button press closes the screensaver
            if (!isPreview)
            {
                Cursor.Show(); // Show cursor before exit
                Application.Exit();
            }
        }

        private void ShaderForm_Activated(object sender, EventArgs e)
        {
            // If the form gets activated (user clicked somewhere), close screensaver
            if (!isPreview && (DateTime.Now - startTime).TotalSeconds > 1) // Allow 1 second grace period for initial activation
            {
                Cursor.Show(); // Show cursor before exit
                Application.Exit();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Catch all key presses, including special keys
            if (!isPreview)
            {
                Cursor.Show(); // Show cursor before exit
                Application.Exit();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            Cursor.Show(); // Make sure cursor is shown when disposing
            animationTimer?.Stop();
            animationTimer?.Dispose();
            shaderCycleTimer?.Stop();
            shaderCycleTimer?.Dispose();
            renderer?.Dispose();
            simpleRenderer?.Dispose();
            glControl?.Dispose();
            base.Dispose(disposing);
        }
    }
}