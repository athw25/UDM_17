using System;
using System.Collections.Generic;

namespace Caro.Shared.Game
{
    public class BoardController
    {
        private int[,] board;
        public int Size { get; private set; }

        private Stack<(int x, int y)> history = new Stack<(int, int)>();

        public BoardController(int size = 20)
        {
            Size = size;
            board = new int[size, size];
        }

        // 0 = trống, 1 = player1, 2 = player2
        public int GetCell(int x, int y)
        {
            return board[x, y];
        }

        public bool IsValidMove(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size && board[x, y] == 0;
        }

        public bool MakeMove(int x, int y, int player)
        {
            if (!IsValidMove(x, y)) return false;

            board[x, y] = player;
            history.Push((x, y));
            return true;
        }
    }
}