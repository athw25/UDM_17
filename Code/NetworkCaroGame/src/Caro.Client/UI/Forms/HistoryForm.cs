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
using System.Threading;
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
            {
                col.Visible = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

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
                {
                    Invoke(new Action(() =>
                    {
                        _ = ReloadSafe();
                    }));
                }
                else
                {
                    _ = ReloadSafe();
                }
            }
        }

        private CancellationTokenSource reloadCts;

        private async Task ReloadSafe()
        {
            reloadCts?.Cancel();
            reloadCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(200, reloadCts.Token); // debounce nhẹ
                await LoadData();
            }
            catch (TaskCanceledException) { }
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

                if (InvokeRequired)
                    Invoke(new Action(() => ApplyFilter()));
                else
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

                if (result == "Win")
                    dataGridViewList.Rows[row].DefaultCellStyle.ForeColor = Color.Green;
                else if (result == "Lose")
                    dataGridViewList.Rows[row].DefaultCellStyle.ForeColor = Color.Red;
                else
                    dataGridViewList.Rows[row].DefaultCellStyle.ForeColor = Color.Gray;

                dataGridViewList.ClearSelection();

                UpdateStats(data);
                if (data.Count == 0)
                {
                    labelMatches.Text = "No matches found";
                    labelWinRate.Text = "";
                    return;
                }
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(HistoryForm));
            panel2 = new Panel();
            label2 = new Label();
            label1 = new Label();
            comboBoxResult = new ComboBox();
            panel1 = new Panel();
            buttonRefresh = new Button();
            label4 = new Label();
            label3 = new Label();
            textBoxSearch = new TextBox();
            dataGridViewList = new DataGridView();
            Time = new DataGridViewTextBoxColumn();
            Opponent = new DataGridViewTextBoxColumn();
            Result = new DataGridViewTextBoxColumn();
            panel3 = new Panel();
            buttonClose = new Button();
            labelWinRate = new Label();
            labelMatches = new Label();
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
            panel2.Location = new Point(43, 12);
            panel2.Name = "panel2";
            panel2.Size = new Size(850, 109);
            panel2.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Forte", 22.2F);
            label2.ForeColor = Color.Firebrick;
            label2.Location = new Point(319, 55);
            label2.Name = "label2";
            label2.Size = new Size(252, 41);
            label2.TabIndex = 0;
            label2.Text = "Match History";
            label2.Click += label2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Showcard Gothic", 22.2F);
            label1.Location = new Point(330, 9);
            label1.Name = "label1";
            label1.Size = new Size(223, 46);
            label1.TabIndex = 1;
            label1.Text = "Game Caro";
            // 
            // comboBoxResult
            // 
            comboBoxResult.FormattingEnabled = true;
            comboBoxResult.Location = new Point(3, 88);
            comboBoxResult.Name = "comboBoxResult";
            comboBoxResult.Size = new Size(235, 28);
            comboBoxResult.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(buttonRefresh);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(textBoxSearch);
            panel1.Controls.Add(comboBoxResult);
            panel1.Location = new Point(43, 151);
            panel1.Name = "panel1";
            panel1.Size = new Size(250, 171);
            panel1.TabIndex = 5;
            // 
            // buttonRefresh
            // 
            buttonRefresh.Location = new Point(3, 139);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new Size(94, 29);
            buttonRefresh.TabIndex = 8;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(3, 65);
            label4.Name = "label4";
            label4.Size = new Size(85, 20);
            label4.TabIndex = 7;
            label4.Text = "Filter result:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 10);
            label3.Name = "label3";
            label3.Size = new Size(126, 20);
            label3.TabIndex = 6;
            label3.Text = "Search Username:";
            label3.Click += label3_Click;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Location = new Point(3, 33);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(235, 27);
            textBoxSearch.TabIndex = 5;
            // 
            // dataGridViewList
            // 
            dataGridViewList.AllowUserToAddRows = false;
            dataGridViewList.AllowUserToDeleteRows = false;
            dataGridViewList.AllowUserToOrderColumns = true;
            dataGridViewList.AllowUserToResizeColumns = false;
            dataGridViewList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewList.BackgroundColor = Color.White;
            dataGridViewList.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridViewList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewList.Columns.AddRange(new DataGridViewColumn[] { Time, Opponent, Result });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridViewList.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewList.EnableHeadersVisualStyles = false;
            dataGridViewList.Location = new Point(362, 134);
            dataGridViewList.Name = "dataGridViewList";
            dataGridViewList.ReadOnly = true;
            dataGridViewList.RowHeadersVisible = false;
            dataGridViewList.RowHeadersWidth = 51;
            dataGridViewList.Size = new Size(531, 315);
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
            panel3.Location = new Point(43, 328);
            panel3.Name = "panel3";
            panel3.Size = new Size(250, 125);
            panel3.TabIndex = 7;
            // 
            // buttonClose
            // 
            buttonClose.Location = new Point(3, 73);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(94, 29);
            buttonClose.TabIndex = 9;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // labelWinRate
            // 
            labelWinRate.AutoSize = true;
            labelWinRate.Location = new Point(11, 38);
            labelWinRate.Name = "labelWinRate";
            labelWinRate.Size = new Size(72, 20);
            labelWinRate.TabIndex = 1;
            labelWinRate.Text = "Win rate: ";
            // 
            // labelMatches
            // 
            labelMatches.AutoSize = true;
            labelMatches.Location = new Point(11, 6);
            labelMatches.Name = "labelMatches";
            labelMatches.Size = new Size(104, 20);
            labelMatches.TabIndex = 0;
            labelMatches.Text = "Total matches:";
            labelMatches.Click += label5_Click;
            // 
            // HistoryForm
            // 
            ClientSize = new Size(910, 461);
            Controls.Add(panel3);
            Controls.Add(dataGridViewList);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Icon = (Icon)resources.GetObject("$this.Icon");
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

