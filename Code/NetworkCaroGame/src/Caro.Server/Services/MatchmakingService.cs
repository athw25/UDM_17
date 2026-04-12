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

        // Danh sÃ¡ch ngÆ°á»i chÆ¡i Ä‘ang chá» ghÃ©p
        private List<ClientHandler> _waitingPlayers = new List<ClientHandler>();

        // Danh sÃ¡ch phÃ²ng Ä‘ang chÆ¡i
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
                case CommandType.Challenge:
                    HandleDirectChallenge(client, packet);
                    break;
                case CommandType.Accept:
                    HandleChallengeAccept(client, packet);
                    break;
                case CommandType.Reject:
                    HandleChallengeReject(client, packet);
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
                // Láº¥y ngÆ°á»i chÆ¡i Ä‘áº§u tiÃªn trong hÃ ng Ä‘á»£i
                var opponent = _waitingPlayers[0];
                _waitingPlayers.RemoveAt(0);

                // Táº¡o phÃ²ng
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

                private void HandleDirectChallenge(ClientHandler client, Packet packet)
        {
            string targetName = packet.Payload;
            var targetClient = _server.GetClientByName(targetName);
            if (targetClient != null)
            {
                targetClient.SendPacket(new Packet { Command = CommandType.Challenge, Payload = client.PlayerInfo.Name });
            }
        }
        private void HandleChallengeAccept(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Payload;
            var challenger = _server.GetClientByName(challengerName);
            if (challenger != null)
            {
                var room = new GameRoom(challenger, client, this);
                _rooms.Add(room);
                room.StartGame();
            }
        }
        private void HandleChallengeReject(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Payload;
            var challenger = _server.GetClientByName(challengerName);
            if (challenger != null)
            {
                challenger.SendPacket(new Packet { Command = CommandType.Reject, Payload = client.PlayerInfo.Name });
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

            // Náº¿u Ä‘ang chá» thÃ¬ xÃ³a khá»i hÃ ng Ä‘á»£i
            _waitingPlayers.Remove(client);

            var room = FindRoomByClient(client);
            if (room != null)
            {
                var opponent = room.Player1 == client ? room.Player2 : room.Player1;

                // bÃ¡o cho Ä‘á»‘i thá»§
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


