using System;
using System.Collections.Generic;
using System.IO;
using Caro.Shared.Models;

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

        public List<MatchHistory> GetHistory()
        {
            var history = new List<MatchHistory>();
            try
            {
                if (File.Exists(FilePath))
                {
                    var lines = File.ReadAllLines(FilePath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            // Format: [YYYY-MM-DD HH:mm:ss] Match: P1 vs P2 -> Winner: Win
                            var timePart = line.Substring(1, 19);
                            var date = DateTime.Parse(timePart);

                            var rest = line.Substring(22); 
                            rest = rest.Replace("Match: ", ""); 
                            var parts = rest.Split(new string[] { " -> Winner: " }, StringSplitOptions.None);
                            var players = parts[0].Split(new string[] { " vs " }, StringSplitOptions.None);

                            history.Add(new MatchHistory
                            {
                                Time = date,
                                Player1 = players[0].Trim(),
                                Player2 = players[1].Trim(),
                                Winner = parts[1].Trim()
                            });
                        }
                        catch { } // Ignore lines with corrupted format
                    }
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
