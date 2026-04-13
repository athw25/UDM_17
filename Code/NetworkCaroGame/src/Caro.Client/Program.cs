using System;
using System.IO;
using System.Windows.Forms;
using Caro.Client.UI.Forms;

namespace Caro.Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => { File.WriteAllText("CrashLog.txt", e.Exception.ToString()); Application.Exit(); };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { File.WriteAllText("CrashLog.txt", e.ExceptionObject.ToString()); Application.Exit(); };
            try { Application.Run(new LoginForm()); } catch (Exception ex) { File.WriteAllText("CrashLog.txt", ex.ToString()); }
        }
    }
}
