using carogame.Models;
using carogame.Services;
using System;

class Program
{
    static void Main()
    {
        var matchmaking = new MatchmakingService();

        var p1 = new Player
        {
            Username = "A",
            ConnectionId = "1",
            Send = data => Console.WriteLine("A nhận: " + data)
        };

        var p2 = new Player
        {
            Username = "B",
            ConnectionId = "2",
            Send = data => Console.WriteLine("B nhận: " + data)
        };

        matchmaking.AddPlayer(p1);
        matchmaking.AddPlayer(p2);

        matchmaking.SendChallenge("1", "2");
        matchmaking.AcceptChallenge("1", "2");
    }
}