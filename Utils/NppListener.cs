using System;
using System.Windows.Forms;

namespace EasySaveAsAdmin.Utils
{
    /// <summary>
    /// This class listens to messages that are not broadcast by the plugins manager (e.g., Notepad++ internal messages).<br></br>
    /// The original idea comes from Peter Frentrup's NppMenuSearch plugin (https://github.com/search?q=repo%3Apeter-frentrup%2FNppMenuSearch%20NppListener&type=code)
    /// </summary>
    public class NppListener : NativeWindow
    {
        protected override void WndProc(ref Message m)
        {
            if (!EasySaveAsAdminMain.IsShuttingDown)
            {
                Console.WriteLine(m.Msg);
                switch (m.Msg)
                {
                   
                }
            }

            base.WndProc(ref m);
        }
    }
}