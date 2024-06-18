using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskalniPrevoditelj.Servisi
{
    public class StartupServis
    {
        const string appName = "FiskalniPrevoditelj"; // Name of your application
        public string GetExecutablePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDirectory = Path.Combine(localAppData, "FiskalniPrevoditelj", "current");
            string executablePath = Path.Combine(appDirectory, "FiskalniPrevoditelj.exe");
            return executablePath;
        }
        public  void AddApplicationToStartup()
        {
            try
            {
                string executablePath = GetExecutablePath();
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue(appName, executablePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to startup: {ex.Message}");
            }
        }

        public  void RemoveApplicationFromStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue(appName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing from startup: {ex.Message}");
            }
        }
        public  bool IsApplicationInStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    if (key != null)
                    {
                        string value = key.GetValue(appName) as string;
                        if (value != null)
                        {
                            string executablePath = GetExecutablePath();
                            return value == executablePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking startup: {ex.Message}");
            }
            return false;
        }

    }
}
