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
                Data = Player2.PlayerInfo.Name,      // name of opponent for Player1
                Payload = "1"                         // 1 = Player1 goes first
            });
            
            Player2.SendPacket(new Packet 
            { 
                Command = CommandType.StartGame, 
                Data = Player1.PlayerInfo.Name,      // name of opponent for Player2
                Payload = "2"                         // 2 =
            });
        }

        public void HandlePacket(ClientHandler sender, Packet packet)
        {
            ClientHandler opponent = sender == Player1 ? Player2 : Player1;

            switch (packet.Command)
            {
                case CommandType.Move:
                    // Broadcast the move to the opponent
                    opponent.SendPacket(packet);
                    break;
                case CommandType.TimerTick:
                case CommandType.TimeOut:
                    opponent.SendPacket(packet);
                    break;
                case CommandType.GameOver:
                    string winner = Serializer.Deserialize<string>(packet.Payload);
                    opponent.SendPacket(packet); // send game over to the other guy too
                    Console.WriteLine($"Game Over: {winner} won.");
                    _historyRepo.SaveMatch(Player1.PlayerInfo.Name, Player2.PlayerInfo.Name, winner);
                    _matchmaking.EndGame(this);
                    break;
                case CommandType.Chat:
                    opponent.SendPacket(packet);
                    break;
            }
        }
    }
}