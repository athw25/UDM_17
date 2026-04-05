using System;
using System.Drawing;
using System.Windows.Forms;
using Caro.Client.Network;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.UI.Forms
{
    public class GameForm : Form
    {
        private const int BoardSize = 15;
        private const int CellSize = 30;
        private Button[,] _cells = new Button[BoardSize, BoardSize];
        private Timer _timer;
        private int _timeLeft = 30;
        private Label _lblStatus;
        private Label _lblTimer;
        
        private string _playerName;
        private bool _isMyTurn;
        private string _mySymbol;
        private string _oppSymbol;
        private bool _gameOver;

        public GameForm(string playerName, bool initialTurn)
        {
            _playerName = playerName;
            _isMyTurn = initialTurn;
            _mySymbol = initialTurn ? "X" : "O";
            _oppSymbol = initialTurn ? "O" : "X";
            
            InitializeComponent();
            ClientSocket.Instance.OnPacketReceived += OnPacketReceived;
            FormClosed += (s, e) => Environment.Exit(0);
        }

        private void InitializeComponent()
        {
            Text = $"Caro - {_playerName} ({_mySymbol})";
            Size = new Size(BoardSize * CellSize + 250, BoardSize * CellSize + 60);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Panel boardPanel = new Panel
            {
                Size = new Size(BoardSize * CellSize, BoardSize * CellSize),
                Location = new Point(10, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(CellSize, CellSize),
                        Location = new Point(j * CellSize, i * CellSize),
                        Tag = new Point(j, i),
                        Font = new Font("Arial", 12, FontStyle.Bold)
                    };
                    btn.Click += Cell_Click;
                    _cells[j, i] = btn;
                    boardPanel.Controls.Add(btn);
                }
            }

            _lblStatus = new Label
            {
                Text = _isMyTurn ? "Your turn!" : "Waiting for opponent...",
                Location = new Point(BoardSize * CellSize + 20, 20),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = _isMyTurn ? Color.Green : Color.Red
            };

            _lblTimer = new Label
            {
                Text = $"Time left: {_timeLeft}s",
                Location = new Point(BoardSize * CellSize + 20, 60),
                AutoSize = true,
                Font = new Font("Arial", 10)
            };

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            
            if (_isMyTurn) _timer.Start();

            Controls.Add(boardPanel);
            Controls.Add(_lblStatus);
            Controls.Add(_lblTimer);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isMyTurn || _gameOver) return;
            
            _timeLeft--;
            _lblTimer.Text = $"Time left: {_timeLeft}s";
            
            if (_timeLeft <= 0)
            {
                _timer.Stop();
                _gameOver = true;
                MessageBox.Show("Time out! You lose.");
                ClientSocket.Instance.Send(new Packet { Command = CommandType.TimeOut, Payload = "timeout" });
                ClientSocket.Instance.Send(new Packet { Command = CommandType.GameOver, Payload = Serializer.Serialize("Opponent (TimeOut)") });
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            if (!_isMyTurn || _gameOver) return;

            Button btn = sender as Button;
            if (btn == null || !string.IsNullOrEmpty(btn.Text)) return;

            Point p = (Point)btn.Tag;
            btn.Text = _mySymbol;
            btn.ForeColor = Color.Blue;

            _timer.Stop();
            _isMyTurn = false;
            UpdateStatusUI();

            var move = new MoveInfo { X = p.X, Y = p.Y, PlayerId = _playerName };
            ClientSocket.Instance.Send(new Packet { Command = CommandType.Move, Payload = Serializer.Serialize(move) });

            if (CheckWin(p.X, p.Y, _mySymbol))
            {
                _gameOver = true;
                MessageBox.Show("You win!");
                ClientSocket.Instance.Send(new Packet { Command = CommandType.GameOver, Payload = Serializer.Serialize(_playerName) });
            }
        }

        private void UpdateStatusUI()
        {
            _lblStatus.Text = _isMyTurn ? "Your turn!" : "Waiting for opponent...";
            _lblStatus.ForeColor = _isMyTurn ? Color.Green : Color.Red;
            _timeLeft = 30;
            _lblTimer.Text = $"Time left: {_timeLeft}s";
            if (_isMyTurn) _timer.Start();
        }

        private void OnPacketReceived(Packet packet)
        {
            if (_gameOver) return;

            Invoke(new Action(() =>
            {
                switch (packet.Command)
                {
                    case CommandType.Move:
                        var move = Serializer.Deserialize<MoveInfo>(packet.Payload);
                        _cells[move.X, move.Y].Text = _oppSymbol;
                        _cells[move.X, move.Y].ForeColor = Color.Red;
                        
                        _isMyTurn = true;
                        UpdateStatusUI();
                        break;

                    case CommandType.TimeOut:
                        _timer.Stop();
                        _gameOver = true;
                        MessageBox.Show("Opponent ran out of time. You win!");
                        break;

                    case CommandType.GameOver:
                        _timer.Stop();
                        _gameOver = true;
                        string winner = Serializer.Deserialize<string>(packet.Payload);
                        MessageBox.Show($"Game Over! {winner} wins.");
                        break;

                    case CommandType.PlayerDisconnected:
                        _timer.Stop();
                        _gameOver = true;
                        MessageBox.Show("Opponent disconnected. You win!");
                        break;
                }
            }));
        }

        private bool CheckWin(int x, int y, string symbol)
        {
            // Simple 5-in-a-row checker (Horizontal, Vertical, 2 Diagonals)
            int[][] directions = new[]
            {
                new[] { 1, 0 }, new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, -1 }
            };

            foreach (var dir in directions)
            {
                int count = 1;
                count += CountDirection(x, y, dir[0], dir[1], symbol);
                count += CountDirection(x, y, -dir[0], -dir[1], symbol);

                if (count >= 5) return true;
            }
            return false;
        }

        private int CountDirection(int startX, int startY, int dx, int dy, string symbol)
        {
            int count = 0;
            int x = startX + dx;
            int y = startY + dy;

            while (x >= 0 && x < BoardSize && y >= 0 && y < BoardSize)
            {
                if (_cells[x, y].Text == symbol) count++;
                else break;
                x += dx;
                y += dy;
            }
            return count;
        }
    }
}