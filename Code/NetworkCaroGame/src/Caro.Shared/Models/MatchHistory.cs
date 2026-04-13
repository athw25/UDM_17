using System;

namespace Caro.Shared.Models
{
    public class MatchHistory
    {
        public DateTime Time { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Winner { get; set; }
        public string Moves { get; set; }
    }
}
