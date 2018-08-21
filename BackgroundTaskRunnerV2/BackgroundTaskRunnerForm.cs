using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

using LockState = BackgroundTaskRunnerV2.SystemEventManager.LockState;
using ProcessStateChange = BackgroundTaskRunnerV2.ProcessManager.ProcessStateChange;
using ProcessStartError = BackgroundTaskRunnerV2.ProcessManager.ProcessStartError;
using ProcessStopError = BackgroundTaskRunnerV2.ProcessManager.ProcessStopError;

namespace BackgroundTaskRunnerV2
{

    /**
     * Core Form model.
     */
    public partial class BackgroundTaskRunnerForm : Form
    {
        SystemEventManager systemEventsManager;
        ProcessManager processManager;

        // Translates the given arguments into an error message format
        private static string CreateErrorEventText(string text, Exception exception)
        {
            string exceptionText = exception != null ? " (" + exception.Message + ")" : "";
            return "[ERROR] " + text + exceptionText;
        }

        // ================================================================
        // Glue Stuff
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

        private void LogErrorEventAsync(string text, Exception exception)
        {
            this.LogEventAsync(CreateErrorEventText(text, exception));
        }
        
        private void LogEventAsync(string text)
        {
            this.Invoke(new Action<string>(this.LogEvent), new object[] { text });
        }

        private void LogErrorEvent(string text, Exception exception)
        {
            this.LogEvent(CreateErrorEventText(text, exception));
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
        // Process and System Event Handlers
        // ================================================================
        
        // Logs process start failure info
        private void HandleProcessStartError(ProcessStartError error, Exception exception)
        {
            LogErrorEventAsync("Process start - " + error.ToString(), exception);
        }

        // Logs process stop failure info
        private void HandleProcessStopError(ProcessStopError error, Exception exception)
        {
            LogErrorEventAsync("Process stop - " + error.ToString(), exception);
        }

        // Returns true if the given state has been checked by the user
        private bool IsValidLockState(LockState state)
        {
            return clbConditions.CheckedItems.Contains(state.ToString());
        }

        // Logs pause events, and starts the process if the pause event is a valid target
        private void HandlePauseEvent(LockState state)
        {
            LogEventAsync("Pause on lock state - " + state.ToString());
            if(IsValidLockState(state))
            {
                LogEventAsync("Pause event starting process...");
                StartWithCurrentPath();
            }
        }

        // Logs resume events, and stops the process if stopOnResume is checked and the resume state is valid
        private void HandleResumeEvent(LockState state)
        {
            LogEventAsync("Resume on lock state - " + state.ToString());
            if(cbStopOnResume.Checked && IsValidLockState(state))
            {
                LogEventAsync("Resume event killing process...");
                this.processManager.RemoveCurrentProcess();
            }
        }

        // Logs state changes for the current process
        private void HandleProcessStateChange(ProcessStateChange state)
        {
            LogEventAsync("Process state change - " + state.ToString());
            if(state == ProcessStateChange.End)
            {
                this.Invoke(new Action(this.processManager.RemoveCurrentProcess));
            }
        }

        // ================================================================
        // UI Interaction Handlers
        // ================================================================

        // Starts the process directly
        private void BtnManualStart_Click(object sender, EventArgs e)
        {
            StartWithCurrentPath();
        }

        // Stops the process directly
        private void BtnManualStop_Click(object sender, EventArgs e)
        {
            this.processManager.RemoveCurrentProcess();
        }

        // Opens the Github page for this project
        private void LinkSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkSource.Text);
        }

        private void CbStopOnResume_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: implement
        }

        private void CbOpenMinimized_CheckedChanged(object sender, EventArgs e)
        {
            // TODO: implement
        }

        private void TbFilePath_TextChanged(object sender, EventArgs e)
        {
            // TODO: implement
        }

        // Handles change detection for accepted lock states
        private void ClbConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] selected = clbConditions.CheckedItems.Cast<string>().ToArray();
            string aggregated = "None";

            if(selected != null && selected.Length > 0)
            {
                aggregated = selected.Aggregate((a, b) => a + ", " + b);
            }
            
            LogEvent("Conditions changed - " + aggregated);
        }

        // Opens a file browser and saves the selected file path when the dialog is confirmed
        private void BtnBrowse_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Batch Files (*.bat)|*.bat|Executables (*.exe)|(*.exe)";
            dialog.Title = "Select Runnable File";
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
