using Caro.Client.Network;
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

        private Button lastMove;
        private Timer timer;
        private int timeLeft = 30;

        // ================= CONSTRUCTOR =================
        public GameForm(string me, string opponent, ClientSocket socket, bool isHost)
        {
            InitializeComponent();

            this.myName = me;
            this.opponent = opponent;
            this.socket = socket;

            textBoxName_me.Text = me;
            textBoxName_Opponent.Text = opponent;

            textBoxIP.Text = "Connected";

            // Host đi trước
            isMyTurn = isHost;
            myValue = isHost ? 1 : 2;

            UpdateTurnUI();

            socket.OnReceive += HandlePacket;

            InitBoard();
            InitTimer();
            ResetTimer();
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
                DisableBoard();
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
            if (!isMyTurn) return;

            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            if (matrix[p.X, p.Y] != 0) return;

            MakeMove(p.X, p.Y, myValue);

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

            isMyTurn = false;
            UpdateTurnUI();
            timer.Stop();
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

            // highlight
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

            switch (packet.Command)
            {
                case CommandType.Move:
                    {
                        var move = JsonSerializer.Deserialize<MoveData>(packet.Data);

                        MakeMove(move.X, move.Y, move.Player);

                        isMyTurn = true;
                        UpdateTurnUI();
                        ResetTimer();
                        break;
                    }

                case CommandType.Win:
                    {
                        timer.Stop();
                        if (packet.Data == "WIN")
                            labelStatus.Text = "You Win!";
                        else
                            labelStatus.Text = "You Lose!";

                        DisableBoard();
                        break;
                    }

                case CommandType.Disconnect:
                    {
                        MessageBox.Show("Opponent disconnected!");
                        labelStatus.Text = "You Win (Opponent left)";
                        DisableBoard();
                        break;
                    }
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
            if (isMyTurn)
            {
                pictureIconTurn.Image = myValue == 1
                    ? Properties.Resources.X
                    : Properties.Resources.O;

                labelStatus.Text = "Your Turn";
            }
            else
            {
                pictureIconTurn.Image = myValue == 1
                    ? Properties.Resources.O
                    : Properties.Resources.X;

                labelStatus.Text = "Waiting...";
            }
            foreach (var btn in board)
                btn.Enabled = isMyTurn && matrix[((Point)btn.Tag).X, ((Point)btn.Tag).Y] == 0;

            pictureIconTurn.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void DisableBoard()
        {
            foreach (var btn in board)
                btn.Enabled = false;

            timer.Stop();
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
            textBoxIP = new TextBox();
            progressBarTime = new ProgressBar();
            textBoxName_me = new TextBox();
            panelChessBoard = new Panel();
            panelPic.SuspendLayout();
            ((ISupportInitialize)pictureGame).BeginInit();
            panel1.SuspendLayout();
            ((ISupportInitialize)pictureIconTurn).BeginInit();
            SuspendLayout();
            // 
            // panelPic
            // 
            panelPic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelPic.Controls.Add(pictureGame);
            panelPic.Location = new Point(688, 12);
            panelPic.Name = "panelPic";
            panelPic.Size = new Size(350, 350);
            panelPic.TabIndex = 1;
            // 
            // pictureGame
            // 
            pictureGame.BackgroundImageLayout = ImageLayout.Stretch;
            pictureGame.Image = Properties.Resources._9a91e11d31b376abcc3b8f28cec9414b;
            pictureGame.ImageLocation = "";
            pictureGame.Location = new Point(27, 22);
            pictureGame.Name = "pictureGame";
            pictureGame.Size = new Size(300, 300);
            pictureGame.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureGame.TabIndex = 0;
            pictureGame.TabStop = false;
            pictureGame.Click += pictureGame_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel1.Controls.Add(labelStatus);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(textBoxName_Opponent);
            panel1.Controls.Add(pictureIconTurn);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(buttonConnect);
            panel1.Controls.Add(textBoxIP);
            panel1.Controls.Add(progressBarTime);
            panel1.Controls.Add(textBoxName_me);
            panel1.Location = new Point(688, 377);
            panel1.Name = "panel1";
            panel1.Size = new Size(354, 235);
            panel1.TabIndex = 2;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(4, 169);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(52, 20);
            labelStatus.TabIndex = 10;
            labelStatus.Text = "Status:";
            labelStatus.Click += label3_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 111);
            label1.Name = "label1";
            label1.Size = new Size(71, 20);
            label1.TabIndex = 9;
            label1.Text = "Time left:";
            label1.Click += label1_Click;
            // 
            // textBoxName_Opponent
            // 
            textBoxName_Opponent.Location = new Point(3, 69);
            textBoxName_Opponent.Name = "textBoxName_Opponent";
            textBoxName_Opponent.Size = new Size(183, 27);
            textBoxName_Opponent.TabIndex = 8;
            textBoxName_Opponent.Text = "Username(Opponent)";
            // 
            // pictureIconTurn
            // 
            pictureIconTurn.Location = new Point(192, 36);
            pictureIconTurn.Name = "pictureIconTurn";
            pictureIconTurn.Size = new Size(135, 127);
            pictureIconTurn.TabIndex = 7;
            pictureIconTurn.TabStop = false;
            pictureIconTurn.Click += pictureBox1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(192, 6);
            label2.Name = "label2";
            label2.Size = new Size(95, 20);
            label2.TabIndex = 6;
            label2.Text = "Symbol Turn:";
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(192, 181);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(135, 29);
            buttonConnect.TabIndex = 4;
            buttonConnect.Text = "Connect";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new Point(3, 3);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new Size(183, 27);
            textBoxIP.TabIndex = 3;
            textBoxIP.Text = "IP Server";
            textBoxIP.TextChanged += textBox2_TextChanged;
            // 
            // progressBarTime
            // 
            progressBarTime.Location = new Point(3, 134);
            progressBarTime.Name = "progressBarTime";
            progressBarTime.Size = new Size(183, 29);
            progressBarTime.TabIndex = 1;
            progressBarTime.Click += progressBarTime_Click;
            // 
            // textBoxName_me
            // 
            textBoxName_me.Location = new Point(3, 36);
            textBoxName_me.Name = "textBoxName_me";
            textBoxName_me.Size = new Size(183, 27);
            textBoxName_me.TabIndex = 0;
            textBoxName_me.Text = "Username(me)";
            // 
            // panelChessBoard
            // 
            panelChessBoard.Location = new Point(21, 11);
            panelChessBoard.Name = "panelChessBoard";
            panelChessBoard.Size = new Size(642, 601);
            panelChessBoard.TabIndex = 3;
            // 
            // GameForm
            // 
            ClientSize = new Size(1062, 689);
            Controls.Add(panelChessBoard);
            Controls.Add(panel1);
            Controls.Add(panelPic);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "GameForm";
            Text = "Game Caro ";
            Load += GameForm_Load;
            panelPic.ResumeLayout(false);
            ((ISupportInitialize)pictureGame).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)pictureIconTurn).EndInit();
            ResumeLayout(false);

        }

        private void pictureGame_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void GameForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {

        }

        private void pictureBoxIcon_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void progressBarTime_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}