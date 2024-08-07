using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EasySaveAsAdmin.Admin
{
    public static class AdminClass
    {
        public static bool SaveAsAdmin(string path, string text)
        {
            RunSaveToolWithAdminPrivs(path, text);
            return true;
        }

        private static void RunSaveToolWithAdminPrivs(string path, string text)
        {
            //Start process with admin rights
            try
            {
                var saveAsAppPath = Environment.CurrentDirectory + "\\plugins\\EasySaveAsAdmin" + "\\SaveAsAdminHelper.exe";
                if (File.Exists(saveAsAppPath) == false)
                {
                    MessageBox.Show("Plugin is not installed correctly. Please reinstall the plugin", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var process = new Process();
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = true;
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