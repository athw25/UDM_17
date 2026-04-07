using carogame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroServer.Services
{
    public class MatchmakingService
    {
        private List<Player> onlinePlayers = new List<Player>();
        private List<GameRoom> rooms = new List<GameRoom>();

        public void BroadcastLobby()
        {
            foreach (var player in onlinePlayers)
            {
                player.Send?.Invoke(new
                {
                    type = "LOBBY",
                    players = onlinePlayers.ConvertAll(p => p.Username)
                });
            }
        }
        public void SendChallenge(string fromId, string toId)
        {
            var from = onlinePlayers.Find(p => p.ConnectionId == fromId);
            var to = onlinePlayers.Find(p => p.ConnectionId == toId);

            if (from == null || to == null) return;

            to.Send?.Invoke(new
            {
                type = "CHALLENGE",
                from = from.Username
            });
        }
        public void AcceptChallenge(string fromId, string toId)
        {
            CreateRoom(fromId, toId);
        }

        public void RejectChallenge(string fromId)
        {
            var player = onlinePlayers.Find(p => p.ConnectionId == fromId);
            player?.Send?.Invoke("Rejected");
        }
        private void CreateRoom(string idA, string idB)
        {
            var a = onlinePlayers.Find(p => p.ConnectionId == idA);
            var b = onlinePlayers.Find(p => p.ConnectionId == idB);

            if (a == null || b == null) return;
            if (a.IsPlaying || b.IsPlaying) return;

            var room = new GameRoom(a, b);
            rooms.Add(room);

            a.IsPlaying = true;
            b.IsPlaying = true;

            a.Send?.Invoke(new { type = "START", roomId = room.RoomId });
            b.Send?.Invoke(new { type = "START", roomId = room.RoomId });
        }
    }
}