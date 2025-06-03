using System;
using System.Windows.Forms;

namespace ShaderSaver
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
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

            foreach (Screen screen in Screen.AllScreens)
            {
                var form = new ShaderForm(screen);
                forms.Add(form);
                form.Show();
            }

            // Run the application
            if (forms.Count > 0)
            {
                Application.Run();
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