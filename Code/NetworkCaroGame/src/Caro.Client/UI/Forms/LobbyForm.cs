using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Caro.Client.Network;
using Caro.Shared.Models;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.UI.Forms
{
    public class LobbyForm : Form
    {
        private ListBox lbPlayers;
        private Button btnChallenge;
        private string _playerName;
        private List<PlayerInfo> _players = new List<PlayerInfo>();

        public LobbyForm(string playerName)
        {
            _playerName = playerName;
            InitializeComponent();
            ClientSocket.Instance.OnPacketReceived += OnPacketReceived;
            FormClosed += (s, e) => Environment.Exit(0);
        }

        private void InitializeComponent()
        {
            Text = $"Caro Lobby - {_playerName}";
            Size = new Size(400, 400);
            StartPosition = FormStartPosition.CenterScreen;

            lbPlayers = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(340, 250),
                DisplayMember = "Name"
            };

            btnChallenge = new Button
            {
                Text = "Challenge Player",
                Location = new Point(140, 290),
                Size = new Size(120, 35)
            };
            btnChallenge.Click += BtnChallenge_Click;

            Controls.Add(lbPlayers);
            Controls.Add(btnChallenge);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClientSocket.Instance.Send(new Packet { Command = CommandType.GetPlayers });
        }

        private class PlayerItem
        {
            public PlayerInfo Original { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }

        private void BtnChallenge_Click(object sender, EventArgs e)
        {
            if (lbPlayers.SelectedItem is PlayerItem item)
            {
                var selected = item.Original;
                if (selected.Name == _playerName)
                {
                    MessageBox.Show("You cannot challenge yourself.");
                    return;
                }
                if (selected.IsPlaying)
                {
                    MessageBox.Show("This player is currently in a match.");
                    return;
                }
                
                var packet = new Packet { Command = CommandType.ChallengeRequest, Payload = Serializer.Serialize(selected.Id) };
                ClientSocket.Instance.Send(packet);
                MessageBox.Show("Challenge sent! Waiting for response...");
            }
        }

        private void OnPacketReceived(Packet packet)
        {
            Invoke(new Action(() =>
            {
                switch (packet.Command)
                {
                    case CommandType.UpdatePlayerList:
                        _players = Serializer.Deserialize<List<PlayerInfo>>(packet.Payload);
                        UpdatePlayerList();
                        break;
                    
                    case CommandType.ChallengeRequest:
                        var req = Serializer.Deserialize<ChallengeInfo>(packet.Payload);
                        var challenger = _players.FirstOrDefault(p => p.Id == req.ChallengerId);
                        string pName = challenger?.Name ?? "Unknown";
                        
                        var result = MessageBox.Show($"Player {pName} has challenged you. Accept?", "Challenge", MessageBoxButtons.YesNo);
                        req.IsAccepted = result == DialogResult.Yes;
                        ClientSocket.Instance.Send(new Packet { Command = CommandType.ChallengeResponse, Payload = Serializer.Serialize(req) });
                        break;
                        
                    case CommandType.ChallengeResponse:
                        var res = Serializer.Deserialize<ChallengeInfo>(packet.Payload);
                        if (!res.IsAccepted)
                        {
                            MessageBox.Show("Your challenge was declined.");
                        }
                        break;
                        
                    case CommandType.StartGame:
                        // Server will broadcast StartGame to both when accepted
                        ClientSocket.Instance.OnPacketReceived -= OnPacketReceived;
                        string turnStr = Serializer.Deserialize<string>(packet.Payload);
                        bool isMyTurn = turnStr == "1";
                        var game = new GameForm(_playerName, isMyTurn);
                        game.Show();
                        Hide();
                        break;
                }
            }));
        }

        private void UpdatePlayerList()
        {
            lbPlayers.Items.Clear();
            foreach (var p in _players)
            {
                string status = p.IsPlaying ? "[In Game]" : "[Lobby]";
                lbPlayers.Items.Add(new PlayerItem { Original = p, Name = $"{p.Name} {status}" });
            }
        }
    }
}