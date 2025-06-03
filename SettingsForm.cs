using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ShaderSaver
{
    public partial class SettingsForm : Form
    {
        private NumericUpDown cycleTimeNumericUpDown;
        private Button okButton;
        private Button cancelButton;
        private Label instructionLabel;
        private Label cycleTimeLabel;
        private Label shaderListLabel;

        public int CycleTimeSeconds { get; private set; } = 10; // Default 10 seconds

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Shader Screensaver Settings";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Instruction label
            instructionLabel = new Label
            {
                Text = "ShaderSaver cycles through 12 built-in shader effects automatically.",
                Size = new Size(460, 20),
                AutoSize = false,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            this.Controls.Add(instructionLabel);

            // Shader list label
            shaderListLabel = new Label
            {
                Text = "Built-in Shader Effects:\n\n" +
                       "1. \"Singularity\" by @XorDev - Black hole with gravitational lensing\n" +
                       "2. \"Sunset\" by @XorDev - Volumetric clouds and atmospheric effects\n" +
                       "3. \"Starship\" by @XorDev - Particle debris inspired by SpaceX tests\n" +
                       "4. \"Origami\" by @XorDev - Soft paper-like geometric shapes\n" +
                       "5. \"Shield\" by @XorDev - Hexagonal energy shield patterns\n" +
                       "6. \"Ghosts\" by @XorDev - 3D volumetric spirits with turbulence\n" +
                       "7. \"Waveform\" by @XorDev - Audio-inspired wave patterns\n" +
                       "8. \"Water Ripples\" - Realistic water simulation with drops\n" +
                       "9. \"Simplex\" by @XorDev - Geometric grid patterns and shapes\n" +
                       "10. \"Terraform\" by Xor - Procedural terrain generation\n" +
                       "11. \"DNA\" by Xor - Double helix molecular visualization\n" +
                       "12. \"Rainbow Road\" by @XorDev - Infinite rainbow highway",
                Location = new Point(12, 45),
                Size = new Size(460, 200),
                AutoSize = false,
                Font = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size - 0.5f)
            };
            this.Controls.Add(shaderListLabel);

            // Attribution label
            var attributionLabel = new Label
            {
                Text = "Special thanks to @XorDev and Xor for creating these beautiful shader effects.\n" +
                       "Original shaders available on Shadertoy and Twitter/X.",
                Location = new Point(12, 255),
                Size = new Size(460, 35),
                AutoSize = false,
                ForeColor = SystemColors.GrayText,
                Font = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size - 0.5f, FontStyle.Italic)
            };
            this.Controls.Add(attributionLabel);

            // Cycle time label
            cycleTimeLabel = new Label
            {
                Text = "Time between shader changes (seconds):",
                Location = new Point(12, 305),
                Size = new Size(220, 20),
                AutoSize = false
            };
            this.Controls.Add(cycleTimeLabel);

            // Cycle time numeric up/down
            cycleTimeNumericUpDown = new NumericUpDown
            {
                Location = new Point(240, 303),
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 300, // Max 5 minutes
                Value = 10, // Default 10 seconds
                DecimalPlaces = 0
            };
            this.Controls.Add(cycleTimeNumericUpDown);

            // Add help text
            var helpLabel = new Label
            {
                Text = "Set how long each shader effect displays before switching to the next one.\nRecommended: 10-30 seconds for best visual experience.",
                Location = new Point(12, 335),
                Size = new Size(460, 35),
                AutoSize = false,
                ForeColor = SystemColors.GrayText
            };
            this.Controls.Add(helpLabel);

            // OK button
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(320, 410),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(405, 410),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);
        }

        private void LoadSettings()
        {
            try
            {
                // Load cycle time from registry, default to 10 seconds
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ShaderSaver"))
                {
                    var cycleTime = key?.GetValue("CycleTimeSeconds");
                    if (cycleTime != null && int.TryParse(cycleTime.ToString(), out int savedTime))
                    {
                        if (savedTime >= 1 && savedTime <= 300)
                        {
                            CycleTimeSeconds = savedTime;
                            cycleTimeNumericUpDown.Value = savedTime;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If registry access fails, use default value
                CycleTimeSeconds = 10;
            }
        }

        private void SaveSettings()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ShaderSaver"))
                {
                    key?.SetValue("CycleTimeSeconds", CycleTimeSeconds);
                }
            }
            catch (Exception)
            {
                // If registry access fails, settings won't persist but the app will still work
                MessageBox.Show("Settings could not be saved to registry, but will be used for this session.",
                               "Settings Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            CycleTimeSeconds = (int)cycleTimeNumericUpDown.Value;
            SaveSettings();
            this.Close();
        }

        public static int GetCycleTimeSeconds()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ShaderSaver"))
                {
                    var cycleTime = key?.GetValue("CycleTimeSeconds");
                    if (cycleTime != null && int.TryParse(cycleTime.ToString(), out int savedTime))
                    {
                        if (savedTime >= 1 && savedTime <= 300)
                        {
                            return savedTime;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If registry access fails, return default
            }
            return 10; // Default 10 seconds
        }
    }
}