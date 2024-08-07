using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace EasySaveAsAdmin.Admin
{
    public static class AdminClass
    {
        public static bool SaveAsAdmin(string path, string text)
        {
            if (IsAdmin())
            {
                return false;
            }

            if (CanSaveWithoutAdmin(path))
            {
                return false;
            }

            RunSaveToolWithAdminPrivs(path, text);
            return true;
        }

        private static bool CanSaveWithoutAdmin(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    return fileStream.CanWrite;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool IsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RunSaveToolWithAdminPrivs(string path, string text)
        {
            //Start process with admin rights
            try
            {
                var saveAsAppPath = Environment.CurrentDirectory + "\\plugins\\EasySaveAsAdmin" + "\\SaveAsAdmin.exe";
                if (File.Exists(saveAsAppPath) == false)
                {
                    MessageBox.Show("Plugin is not installed correctly. Please reinstall the plugin", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                var process = new Process();
                process.StartInfo.FileName = saveAsAppPath;
                process.StartInfo.Arguments = $"\"{path}\" \"{text}\"";
                process.StartInfo.Verb = "runas";
                process.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}