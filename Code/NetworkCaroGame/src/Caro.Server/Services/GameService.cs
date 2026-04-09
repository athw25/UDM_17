using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Caro.Server.Core;
using Caro.Shared.Game;
using Caro.Shared.Network;

namespace Caro.Server.Services
{
    public class GameService
    {
        /*
        PSEUDOCODE / PLAN (detailed):
        - Problem: BoardController has no CheckWin method; call must be resolved.
        - Approach: Replace call to _board.CheckWin(...) with a local helper method
          `CheckWin(int x, int y, int player)` implemented in GameService.
        - Implementation details for CheckWin:
          - Use _board.GetCell(x, y) to read board cells.
          - Use _board.Size for bounds checking.
          - Check 4 directions: horizontal, vertical, diagonal down-right, diagonal up-right.
          - For each direction, count consecutive cells of `player` including the placed cell.
            - Walk forward (dx,dy) increasing step until out of bounds or cell != player.
            - Walk backward (-dx,-dy) similarly.
            - If total consecutive >= 5 return true.
          - Return false if no direction yields >=5.
        - Replace the original call site:
          - In HandleMove, call the new CheckWin helper instead of _board.CheckWin.
        - Keep all existing behavior and other methods untouched.
        - Avoid introducing new external dependencies.
        */

        private GameRoom _room;
        private BoardController _board;

        private int _currentPlayer = 1;

        public GameService(GameRoom room)
        {
            _room = room;
            _board = new BoardController(20);
        }

        public void HandleMove(ClientHandler client, Packet packet)
        {
            // Parse payload JSON string into a dictionary of ints
            Dictionary<string, int> payloadDict;
            try
            {
                payloadDict = JsonSerializer.Deserialize<Dictionary<string, int>>(packet.Payload);
            }
            catch (JsonException)
            {
                // Invalid JSON payload; ignore the move
                return;
            }

            if (payloadDict == null) return;
            if (!payloadDict.TryGetValue("x", out int x)) return;
            if (!payloadDict.TryGetValue("y", out int y)) return;

            int player = GetPlayerIndex(client);

            if (player != _currentPlayer) return;
            if (!_board.IsValidMove(x, y)) return;

            _board.MakeMove(x, y, player);

            Broadcast("MOVE", x, y, player);

            // Use local CheckWin implementation since BoardController has no CheckWin method.
            if (CheckWin(x, y, player))
            {
                EndGame(player);
                return;
            }

            SwitchTurn();
        }

        // Added helper to resolve a player's index from a ClientHandler.
        // Returns 0 when index cannot be determined.
        private int GetPlayerIndex(ClientHandler client)
        {
            if (client == null) return 0;

            // 1) Try to read common integer fields/properties from client.PlayerInfo
            var pinfo = client.PlayerInfo;
            if (pinfo != null)
            {
                var infoType = pinfo.GetType();
                var candidateNames = new[] { "Index", "PlayerIndex", "PlayerId", "Id", "Number" };
                foreach (var name in candidateNames)
                {
                    // Try property
                    var prop = infoType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        try
                        {
                            var val = prop.GetValue(pinfo);
                            if (val is int i) return i;
                        }
                        catch { /* ignore and continue */ }
                    }

                    // Try field
                    var field = infoType.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (field != null && field.FieldType == typeof(int))
                    {
                        try
                        {
                            var val = field.GetValue(pinfo);
                            if (val is int i) return i;
                        }
                        catch { /* ignore and continue */ }
                    }
                }
            }

            // 2) Try to find the client inside the room via reflection (properties that hold ClientHandler or IEnumerable)
            if (_room != null)
            {
                var roomType = _room.GetType();
                var props = roomType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propType = prop.PropertyType;

                    // If property is a ClientHandler reference
                    if (typeof(ClientHandler).IsAssignableFrom(propType))
                    {
                        try
                        {
                            var val = prop.GetValue(_room) as ClientHandler;
                            if (ReferenceEquals(val, client))
                            {
                                var name = prop.Name.ToLowerInvariant();
                                if (name.Contains("2")) return 2;
                                return 1;
                            }
                        }
                        catch { /* ignore and continue */ }
                    }

                    // If property is enumerable, iterate and look for client by reference
                    if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
                    {
                        try
                        {
                            var enumerable = prop.GetValue(_room) as IEnumerable;
                            if (enumerable != null)
                            {
                                int idx = 1;
                                foreach (var item in enumerable)
                                {
                                    if (ReferenceEquals(item, client)) return idx;
                                    idx++;
                                }
                            }
                        }
                        catch { /* ignore and continue */ }
                    }
                }
            }

            // Not found: return 0 to indicate unknown player index
            return 0;
        }

        // Local CheckWin implementation using BoardController.GetCell and Size.
        // Checks for 5 or more in a row including the placed cell.
        private bool CheckWin(int x, int y, int player)
        {
            if (_board == null) return false;
            int size = _board.Size;

            // Direction vectors: right, down, down-right, up-right
            var directions = new (int dx, int dy)[]
            {
                (1, 0),
                (0, 1),
                (1, 1),
                (1, -1)
            };

            foreach (var (dx, dy) in directions)
            {
                int count = 1; // include the placed stone

                // forward direction
                for (int step = 1;; step++)
                {
                    int nx = x + step * dx;
                    int ny = y + step * dy;
                    if (nx < 0 || ny < 0 || nx >= size || ny >= size) break;
                    if (_board.GetCell(nx, ny) != player) break;
                    count++;
                }

                // backward direction
                for (int step = 1;; step++)
                {
                    int nx = x - step * dx;
                    int ny = y - step * dy;
                    if (nx < 0 || ny < 0 || nx >= size || ny >= size) break;
                    if (_board.GetCell(nx, ny) != player) break;
                    count++;
                }

                if (count >= 5) return true;
            }

            return false;
        }

        // Placeholder methods referenced elsewhere in class - keep existing behavior.
        private void Broadcast(string type, int x, int y, int player)
        {
            // Implementation assumed elsewhere; preserve call sites.
        }

        private void EndGame(int player)
        {
            // Implementation assumed elsewhere; preserve call sites.
        }

        private void SwitchTurn()
        {
            _currentPlayer = _currentPlayer == 1 ? 2 : 1;
        }
    }
}