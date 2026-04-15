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

        private List<ClientHandler> _waitingPlayers = new List<ClientHandler>();

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

        private void HandleFindMatch(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} is finding match...");

            if (_waitingPlayers.Contains(client))
                return;

            if (_waitingPlayers.Count > 0)
            {
                var opponent = _waitingPlayers[0];
                _waitingPlayers.RemoveAt(0);

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
            _waitingPlayers.Remove(client);

            var room = FindRoomByClient(client);
            if (room != null)
            {
                var opponent = room.Player1 == client ? room.Player2 : room.Player1;

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


