using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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
        private readonly object _clientsLock = new object();
        private readonly MatchmakingService _matchmakingService;
        private readonly Caro.Server.Storage.MatchHistoryRepository _historyRepo;

        public ServerManager()
        {
            _matchmakingService = new MatchmakingService(this);
            _historyRepo = new Caro.Server.Storage.MatchHistoryRepository();
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
                
                lock (_clientsLock)
                {
                    _clients.Add(handler);
                }
                
                handler.Start();
                Console.WriteLine($"New client connected. Total: {_clients.Count}");
            }
        }

        private void Handler_OnPacketReceived(ClientHandler client, Packet packet)
        {
            // Kiểm tra packet null
            if (packet == null)
            {
                Console.WriteLine($"[Server] Received null packet from {client.PlayerInfo.Name}");
                return;
            }

            switch (packet.Command)
            {
                case CommandType.Login:
                    HandleLogin(client, packet);
                    break;

                case CommandType.GetPlayers:
                    SendPlayerList(client);
                    break;
                    
                case CommandType.GetHistory:
                    var history = _historyRepo.GetHistory();
                    client.SendPacket(new Packet { Command = CommandType.HistoryResponse, Payload = Serializer.Serialize(history) });
                    break;
                    
                default:
                    _matchmakingService.HandlePacket(client, packet);
                    break;
            }
        }

        // Xử lý login với kiểm tra duplicate username
        private void HandleLogin(ClientHandler client, Packet packet)
        {
            try
            {
                string playerName = Serializer.Deserialize<string>(packet.Payload);

                // Validation dữ liệu
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    client.SendPacket(new Packet 
                    { 
                        Command = CommandType.LoginFailed, 
                        Data = "Username không được để trống"
                    });
                    return;
                }

                // Kiểm tra độ dài username
                if (playerName.Length > GameConstants.MAX_USERNAME_LENGTH)
                {
                    client.SendPacket(new Packet 
                    { 
                        Command = CommandType.LoginFailed, 
                        Data = $"Username tối đa {GameConstants.MAX_USERNAME_LENGTH} ký tự"
                    });
                    return;
                }

                // Kiểm tra ký tự đặc biệt
                if (!Regex.IsMatch(playerName, GameConstants.USERNAME_PATTERN))
                {
                    client.SendPacket(new Packet 
                    { 
                        Command = CommandType.LoginFailed, 
                        Data = "Username chỉ chứa chữ, số, underscore (3-20 ký tự)"
                    });
                    return;
                }

                // Kiểm tra duplicate username (đang online)
                lock (_clientsLock)
                {
                    if (_clients.Any(c => c.PlayerInfo.Name == playerName && c != client))
                    {
                        client.SendPacket(new Packet 
                        { 
                            Command = CommandType.DuplicateUsername, 
                            Data = $"Username '{playerName}' đã tồn tại"
                        });
                        return;
                    }
                }

                client.PlayerInfo.Name = playerName;
                client.PlayerInfo.IsPlaying = false;
                
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.LoginSuccess, 
                    Payload = Serializer.Serialize(client.PlayerInfo) 
                });
                
                BroadcastPlayerList();
                Console.WriteLine($"Player {playerName} logged in successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Login error: {ex.Message}");
                client.SendPacket(new Packet 
                { 
                    Command = CommandType.InvalidInput, 
                    Data = "Lỗi xử lý dữ liệu đăng nhập"
                });
            }
        }

        private void Handler_OnDisconnected(ClientHandler client)
        {
            lock (_clientsLock)
            {
                _clients.Remove(client);
            }
            _matchmakingService.HandleDisconnect(client);
            BroadcastPlayerList();
            Console.WriteLine($"Client {client.PlayerInfo.Name ?? "Unknown"} disconnected.");
        }

        public ClientHandler GetClientByName(string name)
        {
            lock (_clientsLock)
            {
                return _clients.FirstOrDefault(c => c.PlayerInfo.Name == name);
            }
        }

        public void BroadcastPlayerList()
        {
            List<PlayerInfo> players;
            lock (_clientsLock)
            {
                players = _clients.Select(c => c.PlayerInfo).ToList();
            }
            
            var packet = new Packet
            {
                Command = CommandType.UpdatePlayerList,
                Payload = Serializer.Serialize(players)
            };
            Broadcast(packet);
        }

        public void SendPlayerList(ClientHandler client)
        {
            List<PlayerInfo> players;
            lock (_clientsLock)
            {
                players = _clients.Select(c => c.PlayerInfo).ToList();
            }
            
            client.SendPacket(new Packet
            {
                Command = CommandType.UpdatePlayerList,
                Payload = Serializer.Serialize(players)
            });
        }

        public void Broadcast(Packet packet)
        {
            List<ClientHandler> currentClients;
            lock (_clientsLock)
            {
                currentClients = _clients.ToList();
            }

            foreach (var client in currentClients)
            {
                client.SendPacket(packet);
            }
        }

        public ClientHandler GetClientById(string id)
        {
            lock (_clientsLock)
            {
                return _clients.FirstOrDefault(c => c.PlayerInfo.Id == id);
            }
        }

        public string GetClientNameById(string id)
        {
            lock (_clientsLock)
            {
                var client = _clients.FirstOrDefault(c => c.PlayerInfo.Id == id);
                return client?.PlayerInfo.Name ?? "Unknown";
            }
        }

        public string GetOpponentName(ClientHandler player1, ClientHandler player2, ClientHandler currentClient)
        {
            if (currentClient.PlayerInfo.Id == player1.PlayerInfo.Id)
                return player2.PlayerInfo.Name;
            else
                return player1.PlayerInfo.Name;
        }
    }
}
