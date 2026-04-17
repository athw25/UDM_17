using System;
using System.Collections.Generic;

namespace Caro.Shared.Game
{
    public class BoardController
    {
        private readonly int[,] board;
        public int Size { get; private set; }

        // store move history for undo/replay
        private readonly Stack<(int x, int y)> history = new Stack<(int, int)>();

        /// <summary>
        /// Create a board. Default size 20x20 to match server/client expectations.
        /// </summary>
        public BoardController(int size = 20)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
            board = new int[size, size];
        }

        // 0 = empty, 1 = player1, 2 = player2
        public int GetCell(int x, int y)
        {
            ValidateCoordinates(x, y);
            return board[x, y];
        }

        public bool IsValidMove(int x, int y)
        {
            if (!IsInside(x, y)) return false;
            return board[x, y] == 0;
        }

        /// <summary>
        /// Place a move for specified player (1 or 2). Returns true if placed.
        /// </summary>
        public bool MakeMove(int x, int y, int player)
        {
            if (player != 1 && player != 2) throw new ArgumentException("player must be 1 or 2", nameof(player));
            if (!IsValidMove(x, y)) return false;

            board[x, y] = player;   
            history.Push((x, y));
            return true;
        }

        /// <summary>
        /// Undo last move. Returns (x,y,player) if undone, null otherwise.
        /// </summary>
        public (int x, int y, int player)? UndoLastMove()
        {
            if (history.Count == 0) return null;

            var last = history.Pop();
            int px = last.x, py = last.y;
            int player = board[px, py];
            board[px, py] = 0;
            return (px, py, player);
        }

        /// <summary>
        /// Reset board to empty state.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    board[i, j] = 0;
            history.Clear();
        }

        /// <summary>
        /// Check whether the last move at (lastX,lastY) by 'player' produces a win (five-in-a-row).
        /// Returns true when player has >=5 contiguous stones in any direction.
        /// </summary>
        public bool CheckWin(int lastX, int lastY, int player)
        {
            if (player != 1 && player != 2) return false;
            if (!IsInside(lastX, lastY)) return false;
            if (board[lastX, lastY] != player) return false;

            // directions: horizontal, vertical, diag down-right, diag up-right
            var directions = new (int dx, int dy)[]
            {
                (1, 0),
                (0, 1),
                (1, 1),
                (1, -1)
            };

            foreach (var (dx, dy) in directions)
            {
                int count = 1;

                // forward
                int x = lastX + dx, y = lastY + dy;
                while (IsInside(x, y) && board[x, y] == player)
                {
                    count++; x += dx; y += dy;
                }

                // backward
                x = lastX - dx; y = lastY - dy;
                while (IsInside(x, y) && board[x, y] == player)
                {
                    count++; x -= dx; y -= dy;
                }

                if (count >= 5) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a shallow copy of the board array (useful for read-only inspection).
        /// </summary>
        public int[,] GetSnapshot()
        {
            var result = new int[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    result[i, j] = board[i, j];
            return result;
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size;
        }

        private void ValidateCoordinates(int x, int y)
        {
            if (!IsInside(x, y)) throw new ArgumentOutOfRangeException($"Coordinates ({x},{y}) are outside the board.");
        }
    }
}