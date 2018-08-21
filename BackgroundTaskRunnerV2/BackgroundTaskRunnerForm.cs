﻿using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

using LockState = BackgroundTaskRunnerV2.SystemEventManager.LockState;
using ProcessStateChange = BackgroundTaskRunnerV2.ProcessManager.ProcessStateChange;
using ProcessStartError = BackgroundTaskRunnerV2.ProcessManager.ProcessStartError;
using ProcessStopError = BackgroundTaskRunnerV2.ProcessManager.ProcessStopError;

namespace BackgroundTaskRunnerV2
{
    public partial class BackgroundTaskRunnerForm : Form
    {
        SystemEventManager systemEventsManager;
        ProcessManager processManager;

        private static string CreateErrorEventText(string text, Exception exception)
        {
            string exceptionText = exception != null ? " (" + exception.Message + ")" : "";
            return "[ERROR] " + text + exceptionText;
        }

        // ================================================================
        // Core Glue Stuff
        // ================================================================

        public BackgroundTaskRunnerForm()
        {
            InitializeComponent();
            processManager = new ProcessManager(".bat", ".exe");
            systemEventsManager = new SystemEventManager();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LogEvent("Start Up");
            processManager.OnProcessStateChange += HandleProcessStateChange;
            processManager.OnProcessStartError += HandleProcessStartError;
            processManager.OnProcessStopError += HandleProcessStopError;
            systemEventsManager.Pause += HandlePauseEvent;
            systemEventsManager.Resume += HandleResumeEvent;
            systemEventsManager.RegisterEventHandlers();
            LoadConditionCheckboxes();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogEvent("Closing...");
            processManager.RemoveCurrentProcess();
            systemEventsManager.DeregisterEventHandlers();
            processManager.OnProcessStateChange -= HandleProcessStateChange;
            processManager.OnProcessStartError -= HandleProcessStartError;
            processManager.OnProcessStopError -= HandleProcessStopError;
            systemEventsManager.Pause -= HandlePauseEvent;
            systemEventsManager.Resume -= HandleResumeEvent;
        }

        private void StartWithCurrentPath()
        {
            this.processManager.Start(tbFilePath.Text);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            systemEventsManager.HandleWndProc(ref m);
        }

        private void LoadConditionCheckboxes()
        {
            
            string[] states = Enum.GetValues(typeof(LockState))
                .Cast<LockState>()
                .Select(state => state.ToString())
                .ToArray<string>();

            clbConditions.Items.Clear();
            clbConditions.Items.AddRange(states);

            for (int i = 0; i < states.Length; i++)
            {
                clbConditions.SetItemChecked(i, true);
            }
        }

        // ================================================================
        // Event Logging
        // ================================================================

        private void LogErrorEvent(string text, Exception exception)
        {
            LogEvent(CreateErrorEventText(text, exception));
        }

        private void LogErrorEventAsync(string text, Exception exception)
        {
            LogEventAsync(CreateErrorEventText(text, exception));
        }

        private void LogEventAsync(string text)
        {
            this.Invoke(new Action<string>(this.LogEvent), new object[] { text });
        }
        
        private void LogEvent(string text)
        {

            string value = DateTime.Now.ToLocalTime() + ":      " + text;
            Console.WriteLine(value);
            lbEventLogs.Items.Add(value);

            int visibleItems = lbEventLogs.ClientSize.Height / lbEventLogs.ItemHeight;
            lbEventLogs.TopIndex = Math.Max(lbEventLogs.Items.Count - visibleItems + 1, 0);
        }

        // ================================================================
        // Core Event Handlers
        // ================================================================

        private void BtnManualStart_Click(object sender, EventArgs e)
        {
            StartWithCurrentPath();
        }

        private void BtnManualStop_Click(object sender, EventArgs e)
        {
            this.processManager.RemoveCurrentProcess();
        }

        private void LinkSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkSource.Text);
        }

        private void ClbConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] selected = clbConditions.CheckedItems.Cast<string>().ToArray();
            LogEvent("Checked states changed: " + selected.Aggregate((a, b) => a + ", " + b));
        }
        
        private void HandleProcessStartError(ProcessStartError error, Exception exception)
        {
            LogErrorEventAsync("Process Start - " + error.ToString(), exception);
        }

        private void HandleProcessStopError(ProcessStopError error, Exception exception)
        {
            LogErrorEventAsync("Process Stop - " + error.ToString(), exception);
        }

        private void HandlePauseEvent(LockState state)
        {
            LogEvent("Pause from lock state - " + state.ToString());
            if(clbConditions.CheckedItems.Contains(state.ToString()))
            {
                LogEventAsync("Pause event starting process...");
                StartWithCurrentPath();
            }
        }

        private void HandleResumeEvent(LockState state)
        {
            LogEventAsync("Resume from lock state - " + state.ToString());
            this.processManager.RemoveCurrentProcess();
        }

        private void HandleProcessStateChange(ProcessStateChange state)
        {
            LogEventAsync("Process State - " + state.ToString());
            if(state == ProcessStateChange.End)
            {
                this.Invoke(new Action(this.processManager.RemoveCurrentProcess));
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Batch Files (*.bat)|*.bat|Executables (*.exe)|(*.exe)";
            dialog.Title = "Select Process";
            dialog.InitialDirectory = "C:\\Desktop";
            dialog.RestoreDirectory = true;
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbFilePath.Text = dialog.FileName;
            }
        }
    }
}
