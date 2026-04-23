using System;
using System.Collections.Generic;
using Caro.Server.Core;
using Caro.Shared.Network;
using Caro.Shared.Models;
using Caro.Shared.Utils;

namespace Caro.Server.Services
{
    // Main service responsible for matchmaking and game coordination
    public class MatchmakingService
    {
        private ServerManager _server;
        private List<ClientHandler> _waitingPlayers = new List<ClientHandler>();
        private List<GameRoom> _rooms = new List<GameRoom>();
        private readonly object _roomsLock = new object();
        
        public MatchmakingService(ServerManager server)
        {
            _server = server;
        }

        // Central method: handles all incoming packets from clients
        public void HandlePacket(ClientHandler client, Packet packet)
        {
            if (packet == null) return;

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
                case CommandType.Surrender:
                case CommandType.TimerTick:
                case CommandType.TimeOut:
                    ForwardToRoom(client, packet);
                    break;

                case CommandType.Disconnect:
                    HandleDisconnect(client);
                    break;
            }
        }

        // RANDOM MATCHMAKING
        private void HandleFindMatch(ClientHandler client)
        {
            if (client.PlayerInfo.IsPlaying)
            {
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.LoginFailed, 
                    Data = "Bạn đang trong trận đấu, không thể tìm trận mới"
                });
                return;
            }

            Console.WriteLine($"{client.PlayerInfo.Name} is finding match...");

            if (_waitingPlayers.Contains(client))
                return;

            if (_waitingPlayers.Count > 0)
            {
                var opponent = _waitingPlayers[0];
                _waitingPlayers.RemoveAt(0);

                var room = new GameRoom(opponent, client, this);
                lock (_roomsLock)
                {
                    _rooms.Add(room);
                }

                // Cập nhật IsPlaying cho cả hai player
                opponent.PlayerInfo.IsPlaying = true;
                client.PlayerInfo.IsPlaying = true;

                Console.WriteLine($"Match found: {opponent.PlayerInfo.Name} vs {client.PlayerInfo.Name}");
                room.StartGame();
            }
            else
            {
                _waitingPlayers.Add(client);
                Console.WriteLine($"{client.PlayerInfo.Name} is waiting...");
            }
        }

        // DIRECT CHALLENGE
        private void HandleDirectChallenge(ClientHandler client, Packet packet)
        {
            string targetName = packet.Data;
            if (string.IsNullOrWhiteSpace(targetName))
            {
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.InvalidInput, 
                    Data = "Tên người chơi không hợp lệ"
                });
                return;
            }

            var targetClient = _server.GetClientByName(targetName);
            
            if (targetClient == null)
            {
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.LoginFailed, 
                    Data = $"Người chơi '{targetName}' không tồn tại"
                });
                return;
            }

            // Check if target is already in a game
            if (targetClient.PlayerInfo.IsPlaying)
            {
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.LoginFailed, 
                    Data = $"Người chơi '{targetName}' đang trong trận đấu, vui lòng thử lại sau"
                });
                return;
            }

            // Send challenge request to target
            targetClient.SendPacket(new Packet 
            { 
                Command = CommandType.Challenge, 
                Data = client.PlayerInfo.Name 
            });
            
            Console.WriteLine($"{client.PlayerInfo.Name} challenged {targetName}");
        }

        // ACCEPT CHALLENGE
        private void HandleChallengeAccept(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Data;
            var challenger = _server.GetClientByName(challengerName);
            
            if (challenger == null)
                return;

            var room = new GameRoom(challenger, client, this);
            lock (_roomsLock)
            {
                _rooms.Add(room);
            }

            // Update IsPlaying for both players
            challenger.PlayerInfo.IsPlaying = true;
            client.PlayerInfo.IsPlaying = true;

            room.StartGame();
        }

        // REJECT CHALLENGE
        private void HandleChallengeReject(ClientHandler client, Packet packet)
        {
            string challengerName = packet.Data;
            var challenger = _server.GetClientByName(challengerName);
            if (challenger != null)
            {
                challenger.SendPacket(new Packet 
                { 
                    Command = CommandType.Reject, 
                    Data = client.PlayerInfo.Name 
                });
            }
        }

        // FORWARD GAME DATA
        private void ForwardToRoom(ClientHandler client, Packet packet)
        {
            var room = FindRoomByClient(client);
            if (room != null)
            {
                room.HandlePacket(client, packet);
            }
        }

        // HANDLE DISCONNECT
        public void HandleDisconnect(ClientHandler client)
        {
            Console.WriteLine($"{client.PlayerInfo.Name} disconnected");
            _waitingPlayers.Remove(client);

            var room = FindRoomByClient(client);
            if (room != null)
            {
                var opponent = room.Player1 == client ? room.Player2 : room.Player1;

                // Notify opponent about disconnection
                opponent.SendPacket(new Packet
                {
                    Command = CommandType.OpponentDisconnected,
                    Data = client.PlayerInfo.Name
                });

                // Update IsPlaying for both players
                opponent.PlayerInfo.IsPlaying = false;
                client.PlayerInfo.IsPlaying = false;

                EndRoom(room);
            }

            // Update IsPlaying in case player was waiting for a match
            client.PlayerInfo.IsPlaying = false;
            _server.BroadcastPlayerList();
        }

        private GameRoom FindRoomByClient(ClientHandler client)
        {
            lock (_roomsLock)
            {
                return _rooms.Find(r => r.Player1 == client || r.Player2 == client);
            }
        }

        public void EndRoom(GameRoom room)
        {
            lock (_roomsLock)
            {
                _rooms.Remove(room);
            }
            _server.BroadcastPlayerList();
        }

        public void EndGame(GameRoom room)
        {
            EndRoom(room);
        }
    }
}


