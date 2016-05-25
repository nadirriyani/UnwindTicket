namespace UnwindTicket
{
    partial class OpenIdeas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenIdeas));
            this.grvLiveIdea = new System.Windows.Forms.DataGridView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.btnLog = new System.Windows.Forms.ToolStripMenuItem();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.notifySLTP = new System.Windows.Forms.NotifyIcon(this.components);
            this.pictBBconnection = new System.Windows.Forms.PictureBox();
            this.lblBBmessage = new System.Windows.Forms.Label();
            this.mnuVersion = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.grvLiveIdea)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictBBconnection)).BeginInit();
            this.SuspendLayout();
            // 
            // grvLiveIdea
            // 
            this.grvLiveIdea.AllowUserToAddRows = false;
            this.grvLiveIdea.AllowUserToResizeRows = false;
            this.grvLiveIdea.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.grvLiveIdea.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grvLiveIdea.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grvLiveIdea.ColumnHeadersHeight = 25;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grvLiveIdea.DefaultCellStyle = dataGridViewCellStyle2;
            this.grvLiveIdea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grvLiveIdea.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.grvLiveIdea.Location = new System.Drawing.Point(0, 24);
            this.grvLiveIdea.Name = "grvLiveIdea";
            this.grvLiveIdea.RowHeadersVisible = false;
            this.grvLiveIdea.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grvLiveIdea.Size = new System.Drawing.Size(992, 762);
            this.grvLiveIdea.TabIndex = 7;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(992, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.btnLog,
            this.mnuVersion});
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(152, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnLog
            // 
            this.btnLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.btnLog.ForeColor = System.Drawing.Color.White;
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(152, 22);
            this.btnLog.Text = "Log";
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // notifySLTP
            // 
            this.notifySLTP.BalloonTipText = "Blotter SL & TP";
            this.notifySLTP.Icon = ((System.Drawing.Icon)(resources.GetObject("notifySLTP.Icon")));
            this.notifySLTP.Text = "Blotter SL & TP";
            this.notifySLTP.Visible = true;
            this.notifySLTP.Click += new System.EventHandler(this.notifySLTP_Click);
            // 
            // pictBBconnection
            // 
            this.pictBBconnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictBBconnection.BackColor = System.Drawing.Color.Transparent;
            this.pictBBconnection.Image = global::UnwindTicket.Properties.Resources.blank16;
            this.pictBBconnection.Location = new System.Drawing.Point(876, 2);
            this.pictBBconnection.Name = "pictBBconnection";
            this.pictBBconnection.Size = new System.Drawing.Size(27, 23);
            this.pictBBconnection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictBBconnection.TabIndex = 9;
            this.pictBBconnection.TabStop = false;
            // 
            // lblBBmessage
            // 
            this.lblBBmessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBBmessage.AutoSize = true;
            this.lblBBmessage.BackColor = System.Drawing.Color.Black;
            this.lblBBmessage.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBBmessage.ForeColor = System.Drawing.Color.White;
            this.lblBBmessage.Location = new System.Drawing.Point(906, 6);
            this.lblBBmessage.Name = "lblBBmessage";
            this.lblBBmessage.Size = new System.Drawing.Size(68, 13);
            this.lblBBmessage.TabIndex = 10;
            this.lblBBmessage.Text = "Bloomberg";
            // 
            // mnuVersion
            // 
            this.mnuVersion.BackColor = System.Drawing.Color.Black;
            this.mnuVersion.ForeColor = System.Drawing.Color.White;
            this.mnuVersion.Name = "mnuVersion";
            this.mnuVersion.Size = new System.Drawing.Size(152, 22);
            this.mnuVersion.Text = "Version";
            this.mnuVersion.Click += new System.EventHandler(this.mnuVersion_Click);
            // 
            // OpenIdeas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(992, 786);
            this.Controls.Add(this.lblBBmessage);
            this.Controls.Add(this.pictBBconnection);
            this.Controls.Add(this.grvLiveIdea);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpenIdeas";
            this.Text = "Blotter SL & TP";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.OpenIdeas_Load);
            this.Resize += new System.EventHandler(this.OpenIdeas_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.grvLiveIdea)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictBBconnection)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grvLiveIdea;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem btnRefresh;
        private System.Windows.Forms.ToolStripMenuItem btnLog;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.NotifyIcon notifySLTP;
        private System.Windows.Forms.PictureBox pictBBconnection;
        private System.Windows.Forms.Label lblBBmessage;
        private System.Windows.Forms.ToolStripMenuItem mnuVersion;

    }
}