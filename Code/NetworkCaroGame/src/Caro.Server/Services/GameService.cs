using System;
using System.Collections.Generic;
using Caro.Server.Core;
using Caro.Server.Storage;
using Caro.Shared.Game;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Server.Services
{
    // Centralized service that manages active game rooms, boards and move validation.
    // MatchmakingService or other server components can use this to create games and forward packets.
    public class GameService
    {
        private readonly List<GameRoom> _rooms = new List<GameRoom>();
        private readonly Dictionary<string, GameRoom> _playerRoomMap = new Dictionary<string, GameRoom>();
        private readonly Dictionary<GameRoom, BoardController> _boards = new Dictionary<GameRoom, BoardController>();
        private readonly Dictionary<GameRoom, MatchmakingService> _roomMatchmaking = new Dictionary<GameRoom, MatchmakingService>();
        private readonly MatchHistoryRepository _historyRepo = new MatchHistoryRepository();

        private static GameService _instance;
        public static GameService Instance => _instance ??= new GameService();

        private GameService() { }

        // Create and register a new game room with an associated board.
        public GameRoom CreateRoom(ClientHandler p1, ClientHandler p2, MatchmakingService matchmaking, int boardSize = 20)
        {
            if (p1 == null || p2 == null || matchmaking == null)
                throw new ArgumentNullException();

            var room = new GameRoom(p1, p2, matchmaking);

            _rooms.Add(room);
            _boards[room] = new BoardController(boardSize);
            _roomMatchmaking[room] = matchmaking;
            _playerRoomMap[p1.PlayerInfo.Id] = room;
            _playerRoomMap[p2.PlayerInfo.Id] = room;

            // Start the game (GameRoom.StartGame will tell clients who goes first)
            room.StartGame();

            Console.WriteLine($"GameService: Created room for {p1.PlayerInfo.Name} vs {p2.PlayerInfo.Name}");
            return room;
        }

        // Called by MatchmakingService when a packet from a client that may belong to a game arrives.
        public void HandlePacket(ClientHandler sender, Packet packet)
        {
            if (sender == null || packet == null) return;

            if (!_playerRoomMap.TryGetValue(sender.PlayerInfo.Id, out var room))
            {
                // Not in a game known to GameService; ignore or log.
                return;
            }

            var opponent = sender == room.Player1 ? room.Player2 : room.Player1;

            switch (packet.Command)
            {
                case CommandType.Move:
                    HandleMove(room, sender, opponent, packet);
                    break;
                case CommandType.TimerTick:
                case CommandType.TimeOut:
                case CommandType.Chat:
                case CommandType.PlayerDisconnected:
                    // Forward these packets to opponent
                    opponent?.SendPacket(packet);
                    break;
                case CommandType.GameOver:
                    // If client reports game over, forward and cleanup
                    opponent?.SendPacket(packet);
                    FinishGame(room, sender.PlayerInfo.Name);
                    break;
                default:
                    // unknown in-game command — forward to opponent by default
                    opponent?.SendPacket(packet);
                    break;
            }
        }

        // End a game and cleanup data structures. Can be called by MatchmakingService.
        public void EndGame(GameRoom room)
        {
            if (room == null) return;
            RemoveRoom(room);
        }

        private void HandleMove(GameRoom room, ClientHandler sender, ClientHandler opponent, Packet packet)
        {
            try
            {
                var move = Serializer.Deserialize<MoveInfo>(packet.Payload);
                if (move == null)
                    return;

                if (!_boards.TryGetValue(room, out var board))
                    return;

                // Determine player number (1 = Player1, 2 = Player2)
                int playerNumber = sender == room.Player1 ? 1 : 2;

                // Validate and make move
                if (!board.IsValidMove(move.X, move.Y))
                {
                    // Invalid move: optionally send some error back to sender (not defined in protocol)
                    Console.WriteLine($"Invalid move from {sender.PlayerInfo.Name} at ({move.X},{move.Y})");
                    return;
                }

                var ok = board.MakeMove(move.X, move.Y, playerNumber);
                if (!ok) return;

                opponent?.SendPacket(packet);

                // Check for win
                if (CheckWin(board, move.X, move.Y, playerNumber))
                {
                    var winnerName = sender.PlayerInfo.Name ?? playerNumber.ToString();

                    // Broadcast GameOver to both players
                    var gameOverPacket = new Packet
                    {
                        Command = CommandType.GameOver,
                        Payload = Serializer.Serialize(winnerName)
                    };
                    room.Player1.SendPacket(gameOverPacket);
                    room.Player2.SendPacket(gameOverPacket);

                    Console.WriteLine($"GameService: {winnerName} won the match between {room.Player1.PlayerInfo.Name} and {room.Player2.PlayerInfo.Name}");

                    // Save history
                    _historyRepo.SaveMatch(room.Player1.PlayerInfo.Name, room.Player2.PlayerInfo.Name, winnerName);

                    // Notify matchmaking to clean up the room if provided
                    if (_roomMatchmaking.TryGetValue(room, out var matchmaking))
                    {
                        matchmaking.EndGame(room);
                    }

                    RemoveRoom(room);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameService.HandleMove error: {ex.Message}");
            }
        }

        private void FinishGame(GameRoom room, string winnerName)
        {
            try
            {
                if (room == null) return;

                // Save history
                _historyRepo.SaveMatch(room.Player1.PlayerInfo.Name, room.Player2.PlayerInfo.Name, winnerName);

                if (_roomMatchmaking.TryGetValue(room, out var matchmaking))
                {
                    matchmaking.EndGame(room);
                }

                RemoveRoom(room);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameService.FinishGame error: {ex.Message}");
            }
        }

        // Remove room and all references
        private void RemoveRoom(GameRoom room)
        {
            if (room == null) return;

            try
            {
                _rooms.Remove(room);
                if (_boards.ContainsKey(room))
                    _boards.Remove(room);
                if (_roomMatchmaking.ContainsKey(room))
                    _roomMatchmaking.Remove(room);

                // Remove player->room mappings
                if (room.Player1?.PlayerInfo?.Id != null && _playerRoomMap.ContainsKey(room.Player1.PlayerInfo.Id))
                    _playerRoomMap.Remove(room.Player1.PlayerInfo.Id);
                if (room.Player2?.PlayerInfo?.Id != null && _playerRoomMap.ContainsKey(room.Player2.PlayerInfo.Id))
                    _playerRoomMap.Remove(room.Player2.PlayerInfo.Id);

                Console.WriteLine($"GameService: Room removed for {room.Player1.PlayerInfo.Name} vs {room.Player2.PlayerInfo.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameService.RemoveRoom error: {ex.Message}");
            }
        }

        // Simple five-in-a-row checker using BoardController.GetCell
        private bool CheckWin(BoardController board, int lastX, int lastY, int player)
        {
            if (board == null) return false;

            int[,] directions = new int[,]
            {
                {1, 0}, // horizontal
                {0, 1}, // vertical
                {1, 1}, // diag down-right
                {1, -1} // diag up-right
            };

            int size = board.Size;

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int dx = directions[d, 0];
                int dy = directions[d, 1];

                int count = 1; // include last move

                int x = lastX + dx, y = lastY + dy;
                while (x >= 0 && y >= 0 && x < size && y < size && board.GetCell(x, y) == player)
                {
                    count++;
                    x += dx; y += dy;
                }

                x = lastX - dx; y = lastY - dy;
                while (x >= 0 && y >= 0 && x < size && y < size && board.GetCell(x, y) == player)
                {
                    count++;
                    x -= dx; y -= dy;
                }

                if (count >= 5) return true;
            }

            return false;
        }
    }
}