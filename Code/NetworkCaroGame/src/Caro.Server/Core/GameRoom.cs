using System;
using Caro.Server.Services;
using Caro.Shared.Network;
using Caro.Shared.Models;
using Caro.Shared.Utils;
using Caro.Server.Storage;

namespace Caro.Server.Core
{
    public class GameRoom
    {
        public ClientHandler Player1 { get; }
        public ClientHandler Player2 { get; }
        private readonly MatchmakingService _matchmaking;
        private MatchHistoryRepository _historyRepo = new MatchHistoryRepository();
        private bool _isGameEnded = false;

        public GameRoom(ClientHandler p1, ClientHandler p2, MatchmakingService matchmaking)
        {
            Player1 = p1;
            Player2 = p2;
            _matchmaking = matchmaking;
        }

        public void StartGame()
        {
            Console.WriteLine($"Starting game between {Player1.PlayerInfo.Name} and {Player2.PlayerInfo.Name}");
            
            Player1.SendPacket(new Packet 
            { 
                Command = CommandType.StartGame, 
                Data = Player2.PlayerInfo.Name,
                Payload = "1"
            });
            
            Player2.SendPacket(new Packet 
            { 
                Command = CommandType.StartGame, 
                Data = Player1.PlayerInfo.Name,
                Payload = "2"
            });
        }

        public void HandlePacket(ClientHandler sender, Packet packet)
        {
            if (packet == null) return;

            ClientHandler opponent = sender == Player1 ? Player2 : Player1;

            switch (packet.Command)
            {
                case CommandType.Move:
                    opponent.SendPacket(packet);
                    break;

                case CommandType.TimerTick:
                case CommandType.TimeOut:
                case CommandType.Chat:
                    opponent.SendPacket(packet);
                    break;

                case CommandType.Surrender:
                    HandleSurrender(sender, opponent);
                    break;

                case CommandType.GameOver:
                    HandleGameOver(sender, opponent, packet);
                    break;

                case CommandType.MatchFinished:
                    // Kết quả hòa
                    HandleDraw(sender, opponent);
                    break;

                default:
                    opponent?.SendPacket(packet);
                    break;
            }
        }

        // Xử lý đầu hàng
        private void HandleSurrender(ClientHandler surrenderer, ClientHandler winner)
        {
            if (_isGameEnded) return;

            _isGameEnded = true;

            Console.WriteLine($"{surrenderer.PlayerInfo.Name} surrendered. {winner.PlayerInfo.Name} wins.");

            // Thông báo cho người thua
            surrenderer.SendPacket(new Packet
            {
                Command = CommandType.GameOver,
                Data = winner.PlayerInfo.Name
            });

            // Thông báo cho người thắng
            winner.SendPacket(new Packet
            {
                Command = CommandType.GameOver,
                Data = winner.PlayerInfo.Name
            });

            // Lưu kết quả
            _historyRepo.SaveMatch(Player1.PlayerInfo.Name, Player2.PlayerInfo.Name, winner.PlayerInfo.Name);

            // Cập nhật IsPlaying và kết thúc room
            Player1.PlayerInfo.IsPlaying = false;
            Player2.PlayerInfo.IsPlaying = false;
            _matchmaking.EndRoom(this);
        }

        // Xử lý kết quả thắng/thua
        private void HandleGameOver(ClientHandler sender, ClientHandler opponent, Packet packet)
        {
            if (_isGameEnded) return;

            _isGameEnded = true;

            string winner = Serializer.Deserialize<string>(packet.Data) ?? sender.PlayerInfo.Name;

            Console.WriteLine($"Game Over: {winner} won.");

            // Gửi kết quả cho đối thủ
            opponent.SendPacket(packet);

            // Lưu lịch sử
            _historyRepo.SaveMatch(Player1.PlayerInfo.Name, Player2.PlayerInfo.Name, winner);

            // Cập nhật IsPlaying
            Player1.PlayerInfo.IsPlaying = false;
            Player2.PlayerInfo.IsPlaying = false;
            _matchmaking.EndRoom(this);
        }

        // Xử lý kết quả hòa
        private void HandleDraw(ClientHandler sender, ClientHandler opponent)
        {
            if (_isGameEnded) return;

            _isGameEnded = true;

            Console.WriteLine($"Game Draw: {Player1.PlayerInfo.Name} vs {Player2.PlayerInfo.Name}");

            // Thông báo hòa cho cả hai người chơi
            var drawPacket = new Packet
            {
                Command = CommandType.MatchFinished,
                Data = "Draw"
            };

            sender.SendPacket(drawPacket);
            opponent.SendPacket(drawPacket);

            // Lưu kết quả hòa
            _historyRepo.SaveMatch(Player1.PlayerInfo.Name, Player2.PlayerInfo.Name, "Draw");

            // Cập nhật IsPlaying
            Player1.PlayerInfo.IsPlaying = false;
            Player2.PlayerInfo.IsPlaying = false;
            _matchmaking.EndRoom(this);
        }
    }
}