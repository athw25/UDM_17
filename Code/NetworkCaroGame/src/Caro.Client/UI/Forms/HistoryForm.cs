//using System.Windows.Forms; namespace Caro.Client.UI.Forms { public class HistoryForm : Form { } }
using Caro.Client.Network;
using Caro.Client.UI.Services;
using Caro.Shared.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace Caro.Client.UI.Forms
{
    public class HistoryForm : Form
    {

        private List<MatchHistory> allData = new List<MatchHistory>();

        private ClientSocket socket;
        private string username;

        public HistoryForm(string username, ClientSocket socket)
        {
            InitializeComponent();

            this.username = username;
            this.socket = socket;

            comboBoxResult.Items.AddRange(new[] { "All", "Win", "Lose" });
            comboBoxResult.SelectedIndex = 0;

            textBoxSearch.TextChanged += (s, e) => ApplyFilter();
            comboBoxResult.SelectedIndexChanged += (s, e) => ApplyFilter();
            buttonRefresh.Click += async (s, e) => await LoadData();
            buttonClose.Click += (s, e) => this.Close();

            foreach (DataGridViewColumn col in dataGridViewList.Columns)
                col.Visible = true;

            this.Shown += async (s, e) => await LoadData();
            socket.OnReceive += HandleSocket;

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            socket.OnReceive -= HandleSocket;
            base.OnFormClosed(e);
        }

        private DateTime lastReload = DateTime.MinValue;

        private void HandleSocket(Packet packet)
        {
            if (packet.Command == CommandType.Win
                || packet.Command == CommandType.MatchFinished)
            {
                if ((DateTime.Now - lastReload).TotalSeconds < 1)
                    return;

                lastReload = DateTime.Now;

                if (InvokeRequired)
                    Invoke(new Action(async () => await LoadData()));
                else
                    _ = LoadData();
            }
        }

        // ================= LOAD DATA =================
        private bool isLoading = false;

        private async Task LoadData()
        {
            if (isLoading) return;

            isLoading = true;
            try
            {
                var data = await socket.GetHistory(username);
                allData = data ?? new List<MatchHistory>();
                ApplyFilter();
            }
            finally
            {
                isLoading = false;
            }
        }

        // ================= FILTER =================
        private void ApplyFilter()
        {
            var filtered = allData;

            // Filter username
            if (!string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                filtered = filtered
                    .Where(x =>
                    {
                        string opponent = (x.Player1 == username ? x.Player2 : x.Player1) ?? "Unknown";
                        return opponent.IndexOf(textBoxSearch.Text,
                            StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToList();
            }

            // Filter result
            if (comboBoxResult.Text == "Win")
                filtered = filtered.Where(x => x.Winner == username).ToList();
            else if (comboBoxResult.Text == "Lose")
                filtered = filtered.Where(x => x.Winner != username).ToList();

            // LUÔN SORT
            filtered = filtered
                .OrderByDescending(x => x.Time)
                .ToList();

            BindData(filtered);
        }

        // ================= BIND =================
        private void BindData(List<MatchHistory> data)
        {
            dataGridViewList.Rows.Clear();

            foreach (var m in data)
            {
                string opponent = (m.Player1 == username ? m.Player2 : m.Player1) ?? "Unknown";
                string result;

                if (string.IsNullOrEmpty(m.Winner))
                    result = "Unknown";
                else if (m.Winner == username)
                    result = "Win";
                else
                    result = "Lose";

                int row = dataGridViewList.Rows.Add(
                    m.Time.ToString("HH:mm dd/MM"),
                    opponent,
                    result
                );

                dataGridViewList.Rows[row].DefaultCellStyle.ForeColor =
                    result == "Win" ? Color.Green : Color.Red;
            }

            dataGridViewList.ClearSelection();

            UpdateStats(data);
            if (data.Count == 0)
            {
                labelMatches.Text = "No matches found";
                labelWinRate.Text = "";
                return;
            }
        }

        // ================= STATS =================
        private void UpdateStats(List<MatchHistory> data)
        {
            int total = data.Count;
            int win = data.Count(x => x.Winner == username);
            labelMatches.Text = $"Total matches: {total}";
            labelWinRate.Text = $"Win rate: {(total == 0 ? 0 : win * 100 / total)}%";
        }      

        private void label2_Click(object sender, System.EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        private void label3_Click(object sender, System.EventArgs e)
        {

        }

        private void button1_Click(object sender, System.EventArgs e)
        {

        }

        private void label5_Click(object sender, System.EventArgs e)
        {

        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            panel2 = new Panel();
            label2 = new Label();
            label1 = new Label();
            comboBoxResult = new ComboBox();
            panel1 = new Panel();
            textBoxSearch = new TextBox();
            label3 = new Label();
            label4 = new Label();
            buttonRefresh = new Button();
            dataGridViewList = new DataGridView();
            Time = new DataGridViewTextBoxColumn();
            Opponent = new DataGridViewTextBoxColumn();
            Result = new DataGridViewTextBoxColumn();
            panel3 = new Panel();
            labelMatches = new Label();
            labelWinRate = new Label();
            buttonClose = new Button();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)dataGridViewList).BeginInit();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(label2);
            panel2.Controls.Add(label1);
            panel2.Location = new System.Drawing.Point(43, 12);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(850, 109);
            panel2.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Forte", 22.2F);
            label2.ForeColor = System.Drawing.Color.Firebrick;
            label2.Location = new System.Drawing.Point(319, 55);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(252, 41);
            label2.TabIndex = 0;
            label2.Text = "Match History";
            label2.Click += label2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Showcard Gothic", 22.2F);
            label1.Location = new System.Drawing.Point(330, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(223, 46);
            label1.TabIndex = 1;
            label1.Text = "Game Caro";
            // 
            // comboBoxResult
            // 
            comboBoxResult.FormattingEnabled = true;
            comboBoxResult.Location = new System.Drawing.Point(3, 88);
            comboBoxResult.Name = "comboBoxResult";
            comboBoxResult.Size = new System.Drawing.Size(235, 28);
            comboBoxResult.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonRefresh);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(textBoxSearch);
            panel1.Controls.Add(comboBoxResult);
            panel1.Location = new System.Drawing.Point(43, 151);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(250, 171);
            panel1.TabIndex = 5;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Location = new System.Drawing.Point(3, 33);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new System.Drawing.Size(235, 27);
            textBoxSearch.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(126, 20);
            label3.TabIndex = 6;
            label3.Text = "Search Username:";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(3, 65);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(85, 20);
            label4.TabIndex = 7;
            label4.Text = "Filter result:";
            // 
            // buttonRefresh
            // 
            buttonRefresh.Location = new System.Drawing.Point(3, 139);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new System.Drawing.Size(94, 29);
            buttonRefresh.TabIndex = 8;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // dataGridViewList
            // 
            dataGridViewList.AllowUserToAddRows = false;
            dataGridViewList.AllowUserToDeleteRows = false;
            dataGridViewList.AllowUserToOrderColumns = true;
            dataGridViewList.AllowUserToResizeColumns = false;
            dataGridViewList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewList.BackgroundColor = System.Drawing.Color.White;
            dataGridViewList.BorderStyle = BorderStyle.None;
            dataGridViewList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridViewList.EnableHeadersVisualStyles = false;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridViewList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewList.Columns.AddRange(new DataGridViewColumn[] { Time, Opponent, Result });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridViewList.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewList.Location = new System.Drawing.Point(362, 134);
            dataGridViewList.Name = "dataGridViewList";
            dataGridViewList.ReadOnly = true;
            dataGridViewList.RowHeadersVisible = false;
            dataGridViewList.RowHeadersWidth = 51;
            dataGridViewList.Size = new System.Drawing.Size(531, 315);
            dataGridViewList.TabIndex = 6;
            // 
            // Time
            // 
            Time.HeaderText = "Time";
            Time.MinimumWidth = 6;
            Time.Name = "Time";
            Time.ReadOnly = true;
            Time.Visible = false;
            // 
            // Opponent
            // 
            Opponent.HeaderText = "Opponent";
            Opponent.MinimumWidth = 6;
            Opponent.Name = "Opponent";
            Opponent.ReadOnly = true;
            Opponent.Visible = false;
            // 
            // Result
            // 
            Result.HeaderText = "Result";
            Result.MinimumWidth = 6;
            Result.Name = "Result";
            Result.ReadOnly = true;
            Result.Visible = false;
            // 
            // panel3
            // 
            panel3.Controls.Add(buttonClose);
            panel3.Controls.Add(labelWinRate);
            panel3.Controls.Add(labelMatches);
            panel3.Location = new System.Drawing.Point(43, 328);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(250, 125);
            panel3.TabIndex = 7;
            // 
            // labelMatches
            // 
            labelMatches.AutoSize = true;
            labelMatches.Location = new System.Drawing.Point(11, 6);
            labelMatches.Name = "labelMatches";
            labelMatches.Size = new System.Drawing.Size(104, 20);
            labelMatches.TabIndex = 0;
            labelMatches.Text = "Total matches:";
            labelMatches.Click += label5_Click;
            // 
            // labelWinRate
            // 
            labelWinRate.AutoSize = true;
            labelWinRate.Location = new System.Drawing.Point(11, 38);
            labelWinRate.Name = "labelWinRate";
            labelWinRate.Size = new System.Drawing.Size(72, 20);
            labelWinRate.TabIndex = 1;
            labelWinRate.Text = "Win rate: ";
            // 
            // buttonClose
            // 
            buttonClose.Location = new System.Drawing.Point(3, 73);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(94, 29);
            buttonClose.TabIndex = 9;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // HistoryForm
            // 
            ClientSize = new System.Drawing.Size(910, 461);
            Controls.Add(panel3);
            Controls.Add(dataGridViewList);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Name = "HistoryForm";
            Text = "Game Caro - Match History";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)dataGridViewList).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);

        }

        private Panel panel2;
        private Label label2;
        private Label label1;
        private ComboBox comboBoxResult;
        private Panel panel1;
        private Label label3;
        private TextBox textBoxSearch;
        private Button buttonRefresh;
        private Label label4;
        private DataGridView dataGridViewList;
        private DataGridViewTextBoxColumn Time;
        private DataGridViewTextBoxColumn Opponent;
        private DataGridViewTextBoxColumn Result;
        private Panel panel3;
        private Label labelMatches;
        private Button buttonClose;
        private Label labelWinRate;
    }
    // ================= MODEL =================
    public class MatchHistory
    {
        public DateTime Time { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Winner { get; set; }
        public string Moves { get; set; }
    }
}

