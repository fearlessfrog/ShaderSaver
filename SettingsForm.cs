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
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(12);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Shown += SettingsForm_Shown;

            var tableLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.Controls.Add(tableLayout);

            // Instruction label
            instructionLabel = new Label
            {
                Text = "ShaderSaver cycles through 12 built-in shader effects automatically.",
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                MaximumSize = new Size(460, 0)
            };
            tableLayout.Controls.Add(instructionLabel, 0, 0);

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
                AutoSize = true,
                Font = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size - 0.5f),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                MaximumSize = new Size(460, 0)
            };
            tableLayout.Controls.Add(shaderListLabel, 0, 1);

            // Attribution label
            var attributionLabel = new Label
            {
                Text = "Special thanks to @XorDev and Xor for creating these beautiful shader effects.\n" +
                       "Original shaders available on Shadertoy and Twitter/X.",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Font = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size - 0.5f, FontStyle.Italic),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 15),
                MaximumSize = new Size(460, 0)
            };
            tableLayout.Controls.Add(attributionLabel, 0, 2);

            var cycleTimePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            tableLayout.Controls.Add(cycleTimePanel, 0, 3);

            // Cycle time label
            cycleTimeLabel = new Label
            {
                Text = "Time between shader changes (seconds):",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 5, 0, 0)
            };
            cycleTimePanel.Controls.Add(cycleTimeLabel);

            // Cycle time numeric up/down
            cycleTimeNumericUpDown = new NumericUpDown
            {
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 300, // Max 5 minutes
                Value = 10, // Default 10 seconds
                DecimalPlaces = 0,
                Anchor = AnchorStyles.Left
            };
            cycleTimePanel.Controls.Add(cycleTimeNumericUpDown);

            // Add help text
            var helpLabel = new Label
            {
                Text = "Set how long each shader effect displays before switching to the next one.\nRecommended: 10-30 seconds for best visual experience.",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 0),
                MaximumSize = new Size(460, 0)
            };
            tableLayout.Controls.Add(helpLabel, 0, 4);

            var buttonsPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Right,
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 0),
            };
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableLayout.Controls.Add(buttonsPanel, 0, 5);

            // OK button
            okButton = new Button
            {
                Text = "OK",
                Size = new Size(95, 35),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(95, 35),
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += (s, e) => this.Close();

            buttonsPanel.Controls.Add(cancelButton, 0, 0);
            buttonsPanel.Controls.Add(okButton, 2, 0);
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            UpdateLabelWidths();
        }

        private void UpdateLabelWidths()
        {
            var tableLayout = this.Controls[0] as TableLayoutPanel;
            if (tableLayout != null && tableLayout.ClientRectangle.Width > 0)
            {
                int availableWidth = tableLayout.ClientRectangle.Width - tableLayout.Padding.Horizontal;
                instructionLabel.MaximumSize = new Size(availableWidth, 0);
                shaderListLabel.MaximumSize = new Size(availableWidth, 0);
                var attributionLabel = tableLayout.GetControlFromPosition(0, 2) as Label;
                if (attributionLabel != null)
                {
                    attributionLabel.MaximumSize = new Size(availableWidth, 0);
                }
                var helpLabel = tableLayout.GetControlFromPosition(0, 4) as Label;
                if (helpLabel != null)
                {
                    helpLabel.MaximumSize = new Size(availableWidth, 0);
                }
            }
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