using System;
using System.Collections.Generic;
using System.IO;

namespace Caro.Server.Storage
{
    public class MatchHistoryRepository
    {
        private const string FilePath = "match_history.txt";

        public void SaveMatch(string player1, string player2, string winner)
        {
            try
            {
                string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Match: {player1} vs {player2} -> Winner: {winner}";
                File.AppendAllLines(FilePath, new[] { log });
                Console.WriteLine("History saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        public List<string> GetHistory()
        {
            var history = new List<string>();
            try
            {
                if (File.Exists(FilePath))
                {
                    history.AddRange(File.ReadAllLines(FilePath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading history: {ex.Message}");
            }
            return history;
        }
    }
}
