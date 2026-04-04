using System;
using System.Collections.Generic;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;
using Caro.Client.Network;

namespace Caro.Client.Services
{
    public class GameClientService
    {
        private static GameClientService _instance;
        public static GameClientService Instance => _instance ??= new GameClientService();

        public event Action<Packet> OnLoginSuccess;
        public event Action<List<PlayerInfo>> OnUpdatePlayerList;
        public event Action<ChallengeInfo> OnChallengeRequest;
        public event Action<ChallengeInfo> OnChallengeResponse;
        public event Action<string> OnStartGame;
        public event Action<List<string>> OnHistoryResponse;

        private GameClientService()
        {
            ClientSocket.Instance.OnPacketReceived += HandlePacket;
        }

        public bool Connect(string ip, int port)
        {
            return ClientSocket.Instance.Connect(ip, port);
        }

        public void Login(string name)
        {
            ClientSocket.Instance.Send(new Packet { Command = CommandType.Login, Payload = Serializer.Serialize(name) });
        }

        public void GetPlayers()
        {
            ClientSocket.Instance.Send(new Packet { Command = CommandType.GetPlayers });
        }

        public void SendChallenge(string targetId)
        {
            ClientSocket.Instance.Send(new Packet { Command = CommandType.ChallengeRequest, Payload = Serializer.Serialize(targetId) });
        }

        public void ReplyChallenge(ChallengeInfo req, bool accept)
        {
            req.IsAccepted = accept;
            ClientSocket.Instance.Send(new Packet { Command = CommandType.ChallengeResponse, Payload = Serializer.Serialize(req) });
        }

        public void RequestHistory()
        {
            ClientSocket.Instance.Send(new Packet { Command = CommandType.GetHistory });
        }

        private void HandlePacket(Packet packet)
        {
            switch (packet.Command)
            {
                case CommandType.LoginSuccess:
                    OnLoginSuccess?.Invoke(packet);
                    break;
                case CommandType.UpdatePlayerList:
                    var players = Serializer.Deserialize<List<PlayerInfo>>(packet.Payload);
                    OnUpdatePlayerList?.Invoke(players);
                    break;
                case CommandType.ChallengeRequest:
                    var req = Serializer.Deserialize<ChallengeInfo>(packet.Payload);
                    OnChallengeRequest?.Invoke(req);
                    break;
                case CommandType.ChallengeResponse:
                    var res = Serializer.Deserialize<ChallengeInfo>(packet.Payload);
                    OnChallengeResponse?.Invoke(res);
                    break;
                case CommandType.StartGame:
                    var turn = Serializer.Deserialize<string>(packet.Payload);
                    OnStartGame?.Invoke(turn);
                    break;
                case CommandType.HistoryResponse:
                    var history = Serializer.Deserialize<List<string>>(packet.Payload);
                    OnHistoryResponse?.Invoke(history);
                    break;
            }
        }
    }
}