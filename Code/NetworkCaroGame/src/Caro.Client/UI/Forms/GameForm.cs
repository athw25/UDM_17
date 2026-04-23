using Caro.Client.Network;
using Caro.Client.UI.Components;
using Caro.Shared.Models;
using Caro.Shared.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.ComponentModel;


namespace Caro.Client.UI.Forms
{
    public class GameForm : Form
    {

        private Panel panelPic;
        private PictureBox pictureGame;
        private Panel panel1;
        private TextBox textBoxName_me;
        private TextBox textBoxIP;
        private ProgressBar progressBarTime;
        private Button buttonConnect;
        private Button buttonSurrender;
        private Panel panelChessBoard;
        private PictureBox pictureIconTurn;
        private Label label2;
        private TextBox textBoxName_Opponent;
        private Label label1;
        private Label labelStatus;


        private const int BOARD_SIZE = 15;
        private const int CELL_SIZE = 40;

        private Button[,] board = new Button[BOARD_SIZE, BOARD_SIZE];
        private int[,] matrix = new int[BOARD_SIZE, BOARD_SIZE];

        private string myName;
        private string opponent;
        private ClientSocket socket;

        private bool isMyTurn;
        private int myValue; // 1 = X, 2 = O
        private bool gameEnded = false;

        private Button lastMove;
        private Timer timer;
        private int timeLeft = 30;
        private int emptyCount = BOARD_SIZE * BOARD_SIZE;

        // ================= CONSTRUCTOR =================
        public GameForm(string me, string opponent, ClientSocket socket, bool isHost)
        {
            InitializeComponent();
    
            textBoxName_me.ReadOnly = true;
            textBoxName_Opponent.ReadOnly = true;
            textBoxIP.ReadOnly = true;  

            this.myName = me;
            this.opponent = opponent;
            this.socket = socket;

            textBoxName_me.Text = me;
            textBoxName_Opponent.Text = opponent;
            
            // Show IP:Port 
            string serverInfo = string.IsNullOrEmpty(socket.ServerIP) 
                ? "Connected" 
                : $"{socket.ServerIP}:{socket.ServerPort}";
            textBoxIP.Text = serverInfo;

            // Host first 
            isMyTurn = isHost;
            myValue = isHost ? 1 : 2;

            InitBoard();
            UpdateTurnUI();

            socket.OnReceive += HandlePacket;

            InitTimer();
            ResetTimer();

            // Ensure we clean up properly on form close
            this.FormClosing += GameForm_FormClosing;
        }

