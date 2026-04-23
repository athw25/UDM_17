using System.Windows.Forms;

namespace Caro.Client.UI.Helpers
{
    public static class UIHelper
    {
        public static void SwitchForm(Form current, Form next)
        {
            // Properly dispose the current form before switching to the new one
            // This prevents multiple forms from being held in memory
            current.Hide();
            
            // Create a new reference to avoid conflicts
            Form previousForm = current;
            
            next.FormClosed += (s, e) =>
            {
                // When the next form closes, show the previous form again
                if (previousForm != null && !previousForm.IsDisposed)
                {
                    previousForm.Show();
                }
            };
            
            next.Show();
        }

        public static void CloseAndReturnToPrevious(Form current, Form previous)
        {
            if (current != null && !current.IsDisposed)
            {
                current.Hide();
                current.Dispose();
            }

            if (previous != null && !previous.IsDisposed)
            {
                previous.Show();
                previous.BringToFront();
                previous.Focus();
            }
        }
    }
}