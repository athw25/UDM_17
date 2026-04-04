using System;
using System.Collections.Generic;
using System.Linq;
using Caro.Server.Core;
using Caro.Shared.Network;
using Caro.Shared.Models;
using Caro.Shared.Utils;

namespace Caro.Server.Services
{
    public class MatchmakingService
    {
        private readonly ServerManager _serverManager;
        private readonly List<GameRoom> _activeRooms = new List<GameRoom>();

        public MatchmakingService(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        public void HandlePacket(ClientHandler sender, Packet packet)
        {
            switch (packet.Command)
            {
                case CommandType.ChallengeRequest:
                    string targetId = Serializer.Deserialize<string>(packet.Payload);
                    var targetClient = _serverManager.GetClientById(targetId);
                    if (targetClient != null && !targetClient.PlayerInfo.IsPlaying)
                    {
                        var info = new ChallengeInfo { ChallengerId = sender.PlayerInfo.Id, TargetId = targetId };
                        targetClient.SendPacket(new Packet { Command = CommandType.ChallengeRequest, Payload = Serializer.Serialize(info) });
                    }
                    break;

                case CommandType.ChallengeResponse:
                    var responseBody = Serializer.Deserialize<ChallengeInfo>(packet.Payload);
                    var challengerClient = _serverManager.GetClientById(responseBody.ChallengerId);
                    
                    if (challengerClient != null)
                    {
                        if (responseBody.IsAccepted)
                        {
                            sender.PlayerInfo.IsPlaying = true;
                            challengerClient.PlayerInfo.IsPlaying = true;
                            _serverManager.BroadcastPlayerList();

                            var room = new GameRoom(challengerClient, sender, this);
                            _activeRooms.Add(room);
                            room.StartGame();
                        }
                        else
                        {
                            challengerClient.SendPacket(new Packet { Command = CommandType.ChallengeResponse, Payload = Serializer.Serialize(responseBody) });
                        }
                    }
                    break;
                    
                default:
                    // Pass to appropriate GameRoom if the player is in game
                    var activeRoom = _activeRooms.FirstOrDefault(r => r.Player1 == sender || r.Player2 == sender);
                    if (activeRoom != null)
                    {
                        activeRoom.HandlePacket(sender, packet);
                    }
                    else
                    {
                        Console.WriteLine($"Unhandled packet from {sender.PlayerInfo.Name}: {packet.Command}");
                    }
                    break;
            }
        }

        public void EndGame(GameRoom room)
        {
            room.Player1.PlayerInfo.IsPlaying = false;
            room.Player2.PlayerInfo.IsPlaying = false;
            _activeRooms.Remove(room);
            _serverManager.BroadcastPlayerList();
        }

        public void HandleDisconnect(ClientHandler client)
        {
            var room = _activeRooms.FirstOrDefault(r => r.Player1 == client || r.Player2 == client);
            if (room != null)
            {
                var otherClient = room.Player1 == client ? room.Player2 : room.Player1;
                otherClient.SendPacket(new Packet { Command = CommandType.PlayerDisconnected, Payload = "Opponent disconnected" });
                EndGame(room);
            }
        }
    }
}