        // ================= INIT =================
        private void InitBoard()
        {
            panelChessBoard.Controls.Clear();

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    Button btn = new Button
                    {
                        Width = CELL_SIZE,
                        Height = CELL_SIZE,
                        Location = new Point(j * CELL_SIZE, i * CELL_SIZE),
                        Tag = new Point(i, j),
                        FlatStyle = FlatStyle.Flat
                    };
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Click += Cell_Click;
                    btn.Margin = new Padding(0);
                    btn.BackColor = Color.White;

                    board[i, j] = btn;
                    panelChessBoard.Controls.Add(btn);
                }
            }
        }

        private void InitTimer()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            progressBarTime.Maximum = 30;
            progressBarTime.Value = 30;
        }

        // ================= TIMER =================
        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            if (timeLeft < 0) timeLeft = 0;

            progressBarTime.Value = timeLeft;

            if (timeLeft <= 0)
            {
                timer.Stop();
                labelStatus.Text = "Lose (timeout)";
                gameEnded = true;
                DisableBoard();
                
                socket.Send(new Packet
                {
                    Command = CommandType.GameOver,
                    Data = JsonSerializer.Serialize(opponent)
                });
            }
        }

        private void ResetTimer()
        {
            timeLeft = 30;
            progressBarTime.Value = 30;
            timer.Start();
        }

        // ================= CLICK =================
        private void Cell_Click(object sender, EventArgs e)
        {
            if (!isMyTurn || gameEnded) return;

            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            if (matrix[p.X, p.Y] != 0) return;

            MakeMove(p.X, p.Y, myValue);
            emptyCount--;

            socket.Send(new Packet
            {
                Command = CommandType.Move,
                Data = JsonSerializer.Serialize(new MoveData
                {
                    X = p.X,
                    Y = p.Y,
                    Player = myValue
                })
            });

            // Check win 
            if (CheckWin(p.X, p.Y))
            {
                gameEnded = true;
                socket.Send(new Packet
                {
                    Command = CommandType.GameOver,
                    Data = JsonSerializer.Serialize(myName)
                });

                labelStatus.Text = "You Win!";
                StyledMessageBox.Success("Bạn đã thắng!");
                DisableBoard();
                timer.Stop();
                return;
            }

            // Check draw
            if (emptyCount == 0)
            {
                gameEnded = true;
                socket.Send(new Packet
                {
                    Command = CommandType.MatchFinished,
                    Data = "Draw"
                });

                labelStatus.Text = "Draw!";
                StyledMessageBox.Info("Kết quả hòa!");
                DisableBoard();
                timer.Stop();
                return;
            }

            isMyTurn = false;
            UpdateTurnUI();
            timer.Stop();
        }

        // ================= SURRENDER =================
        private void ButtonSurrender_Click(object sender, EventArgs e)
        {
            if (gameEnded) return;

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đầu hàng?",
                "Xác nhận",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                gameEnded = true;
                socket.Send(new Packet
                {
                    Command = CommandType.Surrender,
                    Data = myName
                });

                labelStatus.Text = "You Surrendered!";
                DisableBoard();
                timer.Stop();
            }
        }

        // ================= MAKE MOVE =================
        private void MakeMove(int x, int y, int value)
        {
            matrix[x, y] = value;

            Button btn = board[x, y];

            btn.BackgroundImage = value == 1
                ? Properties.Resources.X
                : Properties.Resources.O;

            btn.BackgroundImageLayout = ImageLayout.Stretch;

            if (lastMove != null)
            {
                lastMove.FlatAppearance.BorderSize = 1;
                lastMove.FlatAppearance.BorderColor = Color.Black;
            }

            btn.FlatAppearance.BorderColor = Color.Red;
            btn.FlatAppearance.BorderSize = 3;
            lastMove = btn;
        }

        // ================= RECEIVE =================
        private void HandlePacket(Packet packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandlePacket(packet)));
                return;
            }

            if (packet == null) return;

            try
            {
                switch (packet.Command)
                {
                    case CommandType.Move:
                        {
                            var move = JsonSerializer.Deserialize<MoveData>(packet.Data);
                            if (move == null) break;

                            MakeMove(move.X, move.Y, move.Player);
                            emptyCount--;

                            isMyTurn = true;
                            UpdateTurnUI();
                            ResetTimer();
                            break;
                        }

                    case CommandType.GameOver:
                        {
                            if (gameEnded) break;
                            gameEnded = true;
                            timer.Stop();

                            string winnerName = "";
                            try { winnerName = JsonSerializer.Deserialize<string>(packet.Data); }
                            catch { winnerName = packet.Data; }

                            if (winnerName == myName)
                            {
                                labelStatus.Text = "You Win!";
                                StyledMessageBox.Success("Bạn đã thắng!");
                            }
                            else
                            {
                                labelStatus.Text = "You Lose!";
                                StyledMessageBox.Error("Bạn đã thua!");
                            }

                            DisableBoard();
                            break;
                        }

                    case CommandType.Surrender:
                        {
                            if (gameEnded) break;
                            gameEnded = true;
                            timer.Stop();

                            labelStatus.Text = "You Win (Opponent surrendered)!";
                            StyledMessageBox.Success("Đối thủ đã đầu hàng! Bạn thắng!");
                            DisableBoard();
                            break;
                        }

                    case CommandType.MatchFinished:
                        {
                            if (gameEnded) break;
                            gameEnded = true;
                            timer.Stop();

                            string result = packet.Data;
                            if (result == "Draw")
                            {
                                labelStatus.Text = "Draw!";
                                StyledMessageBox.Info("Kết quả hòa!");
                            }

                            DisableBoard();
                            break;
                        }

                    case CommandType.OpponentDisconnected:
                        {
                            if (gameEnded) break;
                            gameEnded = true;
                            timer.Stop();

                            labelStatus.Text = "You Win (Opponent disconnected)!";
                            StyledMessageBox.Success("Đối thủ đã thoát trận! Bạn thắng!");
                            DisableBoard();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UI Exception in GameForm HandlePacket: {ex}");
            }
        }

        // ================= WIN CHECK =================
        private bool CheckWin(int x, int y)
        {
            return CheckLine(x, y, 1, 0) ||
                   CheckLine(x, y, 0, 1) ||
                   CheckLine(x, y, 1, 1) ||
                   CheckLine(x, y, 1, -1);
        }

        private bool CheckLine(int x, int y, int dx, int dy)
        {
            int count = 1;
            int value = matrix[x, y];

            for (int i = 1; i < 5; i++)
            {
                int nx = x + dx * i;
                int ny = y + dy * i;

                if (nx < 0 || ny < 0 || nx >= BOARD_SIZE || ny >= BOARD_SIZE) break;
                if (matrix[nx, ny] != value) break;

                count++;
            }

            for (int i = 1; i < 5; i++)
            {
                int nx = x - dx * i;
                int ny = y - dy * i;

                if (nx < 0 || ny < 0 || nx >= BOARD_SIZE || ny >= BOARD_SIZE) break;
                if (matrix[nx, ny] != value) break;

                count++;
            }

            return count >= 5;
        }

        // ================= UI =================
        private void UpdateTurnUI()
        {
            if (isMyTurn && !gameEnded)
            {
                pictureIconTurn.Image = myValue == 1
                    ? Properties.Resources.X
                    : Properties.Resources.O;

                labelStatus.Text = "Your Turn";
            }
            else if (!gameEnded)
            {
                pictureIconTurn.Image = myValue == 1
                    ? Properties.Resources.O
                    : Properties.Resources.X;

                labelStatus.Text = "Waiting...";
            }

            foreach (var btn in board)
            {
                Point p = (Point)btn.Tag;
                btn.Enabled = isMyTurn && !gameEnded && matrix[p.X, p.Y] == 0;
            }

            buttonSurrender.Enabled = !gameEnded;
            pictureIconTurn.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void DisableBoard()
        {
            foreach (var btn in board)
                btn.Enabled = false;

            buttonSurrender.Enabled = false;
            timer.Stop();
        }

        // ================= FORM CLOSING =================
        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Unsubscribe from socket events FIRST to prevent any packet handling during cleanup
                if (socket != null)
                {
                    socket.OnReceive -= HandlePacket;
                }

                // Send disconnect notification only if game is not already ended
                if (!gameEnded && socket != null && socket.IsConnected)
                {
                    try
                    {
                        socket.Send(new Packet
                        {
                            Command = CommandType.Disconnect,
                            Data = myName
                        });
                        System.Threading.Thread.Sleep(100); // Brief delay to ensure packet is sent
                    }
                    catch { }
                }

                // Stop and properly dispose timer
                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick;
                    timer.Dispose();
                    timer = null;
                }

                // Clean up board
                if (panelChessBoard != null)
                {
                    panelChessBoard.Controls.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GameForm_FormClosing: {ex.Message}");
            }
        }

        // ================= BUTTON RETURN =================
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            // Disable button to prevent multiple clicks
            buttonConnect.Enabled = false;
            
            try
            {
                // Stop timer immediately
                if (timer != null && timer.Enabled)
                {
                    timer.Stop();
                }

                // Unsubscribe from socket before closing
                if (socket != null)
                {
                    socket.OnReceive -= HandlePacket;
                }

                // Close this form (FormClosing event will be triggered)
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in buttonConnect_Click: {ex.Message}");
                buttonConnect.Enabled = true;
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(GameForm));
            panelPic = new Panel();
            pictureGame = new PictureBox();
            panel1 = new Panel();
            labelStatus = new Label();
            label1 = new Label();
            textBoxName_Opponent = new TextBox();
            pictureIconTurn = new PictureBox();
            label2 = new Label();
            buttonConnect = new Button();
            buttonSurrender = new Button();
            textBoxIP = new TextBox();
            progressBarTime = new ProgressBar();
            textBoxName_me = new TextBox();
            panelChessBoard = new Panel();
            panelPic.SuspendLayout();
            ((ISupportInitialize)pictureGame).BeginInit();
            panel1.SuspendLayout();
            ((ISupportInitialize)pictureIconTurn).BeginInit();
            SuspendLayout();
            
            panelPic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelPic.Controls.Add(pictureGame);
            panelPic.Location = new Point(688, 12);
            panelPic.Name = "panelPic";
            panelPic.Size = new Size(350, 350);
            panelPic.TabIndex = 1;
            
            pictureGame.BackgroundImageLayout = ImageLayout.Stretch;
            pictureGame.Image = Properties.Resources._9a91e11d31b376abcc3b8f28cec9414b;
            pictureGame.ImageLocation = "";
            pictureGame.Location = new Point(27, 22);
            pictureGame.Name = "pictureGame";
            pictureGame.Size = new Size(300, 300);
            pictureGame.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureGame.TabIndex = 0;
            pictureGame.TabStop = false;
            
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel1.Controls.Add(labelStatus);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(textBoxName_Opponent);
            panel1.Controls.Add(pictureIconTurn);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(buttonSurrender);
            panel1.Controls.Add(buttonConnect);
            panel1.Controls.Add(textBoxIP);
            panel1.Controls.Add(progressBarTime);
            panel1.Controls.Add(textBoxName_me);
            panel1.Location = new Point(688, 377);
            panel1.Name = "panel1";
            panel1.Size = new Size(354, 280);
            panel1.TabIndex = 2;
            
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(4, 220);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(52, 20);
            labelStatus.TabIndex = 10;
            labelStatus.Text = "Status:";
            
            label1.AutoSize = true;
            label1.Location = new Point(3, 111);
            label1.Name = "label1";
            label1.Size = new Size(71, 20);
            label1.TabIndex = 9;
            label1.Text = "Time left:";
            
            textBoxName_Opponent.Location = new Point(3, 69);
            textBoxName_Opponent.Name = "textBoxName_Opponent";
            textBoxName_Opponent.Size = new Size(183, 27);
            textBoxName_Opponent.TabIndex = 8;
            textBoxName_Opponent.ReadOnly = true;
            
            pictureIconTurn.Location = new Point(192, 36);
            pictureIconTurn.Name = "pictureIconTurn";
            pictureIconTurn.Size = new Size(135, 127);
            pictureIconTurn.TabIndex = 7;
            pictureIconTurn.TabStop = false;
            
            label2.AutoSize = true;
            label2.Location = new Point(192, 6);
            label2.Name = "label2";
            label2.Size = new Size(95, 20);
            label2.TabIndex = 6;
            label2.Text = "Symbol Turn:";
            
            buttonConnect.Location = new Point(192, 220);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(135, 29);
            buttonConnect.TabIndex = 4;
            buttonConnect.Text = "Return to Lobby";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;

            buttonSurrender.Location = new Point(192, 185);
            buttonSurrender.Name = "buttonSurrender";
            buttonSurrender.Size = new Size(135, 29);
            buttonSurrender.TabIndex = 5;
            buttonSurrender.Text = "Surrender";
            buttonSurrender.UseVisualStyleBackColor = true;
            buttonSurrender.Click += ButtonSurrender_Click;
            
            textBoxIP.Location = new Point(3, 3);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new Size(183, 27);
            textBoxIP.TabIndex = 3;
            textBoxIP.ReadOnly = true;
            
            progressBarTime.Location = new Point(3, 134);
            progressBarTime.Name = "progressBarTime";
            progressBarTime.Size = new Size(183, 29);
            progressBarTime.TabIndex = 1;
            
            textBoxName_me.Location = new Point(3, 36);
            textBoxName_me.Name = "textBoxName_me";
            textBoxName_me.Size = new Size(183, 27);
            textBoxName_me.TabIndex = 0;
            textBoxName_me.ReadOnly = true;
            
            panelChessBoard.Location = new Point(12, 12);
            panelChessBoard.Name = "panelChessBoard";
            panelChessBoard.Size = new Size(670, 600);
            panelChessBoard.TabIndex = 0;
            
            ClientSize = new System.Drawing.Size(1050, 670);
            Controls.Add(panelChessBoard);
            Controls.Add(panelPic);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GameForm";
            Text = "Game Caro - Playing";
            panelPic.ResumeLayout(false);
            ((ISupportInitialize)pictureGame).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)pictureIconTurn).EndInit();
            ResumeLayout(false);
        }

        private void pictureGame_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void progressBarTime_Click(object sender, EventArgs e) { }
        private void textBoxName_me_TextChanged(object sender, EventArgs e) { }
        private void textBoxName_Opponent_TextChanged(object sender, EventArgs e) { }
    }
}