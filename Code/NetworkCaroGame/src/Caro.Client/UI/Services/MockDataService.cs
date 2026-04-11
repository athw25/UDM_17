using System.Collections.Generic;
using Caro.Client.UI.Models;

namespace Caro.Client.UI.Services
{
    public static class MockDataService
    {
        public static List<PlayerViewModel> GetPlayers()
        {
            return new List<PlayerViewModel>
            {
                new PlayerViewModel{ Username = "Alice"},
                new PlayerViewModel{ Username = "Bob"},
                new PlayerViewModel{ Username = "Charlie"}
            };
        }

        public static List<MatchHistoryViewModel> GetHistory()
        {
            return new List<MatchHistoryViewModel>
            {
                new MatchHistoryViewModel{ Player1="Alice", Player2="Bob", Result="Win"},
                new MatchHistoryViewModel{ Player1="Charlie", Player2="Alice", Result="Lose"}
            };
        }
    }
}