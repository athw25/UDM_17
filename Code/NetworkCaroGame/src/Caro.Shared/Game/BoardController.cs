using System;
using System.Collections.Generic;

namespace Caro.Server.Game
{
    public class BoardController
    {
        private int[,] board;
        public int Size { get; private set; }
        private Stack<(int x, int y)> moveHistory = new Stack<(int, int)>();

        public BoardController(int size)
        {
            Size = size;
            board = new int[size, size];
        }

        public bool IsValidMove(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size && board[x, y] == 0;
        }

        public bool MakeMove(int x, int y, int player)
        {
            if (!IsValidMove(x, y)) return false;
            board[x, y] = player;
            moveHistory.Push((x, y));
            return true;
        }

        public (int x, int y)? Undo()
        {
            if (moveHistory.Count == 0) return null;
            var move = moveHistory.Pop();
            board[move.x, move.y] = 0;
            return move;
        }
    }
}
