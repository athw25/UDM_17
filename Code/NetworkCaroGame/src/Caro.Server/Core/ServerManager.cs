using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Caro.Shared.Network;
using Caro.Shared.Models;
using Caro.Shared.Utils;
using Caro.Server.Services;

namespace Caro.Server.Core
{
    public class ServerManager
    {
        private TcpListener _listener;
        private readonly List<ClientHandler> _clients = new List<ClientHandler>();
        private readonly MatchmakingService _matchmakingService;

        public ServerManager()
        {
            _matchmakingService = new MatchmakingService(this);
        }

        public void Start(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"Server started on port {port}. Waiting for clients...");

            Task.Run(AcceptClientsLoop);
        }

        private async Task AcceptClientsLoop()
        {
            while (true)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                var handler = new ClientHandler(tcpClient);
                
                handler.OnPacketReceived += Handler_OnPacketReceived;
                handler.OnDisconnected += Handler_OnDisconnected;
                
                _clients.Add(handler);
                handler.Start();
                Console.WriteLine($"New client connected. Total: {_clients.Count}");
            }
        }

        private void Handler_OnPacketReceived(ClientHandler client, Packet packet)
        {
            switch (packet.Command)
            {
                case CommandType.Login:
                    string playerName = Serializer.Deserialize<string>(packet.Payload);
                    client.PlayerInfo.Name = playerName;
                    client.SendPacket(new Packet { Command = CommandType.LoginSuccess, Payload = Serializer.Serialize(client.PlayerInfo) });
                    BroadcastPlayerList();
                    Console.WriteLine($"Player {playerName} logged in.");
                    break;

                case CommandType.GetPlayers:
                    SendPlayerList(client);
                    break;
                    
                default:
                    _matchmakingService.HandlePacket(client, packet);
                    break;
            }
        }

        private void Handler_OnDisconnected(ClientHandler client)
        {
            _clients.Remove(client);
            _matchmakingService.HandleDisconnect(client);
            BroadcastPlayerList();
            Console.WriteLine($"Client {client.PlayerInfo.Name ?? "Unknown"} disconnected. Total: {_clients.Count}");
        }

        public void BroadcastPlayerList()
        {
            var players = _clients.Select(c => c.PlayerInfo).ToList();
            var packet = new Packet
            {
                Command = CommandType.UpdatePlayerList,
                Payload = Serializer.Serialize(players)
            };
            Broadcast(packet);
        }

        public void SendPlayerList(ClientHandler client)
        {
            var players = _clients.Select(c => c.PlayerInfo).ToList();
            client.SendPacket(new Packet
            {
                Command = CommandType.UpdatePlayerList,
                Payload = Serializer.Serialize(players)
            });
        }

        public void Broadcast(Packet packet)
        {
            foreach (var client in _clients.ToList())
            {
                client.SendPacket(packet);
            }
        }

        public ClientHandler GetClientById(string id)
        {
            return _clients.FirstOrDefault(c => c.PlayerInfo.Id == id);
        }
    }
}