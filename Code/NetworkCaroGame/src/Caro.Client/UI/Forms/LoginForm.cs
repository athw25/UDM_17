
using Caro.Client.Network;
using Caro.Client.UI.Components;
using Caro.Client.UI.Helpers;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Caro.Client.UI.Forms
{
    public class LoginForm : Form
    {
        private Panel panel1;
        private PictureBox pictureBox1;
        private TextBox textBoxServer;
        private TextBox textBoxPort;
        private Panel panel2;
        private Panel panel3;
        private Label label1;
        private Button buttonConnect;
        private TextBox textBoxName;
        private Label label2;
        private ClientSocket socket;

        public LoginForm()
        {
            InitializeComponent();

            // Gán event cho button 
            buttonConnect.Click += Connect_Click;

        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(LoginForm));
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            textBoxServer = new TextBox();
            textBoxPort = new TextBox();
            panel2 = new Panel();
            label2 = new Label();
            label1 = new Label();
            panel3 = new Panel();
            buttonConnect = new Button();
            textBoxName = new TextBox();
            panel1.SuspendLayout();
            ((ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new System.Drawing.Point(259, 4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(409, 419);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBox1.Image = Properties.Resources._9a91e11d31b376abcc3b8f28cec9414b;
            pictureBox1.Location = new System.Drawing.Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(403, 408);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // textBoxServer
            // 
            textBoxServer.Location = new System.Drawing.Point(3, 3);
            textBoxServer.Name = "textBoxServer";
            textBoxServer.Size = new System.Drawing.Size(232, 27);
            textBoxServer.TabIndex = 1;
            textBoxServer.Text = "127.0.0.1"; textBoxServer.Enabled = false;
            textBoxServer.TextChanged += textBoxServer_TextChanged;
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new System.Drawing.Point(3, 36);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new System.Drawing.Size(232, 27);
            textBoxPort.TabIndex = 2;
            textBoxPort.Text = "8888"; textBoxPort.Enabled = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(label2);
            panel2.Controls.Add(label1);
            panel2.Location = new System.Drawing.Point(3, 4);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(238, 187);
            panel2.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Forte", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.ForeColor = System.Drawing.Color.Firebrick;
            label2.Location = new System.Drawing.Point(67, 116);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 41);
            label2.TabIndex = 1;
            label2.Text = "Login";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Showcard Gothic", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.Location = new System.Drawing.Point(15, 61);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(223, 46);
            label1.TabIndex = 0;
            label1.Text = "Game Caro";
            label1.Click += label1_Click;
            // 
            // panel3
            // 
            panel3.Controls.Add(buttonConnect);
            panel3.Controls.Add(textBoxName);
            panel3.Controls.Add(textBoxServer);
            panel3.Controls.Add(textBoxPort);
            panel3.Location = new System.Drawing.Point(3, 197);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(238, 215);
            panel3.TabIndex = 4;
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new System.Drawing.Point(67, 121);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new System.Drawing.Size(94, 29);
            buttonConnect.TabIndex = 4;
            buttonConnect.Text = "Connect";
            buttonConnect.UseVisualStyleBackColor = true;
            // 
            // textBoxName
            // 
            textBoxName.Location = new System.Drawing.Point(3, 70);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new System.Drawing.Size(232, 27);
            textBoxName.TabIndex = 3;
            textBoxName.Text = "UserName";
            // 
            // LoginForm
            // 
            ClientSize = new System.Drawing.Size(671, 427);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "LoginForm";
            Text = "Game Caro - Login";
            panel1.ResumeLayout(false);
            ((ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);

        }

        private async void Connect_Click(object sender, EventArgs e)
        {
            string ip = textBoxServer.Text.Trim();
            string portText = textBoxPort.Text.Trim();
            string username = textBoxName.Text.Trim();

            // ===== VALIDATE =====
            if (string.IsNullOrWhiteSpace(ip) || ip == "IP Server")
            {
                StyledMessageBox.Error("Please enter Server IP");
                return;
            }

            if (string.IsNullOrWhiteSpace(portText) || portText == "Port")
            {
                StyledMessageBox.Error("Please enter Port");
                return;
            }

            if (!int.TryParse(portText, out int port))
            {
                StyledMessageBox.Error("Port must be a number");
                return;
            }

            if (string.IsNullOrWhiteSpace(username) || username == "UserName")
            {
                StyledMessageBox.Error("Please enter Username");
                return;
            }

            // ===== CONNECT SERVER =====
            try
            {
                buttonConnect.Enabled = false;

                socket = new ClientSocket();

                await socket.ConnectAsync(ip, port);
                socket.Login(username);
                UIHelper.SwitchForm(this, new LobbyForm(username, socket));
            }
            catch (Exception ex)
            {
                StyledMessageBox.Error("Cannot connect to server\n" + ex.Message);
                buttonConnect.Enabled = true;
            }
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBoxServer_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
