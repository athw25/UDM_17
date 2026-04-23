using Caro.Client.Network;
using Caro.Client.UI.Components;
using Caro.Client.UI.Helpers;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;
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

            InitializeComponent(); 

            buttonChallenge.Enabled = false;
            listBoxPlayer.SelectedIndexChanged += (s, e) =>
            {
                buttonChallenge.Enabled = listBoxPlayer.SelectedItem != null;
            };

            buttonChallenge.Click += BtnChallenge_Click;
            buttonRefresh.Click += BtnRefresh_Click;
            buttonHistory.Click += BtnHistory_Click;
            buttonLogout.Click += BtnLogout_Click;

            this.socket.OnReceive += HandlePacket;
            
            // Handle form closing
            this.FormClosing += LobbyForm_FormClosing;
        }

        // ================= FORM CLOSING =================
        private void LobbyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Unsubscribe from socket events
                if (socket != null)
                {
                    socket.OnReceive -= HandlePacket;
                }
            }
            catch { }
        }

        // ================= HANDLE PACKET =================
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
                    case CommandType.PlayerList:
                    case CommandType.UpdatePlayerList:
                        {
                            var players = Serializer.Deserialize<List<PlayerInfo>>(packet.Payload);
                            if (players == null) break;

                            listBoxPlayer.DataSource = players
                                .Where(p => p.Name != username)
                                .Select(p => p.Name)
                                .ToList();
                            
                            UpdateStatus();
                            break;
                        }

                    case CommandType.Challenge:
                        {
                            string fromUser = packet.Data;

                            var result = MessageBox.Show(
                                $"{fromUser} mời bạn chơi",
                                "Mời chơi",
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

                            buttonChallenge.Enabled = listBoxPlayer.SelectedItem != null;
                            break;
                        }

                    case CommandType.Reject:
                        {
                            StyledMessageBox.Show($"{packet.Data} đã từ chối lời mời!");
                            buttonChallenge.Enabled = listBoxPlayer.SelectedItem != null;
                            break;
                        }

                    case CommandType.LoginFailed:
                        {
                            StyledMessageBox.Error(packet.Data);
                            break;
                        }

                    case CommandType.DuplicateUsername:
                        {
                            StyledMessageBox.Error(packet.Data);
                            UIHelper.SwitchForm(this, new LoginForm());
                            break;
                        }

                    case CommandType.InvalidInput:
                        {
                            StyledMessageBox.Error(packet.Data);
                            break;
                        }

                    case CommandType.StartGame:
                        {
                            string opponent = packet.Data;
                            bool isHost = packet.Payload == "1";

                            // Create GameForm without closing LobbyForm yet
                            GameForm gameForm = new GameForm(username, opponent, socket, isHost);
                            gameForm.FormClosed += (s, e) =>
                            {
                                // When game ends, show lobby again
                                if (!this.IsDisposed)
                                {
                                    this.Show();
                                    this.BringToFront();
                                    this.Focus();
                                    
                                    // Request player list update
                                    socket.Send(new Packet
                                    {
                                        Command = CommandType.GetPlayers,
                                        Payload = ""
                                    });
                                }
                            };

                            this.Hide();
                            gameForm.Show();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                StyledMessageBox.Error($"Lỗi: {ex.Message}");
            }
        }

        // ================= UPDATE UI =================
        private void UpdateStatus()
        {
            int count = listBoxPlayer.Items.Count;              
            labelStatus.Text = $"Online: {count} người chơi";
        }

        // ================= EVENTS =================
        private void BtnChallenge_Click(object sender, EventArgs e)
        {
            if (listBoxPlayer.SelectedItem == null)
            {
                StyledMessageBox.Show("Vui lòng chọn một người chơi");
                return;
            }

            string opponent = listBoxPlayer.SelectedItem.ToString();

            socket.Send(new Packet
            {
                Command = CommandType.Challenge,
                Data = opponent
            });
            
            buttonChallenge.Enabled = false;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            socket.Send(new Packet
            {
                Command = CommandType.GetPlayers,
                Payload = ""
            });
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            new HistoryForm(username, socket).Show();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            UIHelper.SwitchForm(this, new LoginForm());
        }

        // ================= DESIGN =================
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LobbyForm));
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
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(519, 109);
            panel2.TabIndex = 2;
            
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Forte", 22.2F);
            label2.ForeColor = System.Drawing.Color.Firebrick;
            label2.Location = new System.Drawing.Point(207, 57);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(105, 41);
            label2.TabIndex = 0;
            label2.Text = "Lobby";
            
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Showcard Gothic", 22.2F);
            label1.Location = new System.Drawing.Point(143, 11);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(223, 46);
            label1.TabIndex = 1;
            label1.Text = "Game Caro";
            
            panel1.Controls.Add(labelStatus);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(listBoxPlayer);
            panel1.Location = new System.Drawing.Point(12, 127);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(238, 241);
            panel1.TabIndex = 1;
            
            labelStatus.Location = new System.Drawing.Point(15, 30);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(200, 23);
            labelStatus.TabIndex = 0;
            labelStatus.Text = "Status:";
            
            label3.Location = new System.Drawing.Point(15, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 23);
            label3.TabIndex = 1;
            label3.Text = "Danh sách";
            
            listBoxPlayer.Location = new System.Drawing.Point(15, 64);
            listBoxPlayer.Name = "listBoxPlayer";
            listBoxPlayer.Size = new System.Drawing.Size(208, 144);
            listBoxPlayer.TabIndex = 2;
            
            panel3.Controls.Add(label4);
            panel3.Controls.Add(buttonLogout);
            panel3.Controls.Add(buttonHistory);
            panel3.Controls.Add(buttonRefresh);
            panel3.Controls.Add(buttonChallenge);
            panel3.Location = new System.Drawing.Point(293, 127);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(238, 241);
            panel3.TabIndex = 0;
            
            label4.Location = new System.Drawing.Point(25, 10);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(100, 23);
            label4.TabIndex = 0;
            label4.Text = "Hành động";
            
            buttonLogout.Location = new System.Drawing.Point(25, 166);
            buttonLogout.Name = "buttonLogout";
            buttonLogout.Size = new System.Drawing.Size(173, 29);
            buttonLogout.TabIndex = 1;
            buttonLogout.Text = "Logout";
            buttonLogout.UseVisualStyleBackColor = true;
            
            buttonHistory.Location = new System.Drawing.Point(25, 131);
            buttonHistory.Name = "buttonHistory";
            buttonHistory.Size = new System.Drawing.Size(173, 29);
            buttonHistory.TabIndex = 0;
            buttonHistory.Text = "Match History";
            buttonHistory.UseVisualStyleBackColor = true;
            
            buttonRefresh.Location = new System.Drawing.Point(25, 96);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new System.Drawing.Size(173, 29);
            buttonRefresh.TabIndex = 1;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            
            buttonChallenge.Location = new System.Drawing.Point(25, 61);
            buttonChallenge.Name = "buttonChallenge";
            buttonChallenge.Size = new System.Drawing.Size(173, 29);
            buttonChallenge.TabIndex = 2;
            buttonChallenge.Text = "Challenge";
            buttonChallenge.UseVisualStyleBackColor = true;
            
            ClientSize = new System.Drawing.Size(543, 381);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "LobbyForm";
            Text = "Game Caro - Lobby";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }
    }
}

