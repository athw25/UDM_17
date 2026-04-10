using System.Windows.Forms;

namespace Caro.Client.UI.Helpers
{
    public static class UIHelper
    {
        public static void SwitchForm(Form current, Form next)
        {
            current.Hide();
            next.Show();
        }
    }
}