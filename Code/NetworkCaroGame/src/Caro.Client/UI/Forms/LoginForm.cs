using System;
using System.Drawing;
using System.Windows.Forms;
using Caro.Client.Network;
using Caro.Shared.Network;
using Caro.Shared.Utils;

namespace Caro.Client.UI.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtName;
        private TextBox txtIp;
        private Button btnConnect;

        public LoginForm()
        {
            InitializeComponent();
            ClientSocket.Instance.OnPacketReceived += OnPacketReceived;
        }

        private void InitializeComponent()
        {
            Text = "Caro Network - Login";
            Size = new Size(300, 200);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblName = new Label { Text = "Player Name:", Location = new Point(20, 20), AutoSize = true };
            txtName = new TextBox { Location = new Point(100, 20), Width = 150 };

            Label lblIp = new Label { Text = "Server IP:", Location = new Point(20, 60), AutoSize = true };
            txtIp = new TextBox { Text = "127.0.0.1", Location = new Point(100, 60), Width = 150 };

            btnConnect = new Button { Text = "Connect", Location = new Point(100, 100), Width = 100 };
            btnConnect.Click += BtnConnect_Click;

            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblIp);
            Controls.Add(txtIp);
            Controls.Add(btnConnect);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a name.");
                return;
            }

            btnConnect.Enabled = false;
            
            if (ClientSocket.Instance.Connect(txtIp.Text, 8888))
            {
                var packet = new Packet { Command = CommandType.Login, Payload = Serializer.Serialize(txtName.Text) };
                ClientSocket.Instance.Send(packet);
            }
            else
            {
                MessageBox.Show("Failed to connect to server.");
                btnConnect.Enabled = true;
            }
        }

        private void OnPacketReceived(Packet packet)
        {
            if (packet.Command == CommandType.LoginSuccess)
            {
                ClientSocket.Instance.OnPacketReceived -= OnPacketReceived;
                Invoke(new Action(() =>
                {
                    var lobby = new LobbyForm(txtName.Text);
                    lobby.Show();
                    Hide();
                }));
            }
        }
    }
}
