using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ShaderSaver
{
    static class Program
    {


        [STAThread]
        static void Main(string[] args)
        {
            // Set DPI awareness using the modern .NET 6 API
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Parse command line arguments for screensaver modes
            if (args.Length == 0)
            {
                // No arguments - show settings dialog
                ShowSettingsDialog();
            }
            else if (args[0].ToLower().StartsWith("/s"))
            {
                // Screensaver mode
                ShowScreensaver();
            }
            else if (args[0].ToLower().StartsWith("/p"))
            {
                // Preview mode
                if (args.Length > 1 && IntPtr.TryParse(args[1], out IntPtr previewHandle))
                {
                    ShowPreview(previewHandle);
                }
            }
            else if (args[0].ToLower().StartsWith("/c"))
            {
                // Configuration mode
                ShowSettingsDialog();
            }
        }



        private static void ShowScreensaver()
        {
            // Create fullscreen shader forms for each monitor
            var forms = new List<ShaderForm>();
            ShaderForm? primaryForm = null;

            foreach (Screen screen in Screen.AllScreens)
            {
                var form = new ShaderForm(screen);
                forms.Add(form);

                // Keep track of the primary screen form
                if (screen.Primary)
                {
                    primaryForm = form;
                }

                form.Show();

                // Small delay to prevent timing issues with multiple monitors
                System.Threading.Thread.Sleep(10);
            }

            // Run the application with the primary form as the main form
            if (primaryForm != null)
            {
                Application.Run(primaryForm);
            }
            else if (forms.Count > 0)
            {
                Application.Run(forms[0]);
            }
        }

        private static void ShowPreview(IntPtr previewHandle)
        {
            // Show preview in the small preview window
            var previewForm = new ShaderForm(previewHandle);
            Application.Run(previewForm);
        }

        private static void ShowSettingsDialog()
        {
            // Show settings dialog
            using (var settingsForm = new SettingsForm())
            {
                Application.Run(settingsForm);
            }
        }
    }
}