using Caro.Client.Network;
using Caro.Client.UI.Helpers;
using Caro.Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Caro.Client.UI.Forms
{
    public class LobbyForm : Form
    {
        private Panel panel2;
        private Label label2;
        private Label label1;
        private Panel panel1;
        private Panel panel3;
        private Label label3;
        private ListBox listBoxPlayer;
        private Button buttonChallenge;
        private Button buttonHistory;
        private Button buttonRefresh;
        private Label label4;
        private Button buttonLogout;
        private Label labelStatus;

        private string username;
        private ClientSocket socket;

        // ================= CONSTRUCTOR =================
        public LobbyForm(string user, ClientSocket socket)
        {
            username = user;
            this.socket = socket;

            InitializeComponent(); // GIỮ NGUYÊN UI

            // UI config
            buttonChallenge.Enabled = false;
            listBoxPlayer.SelectedIndexChanged += (s, e) =>
            {
                buttonChallenge.Enabled = listBoxPlayer.SelectedItem != null;
            };

            // events
            buttonChallenge.Click += BtnChallenge_Click;
            buttonRefresh.Click += BtnRefresh_Click;
            buttonHistory.Click += BtnHistory_Click;
            buttonLogout.Click += BtnLogout_Click;

            // nhận data từ server
            this.socket.OnReceive += HandlePacket;
        }

        // ================= HANDLE PACKET =================
        private void HandlePacket(Packet packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandlePacket(packet)));
                return;
            }

            switch (packet.Command)
            {
                case CommandType.PlayerList:
                    var players = JsonSerializer.Deserialize<List<string>>(packet.Data);

                    listBoxPlayer.DataSource = null;
                    listBoxPlayer.DataSource = players
                        .Where(p => p != username)
                        .ToList();

                    UpdateStatus();
                    break;

                case CommandType.Challenge:
                    string fromUser = packet.Data;

                    var result = MessageBox.Show(
                        $"{fromUser} challenged you",
                        "Challenge",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        socket.Send(new Packet
                        {
                            Command = CommandType.Accept,
                            Data = fromUser
                        });
                    }
                    else
{
    socket.Send(new Packet
    {
        Command = CommandType.Reject,
        Data = fromUser
    });
}
                    break;
                case CommandType.Reject:
                    {
                        MessageBox.Show($"{packet.Data} rejected your challenge!");
                        break;
                    }

                case CommandType.StartGame:
                    string opponent = packet.Data;

                    UIHelper.SwitchForm(this, new GameForm(username, opponent));
                    break;
            }
        }

        // ================= UPDATE UI =================
        private void UpdateStatus()
        {
            int count = listBoxPlayer.Items.Count;
            labelStatus.Text = $"Status: Online Players: {count}";
        }

        // ================= EVENTS =================

        // Challenge
        private void BtnChallenge_Click(object sender, EventArgs e)
        {
            if (listBoxPlayer.SelectedItem == null)
            {
                MessageBox.Show("Please select a player!");
                return;
            }

            string opponent = listBoxPlayer.SelectedItem.ToString();

            socket.Send(new Packet
            {
                Command = CommandType.Challenge,
                Data = opponent
            });
        }

        // Refresh
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            socket.Send(new Packet
            {
                Command = CommandType.PlayerList,
                Data = ""
            });
        }

        // History
        private void BtnHistory_Click(object sender, EventArgs e)
        {
            buttonHistory.Click += (s, e) =>
            {
                new HistoryForm(username, socket).Show();
            };
        }

        // Logout
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            UIHelper.SwitchForm(this, new LoginForm());
        }

        // ================= DESIGN (GIỮ NGUYÊN) =================
        private void InitializeComponent()
        {
            panel2 = new Panel();
            label2 = new Label();
            label1 = new Label();
            panel1 = new Panel();
            labelStatus = new Label();
            label3 = new Label();
            listBoxPlayer = new ListBox();
            panel3 = new Panel();
            label4 = new Label();
            buttonLogout = new Button();
            buttonHistory = new Button();
            buttonRefresh = new Button();
            buttonChallenge = new Button();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();

            panel2.Controls.Add(label2);
            panel2.Controls.Add(label1);
            panel2.Location = new System.Drawing.Point(12, 12);
            panel2.Size = new System.Drawing.Size(519, 109);

            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Forte", 22.2F);
            label2.ForeColor = System.Drawing.Color.Firebrick;
            label2.Location = new System.Drawing.Point(207, 57);
            label2.Text = "Lobby";

            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Showcard Gothic", 22.2F);
            label1.Location = new System.Drawing.Point(143, 11);
            label1.Text = "Game Caro";

            panel1.Controls.Add(labelStatus);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(listBoxPlayer);
            panel1.Location = new System.Drawing.Point(12, 127);
            panel1.Size = new System.Drawing.Size(238, 241);

            labelStatus.Location = new System.Drawing.Point(15, 30);
            labelStatus.Text = "Status:";

            label3.Location = new System.Drawing.Point(15, 10);
            label3.Text = "Player List";

            listBoxPlayer.Location = new System.Drawing.Point(15, 64);
            listBoxPlayer.Size = new System.Drawing.Size(208, 144);

            panel3.Controls.Add(label4);
            panel3.Controls.Add(buttonLogout);
            panel3.Controls.Add(buttonHistory);
            panel3.Controls.Add(buttonRefresh);
            panel3.Controls.Add(buttonChallenge);
            panel3.Location = new System.Drawing.Point(293, 127);
            panel3.Size = new System.Drawing.Size(238, 241);

            label4.Location = new System.Drawing.Point(25, 10);
            label4.Text = "Actions";

            buttonChallenge.Text = "Challenge";
            buttonChallenge.Location = new System.Drawing.Point(25, 61);

            buttonRefresh.Text = "Refresh";
            buttonRefresh.Location = new System.Drawing.Point(25, 96);

            buttonHistory.Text = "History";
            buttonHistory.Location = new System.Drawing.Point(25, 131);

            buttonLogout.Text = "Logout";
            buttonLogout.Location = new System.Drawing.Point(25, 166);

            Controls.Add(panel3);
            Controls.Add(panel1);
            Controls.Add(panel2);

            Text = "Game Caro - Lobby";
            ClientSize = new System.Drawing.Size(543, 429);

            ResumeLayout(false);
        }
    }
}