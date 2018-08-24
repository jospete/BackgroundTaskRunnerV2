namespace BackgroundTaskRunnerV2
{
    partial class BackgroundTaskRunnerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackgroundTaskRunnerForm));
            this.lbEventLogs = new System.Windows.Forms.ListBox();
            this.lblEventLogs = new System.Windows.Forms.Label();
            this.lblConditions = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.clbConditions = new System.Windows.Forms.CheckedListBox();
            this.btnManualStart = new System.Windows.Forms.Button();
            this.cbStopOnResume = new System.Windows.Forms.CheckBox();
            this.cbOpenMinimized = new System.Windows.Forms.CheckBox();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnManualStop = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.linkSource = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lbEventLogs
            // 
            this.lbEventLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbEventLogs.FormattingEnabled = true;
            this.lbEventLogs.HorizontalScrollbar = true;
            this.lbEventLogs.Location = new System.Drawing.Point(20, 322);
            this.lbEventLogs.Name = "lbEventLogs";
            this.lbEventLogs.Size = new System.Drawing.Size(457, 173);
            this.lbEventLogs.TabIndex = 0;
            // 
            // lblEventLogs
            // 
            this.lblEventLogs.AutoSize = true;
            this.lblEventLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEventLogs.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblEventLogs.Location = new System.Drawing.Point(17, 294);
            this.lblEventLogs.Name = "lblEventLogs";
            this.lblEventLogs.Size = new System.Drawing.Size(71, 13);
            this.lblEventLogs.TabIndex = 1;
            this.lblEventLogs.Text = "Event Logs";
            // 
            // lblConditions
            // 
            this.lblConditions.AutoSize = true;
            this.lblConditions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConditions.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblConditions.Location = new System.Drawing.Point(16, 13);
            this.lblConditions.Name = "lblConditions";
            this.lblConditions.Size = new System.Drawing.Size(248, 20);
            this.lblConditions.TabIndex = 2;
            this.lblConditions.Text = "If any of these conditions are met:";
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAction.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblAction.Location = new System.Drawing.Point(16, 129);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(210, 20);
            this.lblAction.TabIndex = 3;
            this.lblAction.Text = "Perform the following Action:";
            // 
            // clbConditions
            // 
            this.clbConditions.CheckOnClick = true;
            this.clbConditions.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.clbConditions.FormattingEnabled = true;
            this.clbConditions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.clbConditions.Location = new System.Drawing.Point(20, 39);
            this.clbConditions.Margin = new System.Windows.Forms.Padding(8);
            this.clbConditions.Name = "clbConditions";
            this.clbConditions.Size = new System.Drawing.Size(245, 64);
            this.clbConditions.TabIndex = 4;
            this.clbConditions.SelectedIndexChanged += new System.EventHandler(this.ClbConditions_SelectedIndexChanged);
            // 
            // btnManualStart
            // 
            this.btnManualStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManualStart.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnManualStart.Location = new System.Drawing.Point(369, 152);
            this.btnManualStart.Name = "btnManualStart";
            this.btnManualStart.Size = new System.Drawing.Size(108, 31);
            this.btnManualStart.TabIndex = 5;
            this.btnManualStart.Text = "Manual Start";
            this.btnManualStart.UseVisualStyleBackColor = true;
            this.btnManualStart.Click += new System.EventHandler(this.BtnManualStart_Click);
            // 
            // cbStopOnResume
            // 
            this.cbStopOnResume.AutoSize = true;
            this.cbStopOnResume.Checked = true;
            this.cbStopOnResume.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStopOnResume.Font = new System.Drawing.Font("Microsoft Tai Le", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbStopOnResume.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.cbStopOnResume.Location = new System.Drawing.Point(338, 16);
            this.cbStopOnResume.Name = "cbStopOnResume";
            this.cbStopOnResume.Size = new System.Drawing.Size(146, 20);
            this.cbStopOnResume.TabIndex = 7;
            this.cbStopOnResume.Text = "Auto-Stop On Resume";
            this.cbStopOnResume.UseVisualStyleBackColor = true;
            this.cbStopOnResume.CheckedChanged += new System.EventHandler(this.CbStopOnResume_CheckedChanged);
            // 
            // cbOpenMinimized
            // 
            this.cbOpenMinimized.AutoSize = true;
            this.cbOpenMinimized.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbOpenMinimized.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.cbOpenMinimized.Location = new System.Drawing.Point(338, 39);
            this.cbOpenMinimized.Name = "cbOpenMinimized";
            this.cbOpenMinimized.Size = new System.Drawing.Size(161, 19);
            this.cbOpenMinimized.TabIndex = 8;
            this.cbOpenMinimized.Text = "Open Runner Minimized";
            this.cbOpenMinimized.UseVisualStyleBackColor = true;
            this.cbOpenMinimized.CheckedChanged += new System.EventHandler(this.CbOpenMinimized_CheckedChanged);
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(20, 152);
            this.tbFilePath.Multiline = true;
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(244, 70);
            this.tbFilePath.TabIndex = 9;
            this.tbFilePath.TextChanged += new System.EventHandler(this.TbFilePath_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowse.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnBrowse.Location = new System.Drawing.Point(20, 228);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(108, 31);
            this.btnBrowse.TabIndex = 10;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // btnManualStop
            // 
            this.btnManualStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManualStop.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnManualStop.Location = new System.Drawing.Point(369, 191);
            this.btnManualStop.Name = "btnManualStop";
            this.btnManualStop.Size = new System.Drawing.Size(108, 31);
            this.btnManualStop.TabIndex = 11;
            this.btnManualStop.Text = "Manual Stop";
            this.btnManualStop.UseVisualStyleBackColor = true;
            this.btnManualStop.Click += new System.EventHandler(this.BtnManualStop_Click);
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSource.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblSource.Location = new System.Drawing.Point(28, 510);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(51, 13);
            this.lblSource.TabIndex = 12;
            this.lblSource.Text = "Source:";
            // 
            // linkSource
            // 
            this.linkSource.AutoSize = true;
            this.linkSource.Location = new System.Drawing.Point(86, 510);
            this.linkSource.Name = "linkSource";
            this.linkSource.Size = new System.Drawing.Size(269, 13);
            this.linkSource.TabIndex = 13;
            this.linkSource.TabStop = true;
            this.linkSource.Text = "https://github.com/jospete/BackgroundTaskRunnerV2";
            this.linkSource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSource_LinkClicked);
            // 
            // BackgroundTaskRunnerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(505, 532);
            this.Controls.Add(this.linkSource);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.btnManualStop);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.cbOpenMinimized);
            this.Controls.Add(this.cbStopOnResume);
            this.Controls.Add(this.btnManualStart);
            this.Controls.Add(this.clbConditions);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.lblConditions);
            this.Controls.Add(this.lblEventLogs);
            this.Controls.Add(this.lbEventLogs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BackgroundTaskRunnerForm";
            this.Text = "Background Task Runner V2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbEventLogs;
        private System.Windows.Forms.Label lblEventLogs;
        private System.Windows.Forms.Label lblConditions;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.CheckedListBox clbConditions;
        private System.Windows.Forms.Button btnManualStart;
        private System.Windows.Forms.CheckBox cbStopOnResume;
        private System.Windows.Forms.CheckBox cbOpenMinimized;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnManualStop;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.LinkLabel linkSource;
    }
}

