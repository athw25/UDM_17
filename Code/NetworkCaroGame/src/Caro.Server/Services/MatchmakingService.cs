using System;
using System.Collections.Generic;
using Caro.Server.Core;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Server.Services
{
    public class MatchmakingService
    {
        private ServerManager _server;

        // Danh sách người chơi đang chờ ghép
        private List<ClientHandler> _waitingPlayers = new List<ClientHandler>();

        // Danh sách phòng đang chơi
        private List<GameRoom> _rooms = new List<GameRoom>();

        public MatchmakingService(ServerManager server)
        {
            _server = server;
        }

        public void HandlePacket(ClientHandler client, Packet packet)
        {
            switch (packet.Command)
            {
                case CommandType.ChallengeRequest:
                    HandleFindMatch(client);
                    break;

                case CommandType.Move:
                case CommandType.Chat:
                case CommandType.GameOver:
                case CommandType.TimerTick:
                case CommandType.TimeOut:
                    ForwardToRoom(client, packet);
                    break;
            }
        }

        // MATCHMAKING
        private void HandleFindMatch(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} is finding match...");

            if (_waitingPlayers.Contains(client))
                return;

            if (_waitingPlayers.Count > 0)
            {
                // Lấy người chơi đầu tiên trong hàng đợi
                var opponent = _waitingPlayers[0];
                _waitingPlayers.RemoveAt(0);

                // Tạo phòng
                var room = new GameRoom(opponent, client, this);
                _rooms.Add(room);

                Console.WriteLine($"Match found: {opponent.PlayerInfo.Name} vs {client.PlayerInfo.Name}");

                room.StartGame();
            }
            else
            {
                _waitingPlayers.Add(client);
                Console.WriteLine($"{client.PlayerInfo.Name} is waiting...");
            }
        }

        // FORWARD PACKET
        private void ForwardToRoom(ClientHandler client, Packet packet)
        {
            var room = FindRoomByClient(client);
            if (room != null)
            {
                room.HandlePacket(client, packet);
            }
        }

        public void HandleDisconnect(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} disconnected");

            // Nếu đang chờ thì xóa khỏi hàng đợi
            _waitingPlayers.Remove(client);

            var room = FindRoomByClient(client);
            if (room != null)
            {
                var opponent = room.Player1 == client ? room.Player2 : room.Player1;

                // báo cho đối thủ
                opponent.SendPacket(new Packet
                {
                    Command = CommandType.PlayerDisconnected,
                    Payload = ""
                });

                _rooms.Remove(room);
            }
        }

        public void EndGame(GameRoom room)
        {
            Console.WriteLine("Ending game...");

            if (_rooms.Contains(room))
            {
                _rooms.Remove(room);
            }
        }

        private GameRoom FindRoomByClient(ClientHandler client)
        {
            foreach (var room in _rooms)
            {
                if (room.Player1 == client || room.Player2 == client)
                    return room;
            }
            return null;
        }
    }
}
