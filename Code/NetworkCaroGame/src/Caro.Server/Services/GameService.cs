using System.Collections.Generic;
using Caro.Server.Core;
using Caro.Shared.Game;
using Caro.Shared.Network;

namespace Caro.Server.Services
{
    public class GameService
    {
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
            int x = packet.Data["x"];
            int y = packet.Data["y"];
            int player = GetPlayerIndex(client);

            if (player != _currentPlayer) return;
            if (!_board.IsValidMove(x, y)) return;

            _board.MakeMove(x, y, player);

            Broadcast("MOVE", x, y, player);

            if (_board.CheckWin(x, y, player))
            {
                EndGame(player);
                return;
            }

            SwitchTurn();
        }

    }
}
