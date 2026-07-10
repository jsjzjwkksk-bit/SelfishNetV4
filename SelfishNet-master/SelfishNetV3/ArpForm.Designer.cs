namespace SelfishNetv3
{
    partial class ArpForm
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
            if (disposing)
            {
                components?.Dispose();
                arpService?.Dispose();
                devices?.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArpForm));

            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnDiscover = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStartSpoof = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStopSpoof = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEmergencyStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();

            this.deviceGridView = new System.Windows.Forms.DataGridView();
            this.ColRole = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColPCName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColPCIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColPCMac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDownload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColUpload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDownCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColUploadCap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColBlock = new System.Windows.Forms.DataGridViewCheckBoxColumn();

            this.contextMenuViews = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ViewMenuIP = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuMAC = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuDownload = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuDownloadCap = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuUploadCap = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuBlock = new System.Windows.Forms.ToolStripMenuItem();

            this.timerStats = new System.Windows.Forms.Timer(this.components);
            this.timerCleanup = new System.Windows.Forms.Timer(this.components);
            this.timerSpoof = new System.Windows.Forms.Timer(this.components);

            this.selfishNetTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.selfishNetTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();

            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deviceGridView)).BeginInit();
            this.contextMenuViews.SuspendLayout();
            this.selfishNetTray.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // ── toolStrip1 ──
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnDiscover,
                this.toolStripSeparator1,
                this.btnStartSpoof,
                this.toolStripSeparator2,
                this.btnStopSpoof,
                this.toolStripSeparator3,
                this.btnEmergencyStop,
                this.toolStripSeparator4
            });
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1028, 41);
            this.toolStrip1.TabIndex = 0;

            // ── btnDiscover ──
            this.btnDiscover.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDiscover.Image = global::SelfishNetv3.Properties.Resources.Network_Map;
            this.btnDiscover.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDiscover.Name = "btnDiscover";
            this.btnDiscover.Size = new System.Drawing.Size(36, 36);
            this.btnDiscover.Text = "Network Discovery";
            this.btnDiscover.Click += new System.EventHandler(this.BtnDiscover_Click);

            // ── toolStripSeparator1 ──
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 41);

            // ── btnStartSpoof ──
            this.btnStartSpoof.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStartSpoof.Image = global::SelfishNetv3.Properties.Resources.Network_ConnectTo;
            this.btnStartSpoof.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStartSpoof.Name = "btnStartSpoof";
            this.btnStartSpoof.Size = new System.Drawing.Size(36, 36);
            this.btnStartSpoof.Text = "Start redirecting / spoofing";
            this.btnStartSpoof.Click += new System.EventHandler(this.BtnStartSpoof_Click);

            // ── toolStripSeparator2 ──
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 41);

            // ── btnStopSpoof ──
            this.btnStopSpoof.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStopSpoof.Image = global::SelfishNetv3.Properties.Resources.DisconnectedDrive;
            this.btnStopSpoof.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStopSpoof.Name = "btnStopSpoof";
            this.btnStopSpoof.Size = new System.Drawing.Size(36, 36);
            this.btnStopSpoof.Text = "Stop redirecting / spoofing";
            this.btnStopSpoof.Click += new System.EventHandler(this.BtnStopSpoof_Click);

            // ── toolStripSeparator3 ──
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 41);

            // ── btnEmergencyStop ──
            this.btnEmergencyStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEmergencyStop.ForeColor = System.Drawing.Color.Red;
            this.btnEmergencyStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEmergencyStop.Name = "btnEmergencyStop";
            this.btnEmergencyStop.Size = new System.Drawing.Size(110, 36);
            this.btnEmergencyStop.Text = "⚠ EMERGENCY STOP";
            this.btnEmergencyStop.ToolTipText = "Immediately stop all spoofing and restore all ARP tables";
            this.btnEmergencyStop.Click += (s, e) => EmergencyStop();

            // ── toolStripSeparator4 ──
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 41);

            // ── deviceGridView ──
            this.deviceGridView.AllowUserToAddRows = false;
            this.deviceGridView.AllowUserToDeleteRows = false;
            this.deviceGridView.Anchor = System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;
            this.deviceGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.deviceGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.deviceGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.deviceGridView.ColumnHeadersHeight = 35;
            this.deviceGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.ColRole,
                this.ColPCName,
                this.ColPCIP,
                this.ColPCMac,
                this.ColDownload,
                this.ColUpload,
                this.ColDownCap,
                this.ColUploadCap,
                this.ColBlock
            });
            this.deviceGridView.ContextMenuStrip = this.contextMenuViews;
            this.deviceGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.deviceGridView.Location = new System.Drawing.Point(0, 44);
            this.deviceGridView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.deviceGridView.Name = "deviceGridView";
            this.deviceGridView.RowHeadersVisible = false;
            this.deviceGridView.RowHeadersWidth = 62;
            this.deviceGridView.ShowCellErrors = false;
            this.deviceGridView.ShowCellToolTips = false;
            this.deviceGridView.ShowEditingIcon = false;
            this.deviceGridView.ShowRowErrors = false;
            this.deviceGridView.Size = new System.Drawing.Size(1028, 556);
            this.deviceGridView.TabIndex = 1;
            this.deviceGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DeviceGridView_CellValueChanged);
            this.deviceGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.DeviceGridView_CurrentCellDirtyStateChanged);

            // ── Columns ──
            this.ColRole.HeaderText = "Role";
            this.ColRole.Name = "ColRole";
            this.ColRole.ReadOnly = true;
            this.ColRole.FillWeight = 60F;
            this.ColRole.MinimumWidth = 50;

            this.ColPCName.HeaderText = "Name";
            this.ColPCName.Name = "ColPCName";
            this.ColPCName.ReadOnly = true;
            this.ColPCName.FillWeight = 150F;
            this.ColPCName.MinimumWidth = 60;

            this.ColPCIP.HeaderText = "IP";
            this.ColPCIP.Name = "ColPCIP";
            this.ColPCIP.ReadOnly = true;
            this.ColPCIP.MinimumWidth = 80;

            this.ColPCMac.HeaderText = "MAC";
            this.ColPCMac.Name = "ColPCMac";
            this.ColPCMac.ReadOnly = true;
            this.ColPCMac.MinimumWidth = 100;

            this.ColDownload.HeaderText = "Download KB/s";
            this.ColDownload.Name = "ColDownload";
            this.ColDownload.ReadOnly = true;
            this.ColDownload.MinimumWidth = 50;

            this.ColUpload.HeaderText = "Upload KB/s";
            this.ColUpload.Name = "ColUpload";
            this.ColUpload.ReadOnly = true;
            this.ColUpload.MinimumWidth = 50;

            this.ColDownCap.HeaderText = "Down Cap (KB/s)";
            this.ColDownCap.Name = "ColDownCap";
            this.ColDownCap.MinimumWidth = 50;

            this.ColUploadCap.HeaderText = "Up Cap (KB/s)";
            this.ColUploadCap.Name = "ColUploadCap";
            this.ColUploadCap.MinimumWidth = 50;

            this.ColBlock.HeaderText = "Block";
            this.ColBlock.Name = "ColBlock";
            this.ColBlock.MinimumWidth = 40;
            this.ColBlock.FillWeight = 40F;

            // ── contextMenuViews ──
            this.contextMenuViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.ViewMenuIP,
                this.ViewMenuMAC,
                this.ViewMenuDownload,
                this.ViewMenuUpload,
                this.ViewMenuDownloadCap,
                this.ViewMenuUploadCap,
                this.ViewMenuBlock
            });
            this.contextMenuViews.Name = "contextMenuViews";
            this.contextMenuViews.Size = new System.Drawing.Size(239, 230);

            this.ViewMenuIP.Checked = true; this.ViewMenuIP.CheckOnClick = true;
            this.ViewMenuIP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuIP.Name = "ViewMenuIP"; this.ViewMenuIP.Text = "IP";
            this.ViewMenuIP.CheckStateChanged += (s, e) => ColPCIP.Visible = ViewMenuIP.Checked;

            this.ViewMenuMAC.Checked = true; this.ViewMenuMAC.CheckOnClick = true;
            this.ViewMenuMAC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuMAC.Name = "ViewMenuMAC"; this.ViewMenuMAC.Text = "MAC Address";
            this.ViewMenuMAC.CheckStateChanged += (s, e) => ColPCMac.Visible = ViewMenuMAC.Checked;

            this.ViewMenuDownload.Checked = true; this.ViewMenuDownload.CheckOnClick = true;
            this.ViewMenuDownload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuDownload.Name = "ViewMenuDownload"; this.ViewMenuDownload.Text = "Download";
            this.ViewMenuDownload.CheckStateChanged += (s, e) => ColDownload.Visible = ViewMenuDownload.Checked;

            this.ViewMenuUpload.Checked = true; this.ViewMenuUpload.CheckOnClick = true;
            this.ViewMenuUpload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuUpload.Name = "ViewMenuUpload"; this.ViewMenuUpload.Text = "Upload";
            this.ViewMenuUpload.CheckStateChanged += (s, e) => ColUpload.Visible = ViewMenuUpload.Checked;

            this.ViewMenuDownloadCap.Checked = true; this.ViewMenuDownloadCap.CheckOnClick = true;
            this.ViewMenuDownloadCap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuDownloadCap.Name = "ViewMenuDownloadCap"; this.ViewMenuDownloadCap.Text = "Down Capacity";
            this.ViewMenuDownloadCap.CheckStateChanged += (s, e) => ColDownCap.Visible = ViewMenuDownloadCap.Checked;

            this.ViewMenuUploadCap.Checked = true; this.ViewMenuUploadCap.CheckOnClick = true;
            this.ViewMenuUploadCap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuUploadCap.Name = "ViewMenuUploadCap"; this.ViewMenuUploadCap.Text = "Upload Capacity";
            this.ViewMenuUploadCap.CheckStateChanged += (s, e) => ColUploadCap.Visible = ViewMenuUploadCap.Checked;

            this.ViewMenuBlock.Checked = true; this.ViewMenuBlock.CheckOnClick = true;
            this.ViewMenuBlock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuBlock.Name = "ViewMenuBlock"; this.ViewMenuBlock.Text = "Block";
            this.ViewMenuBlock.CheckStateChanged += (s, e) => ColBlock.Visible = ViewMenuBlock.Checked;

            // ── Timers ──
            this.timerStats.Tick += new System.EventHandler(this.TimerStats_Tick);
            this.timerCleanup.Tick += new System.EventHandler(this.TimerCleanup_Tick);
            this.timerSpoof.Tick += new System.EventHandler(this.TimerSpoof_Tick);

            // ── selfishNetTrayIcon ──
            this.selfishNetTrayIcon.BalloonTipText = "SelfishNet is minimized";
            this.selfishNetTrayIcon.BalloonTipTitle = "SelfishNet";
            this.selfishNetTrayIcon.ContextMenuStrip = this.selfishNetTray;
            this.selfishNetTrayIcon.Text = "SelfishNet v4";
            this.selfishNetTrayIcon.Visible = true;
            this.selfishNetTrayIcon.MouseDoubleClick += (s, e) => RestoreFromTray();

            // ── selfishNetTray ──
            this.selfishNetTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.showToolStripMenuItem,
                this.exitToolStripMenuItem
            });
            this.selfishNetTray.Name = "selfishNetTray";
            this.selfishNetTray.Size = new System.Drawing.Size(137, 68);

            this.showToolStripMenuItem.Image = global::SelfishNetv3.Properties.Resources._167;
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += (s, e) => RestoreFromTray();

            this.exitToolStripMenuItem.Image = global::SelfishNetv3.Properties.Resources._172;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += (s, e) => Close();

            // ── ArpForm ──
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 630);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.deviceGridView);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ArpForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SelfishNet v4.0 (.NET 8)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ArpForm_FormClosing);
            this.Load += new System.EventHandler(this.ArpForm_Load);
            this.Shown += new System.EventHandler(this.ArpForm_Shown);
            this.Resize += new System.EventHandler(this.ArpForm_Resize);

            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deviceGridView)).EndInit();
            this.contextMenuViews.ResumeLayout(false);
            this.selfishNetTray.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

            // ── statusStrip ──
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.statusLabel
            });
            this.statusStrip.Location = new System.Drawing.Point(0, 605);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1028, 25);
            this.statusStrip.TabIndex = 2;

            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(1013, 20);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusLabel.Text = "Ready — Select an adapter and discover devices";
        }

        #endregion

        // ── Toolbar ──
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnDiscover;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnStartSpoof;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnStopSpoof;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnEmergencyStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;

        // ── DataGridView (replaces TreeGridView) ──
        private System.Windows.Forms.DataGridView deviceGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColRole;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPCName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPCIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPCMac;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDownload;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColUpload;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDownCap;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColUploadCap;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColBlock;

        // ── Context menu ──
        private System.Windows.Forms.ContextMenuStrip contextMenuViews;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuIP;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuMAC;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuDownload;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuUpload;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuDownloadCap;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuUploadCap;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuBlock;

        // ── Timers ──
        private System.Windows.Forms.Timer timerStats;
        private System.Windows.Forms.Timer timerCleanup;
        private System.Windows.Forms.Timer timerSpoof;

        // ── System tray ──
        private System.Windows.Forms.NotifyIcon selfishNetTrayIcon;
        private System.Windows.Forms.ContextMenuStrip selfishNetTray;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

        // ── Status bar ──
        private System.Windows.Forms.StatusStrip statusStrip;
        internal System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
