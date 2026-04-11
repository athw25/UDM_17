using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caro.Client.Network;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.Services
{
    /// <summary>
    /// High-level client-side service that wraps a ClientSocket and exposes events / helpers
    /// used by the UI forms (Login, Lobby, Game, History).
    /// </summary>
    public class GameClientService
    {
        private static GameClientService _instance;
        public static GameClientService Instance => _instance ??= new GameClientService();

        private ClientSocket? _socket;

        // Events expected by UI
        public event Action<Packet>? OnLoginSuccess;                       // raw packet (keeps compatibility)
        public event Action<List<PlayerInfo>>? OnUpdatePlayerList;         // when server sends detailed player info
        public event Action<List<string>>? OnPlayerList;                  // when server sends simple player name list
        public event Action<ChallengeInfo>? OnChallengeRequest;
        public event Action<ChallengeInfo>? OnChallengeResponse;
        public event Action<string>? OnStartGame;                         // payload/data: who goes first or opponent info
        public event Action<MoveInfo>? OnMoveReceived;
        public event Action<List<string>>? OnHistoryResponse;

        private GameClientService() { }

        // ========== CONNECTION / LIFECYCLE ==========

        public bool IsConnected => _socket?.IsConnected ?? false;

        public async Task ConnectAsync(string ip, int port)
        {
            _socket = new ClientSocket();
            _socket.OnReceive += HandlePacket;
            await _socket.ConnectAsync(ip, port);
        }

        public void Disconnect()
        {
            try
            {
                _socket?.Disconnect();
            }
            catch { }
            finally
            {
                if (_socket != null)
                    _socket.OnReceive -= HandlePacket;
                _socket = null;
            }
        }

        // Expose raw socket for advanced scenarios (forms currently use ClientSocket directly in the project)
        public ClientSocket? Socket => _socket;

        // ========== REQUESTS / ACTIONS ==========

        public void Login(string username)
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            _socket.Login(username);
        }

        public void GetPlayers()
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            // LobbyForm expects CommandType.PlayerList with Data = ""
            _socket.Send(new Packet { Command = CommandType.PlayerList, Data = "" });
        }

        public void SendChallenge(string targetNameOrId)
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            _socket.Challenge(targetNameOrId);
        }

        public void ReplyChallenge(string target, bool accept)
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            _socket.Send(new Packet
            {
                Command = accept ? CommandType.Accept : CommandType.Reject,
                Data = target
            });
        }

        public void SendMove(MoveInfo move)
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            _socket.SendMove(move);
        }

        public void RequestHistory(string username)
        {
            if (_socket == null) throw new InvalidOperationException("Not connected");
            // Use the simple Request pattern; UI subscribes to OnHistoryResponse.
            _socket.Send(new Packet { Command = CommandType.GetHistory, Data = username });
        }

        // ========== INTERNAL: PACKET HANDLING ==========

        private void HandlePacket(Packet packet)
        {
            try
            {
                // Normalize payload/data content (server / socket implementations use either field)
                string content = packet.Payload ?? packet.Data ?? string.Empty;

                switch (packet.Command)
                {
                    case CommandType.LoginSuccess:
                        // Server may send PlayerInfo or simple acknowledgement; forward raw packet and try to parse PlayerInfo
                        OnLoginSuccess?.Invoke(packet);
                        break;

                    case CommandType.UpdatePlayerList:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var players = Serializer.Deserialize<List<PlayerInfo>>(content);
                                OnUpdatePlayerList?.Invoke(players);
                            }
                            catch
                            {
                                // ignore parsing errors for this command
                            }
                        }
                        break;

                    case CommandType.PlayerList:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var names = Serializer.Deserialize<List<string>>(content);
                                OnPlayerList?.Invoke(names);
                            }
                            catch
                            {
                                // ignore parse errors
                            }
                        }
                        break;

                    case CommandType.Challenge:
                    case CommandType.ChallengeRequest:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var req = Serializer.Deserialize<ChallengeInfo>(content);
                                OnChallengeRequest?.Invoke(req);
                            }
                            catch
                            {
                                // fallback: treat content as challenger name
                                OnChallengeRequest?.Invoke(new ChallengeInfo { ChallengerId = content, TargetId = string.Empty, IsAccepted = false });
                            }
                        }
                        break;

                    case CommandType.ChallengeResponse:
                    case CommandType.Accept:
                    case CommandType.Reject:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var resp = Serializer.Deserialize<ChallengeInfo>(content);
                                OnChallengeResponse?.Invoke(resp);
                            }
                            catch
                            {
                                // fallback: build a minimal response object
                                OnChallengeResponse?.Invoke(new ChallengeInfo { ChallengerId = content, TargetId = string.Empty, IsAccepted = packet.Command == CommandType.Accept });
                            }
                        }
                        break;

                    case CommandType.StartGame:
                        // payload could be "1"/"2" or opponent name — forward raw
                        OnStartGame?.Invoke(content);
                        break;

                    case CommandType.Move:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var move = Serializer.Deserialize<MoveInfo>(content);
                                OnMoveReceived?.Invoke(move);
                            }
                            catch
                            {
                                // ignore parse errors
                            }
                        }
                        break;

                    case CommandType.HistoryResponse:
                    case CommandType.HistoryData:
                        if (!string.IsNullOrEmpty(content))
                        {
                            try
                            {
                                var history = Serializer.Deserialize<List<string>>(content);
                                OnHistoryResponse?.Invoke(history);
                            }
                            catch
                            {
                                // ignore parse problems
                            }
                        }
                        break;

                    default:
                        // Unknown/other commands can be handled by consumers through OnLoginSuccess (raw packet) or by direct socket subscription
                        break;
                }
            }
            catch (Exception)
            {
                // swallow to avoid bringing down socket loop; UI may subscribe to socket.OnReceive for raw packets if needed
            }
        }
    }
}