
using System;
using System.Windows.Forms;

namespace Caro.Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new UI.Forms.LoginForm());
        }
    }
}
