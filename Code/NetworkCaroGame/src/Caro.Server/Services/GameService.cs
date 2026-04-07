using System.Collections.Generic;
using System.Linq;
using Caro.Server.Core;
using Caro.Server.Game;
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
    //chua co trong packet
/*public void HandleMove(ClientHandler client, Packet packet)
{
    //check running
    if (!_room.IsPlaying) return;

    int x = packet.Data["x"];
    int y = packet.Data["y"];
    int player = GetPlayerIndex(client);

    if (player != _currentPlayer) return;

    if (!_board.MakeMove(x, y, player)) return;

    Broadcast("MOVE", x, y, player);

    // check win
    if (_board.CheckWin(x, y, player))
    {
        EndGame(player);
        return;
    }

    // check draw
    if (_board.IsDraw())
    {
        EndGame(0); // 0 = hòa
        return;
    }

    SwitchTurn();
}*/
    }
}
