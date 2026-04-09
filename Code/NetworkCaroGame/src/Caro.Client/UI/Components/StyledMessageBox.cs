//namespace Caro.Client.UI.Components { public class StyledMessageBox { } }
using System.Windows.Forms;

namespace Caro.Client.UI.Components
{
    public static class StyledMessageBox
    {
        public static void Show(string message, string title = "Info")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}