namespace Caro.Client.UI.Models
{
    public class PlayerViewModel
    {
        public string Username { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}