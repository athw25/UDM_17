
//using System;
//using System.Windows.Forms;

//namespace Caro.Client
//{
//    static class Program
//    {
//        [STAThread]
//        static void Main()
//        {
//            Application.EnableVisualStyles();
//            Application.Run(new UI.Forms.LoginForm());
//        }
//    }
//}
using System;
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

            Application.Run(new LoginForm());
        }
    }
}