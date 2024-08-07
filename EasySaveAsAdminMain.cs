using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EasySaveAsAdmin.Admin;
using EasySaveAsAdmin.FileTools;
using EasySaveAsAdmin.PluginInfrastructure;
using EasySaveAsAdmin.Utils;

namespace EasySaveAsAdmin
{
    internal static class EasySaveAsAdminMain
    {
        #region " Fields "
        internal const string PluginName = "Easy Save as Admin";
        public const bool IsShuttingDown = false;
        private static bool _adminSaveToolUsed;
        //private static NppListener _nppListener = null;
        #endregion

        internal static void CommandMenuInit()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadDependency;
            PluginBase.SetCommand(1, "A&bout", ShowAbout);
            //_nppListener = new NppListener();
            //_nppListener.AssignHandle(PluginBase.NppData._nppHandle);
        }

        private static void ShowAbout()
        {
            var about = new About.About();
            about.StartPosition = FormStartPosition.CenterParent;
            about.ShowDialog();
            about.Focus();
        }

        private static Assembly LoadDependency(object sender, ResolveEventArgs args)
        {
            var assemblyFile = Path.Combine(Npp.PluginDllDirectory, new AssemblyName(args.Name).Name) + ".dll";
            return File.Exists(assemblyFile) ? Assembly.LoadFrom(assemblyFile) : null;
        }

        public static void OnNotification(ScNotification notification)
        {
            var code = notification.Header.Code;
            const uint saveCode = (uint)NppMsg.NPPN_FILEBEFORESAVE;

            switch (code)
            {
                case saveCode:
                {
                    var filePath = Npp.Notepad.GetCurrentFilePath();
                    if (FileTool.CanSave(filePath))
                    {
                        return;
                    }
                    
                    var text = Npp.Editor.GetText();
                    var bytes = Encoding.UTF8.GetBytes(text);
                    var base64String = Convert.ToBase64String(bytes);
                    _adminSaveToolUsed = AdminClass.SaveAsAdmin(filePath, base64String);
                    break;
                }
                case (uint)NppMsg.NPPN_FILESAVED:
                {
                    var filePath = Npp.Notepad.GetCurrentFilePath();
                    if (_adminSaveToolUsed)
                    {
                        Thread.Sleep(1000);
                        Npp.Notepad.ReloadFile(filePath);
                    }
                    break;
                }
            }
        }
    }
}   
