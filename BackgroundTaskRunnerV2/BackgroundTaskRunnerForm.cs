using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

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

        const string NO_CONDITIONS = "None";

        private ProcessManager processManager;
        private SystemEventManager systemEventsManager;

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
            Properties.Settings.Default.Reload();
            ApplySettings();

            if (cbOpenMinimized.Checked)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            LogEvent("Closing...");

            Properties.Settings.Default.Save();
            processManager.RemoveCurrentProcess();
            systemEventsManager.DeregisterEventHandlers();

            processManager.OnProcessStateChange -= HandleProcessStateChange;
            processManager.OnProcessStartError -= HandleProcessStartError;
            processManager.OnProcessStopError -= HandleProcessStopError;
            systemEventsManager.Pause -= HandlePauseEvent;
            systemEventsManager.Resume -= HandleResumeEvent;
        }

        // Convenience for starting the process with the current path
        private void StartWithCurrentPath()
        {
            this.processManager.Start(tbFilePath.Text);
        }

        // Passthrough for Windows Form events
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            systemEventsManager.HandleWndProc(ref m);
        }
        
        // Apply all settings from stored preferences
        private void ApplySettings()
        {
            LogEvent("Applying user settings");
            LoadFilePath();
            LoadCustomActionCheckboxes();
            LoadConditionCheckboxes();
        }

        // Load the file path from the stored preferences
        private void LoadFilePath()
        {
            tbFilePath.Text = Properties.Settings.Default.FilePath;
        }

        // Load custom checkbox states from stored preferences
        private void LoadCustomActionCheckboxes()
        {
            cbStopOnResume.Checked = Properties.Settings.Default.StopProcessOnResume;
            cbOpenMinimized.Checked = Properties.Settings.Default.MinimizeOnStart;
        }

        // Load condition states from stored preferences
        private void LoadConditionCheckboxes()
        {
            
            string[] states = Enum.GetValues(typeof(LockState))
                .Cast<LockState>()
                .Select(state => state.ToString())
                .ToArray<string>();

            clbConditions.Items.Clear();
            clbConditions.Items.AddRange(states);
            string selectedConditions = Properties.Settings.Default.Conditions;

            for (int i = 0; i < states.Length; i++)
            {
                clbConditions.SetItemChecked(i, selectedConditions.Contains(states[i]));
            }
        }

        // ================================================================
        // Event Logging
        // ================================================================

        // Posts an error event log on the thread queue
        private void LogErrorEventAsync(string text, Exception exception)
        {
            this.LogEventAsync(CreateErrorEventText(text, exception));
        }
        
        // Posts an event log on the thread queue
        private void LogEventAsync(string text)
        {
            this.Invoke(new Action<string>(this.LogEvent), new object[] { text });
        }

        // Wraps a logged event with an error format
        private void LogErrorEvent(string text, Exception exception)
        {
            this.LogEvent(CreateErrorEventText(text, exception));
        }

        // Core event logging that adds the event to the event logs list box
        private void LogEvent(string text)
        {

            string value = DateTime.Now.ToLocalTime() + ":      " + text;
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

        // Save the 'Stop On Resume' checked state on change
        private void CbStopOnResume_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StopProcessOnResume = cbStopOnResume.Checked;
        }

        // Save the 'Minimize On Start' checked state on change
        private void CbOpenMinimized_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimizeOnStart = cbOpenMinimized.Checked;
        }

        // Save the target file path on change
        private void TbFilePath_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FilePath = tbFilePath.Text;
        }

        // Handles change detection for accepted lock states
        private void ClbConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] selected = clbConditions.CheckedItems.Cast<string>().ToArray();
            string aggregated = NO_CONDITIONS;

            if(selected != null && selected.Length > 0)
            {
                aggregated = selected.Aggregate((a, b) => a + ", " + b);
            }
            
            LogEvent("Conditions changed - " + aggregated);
            Properties.Settings.Default.Conditions = aggregated;
            
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
