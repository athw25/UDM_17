namespace Caro.Client.UI.Models
{
    public class MatchHistoryViewModel
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Result { get; set; }

        public override string ToString()
        {
            return $"{Player1} vs {Player2} - {Result}";
        }
    }
}