using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace BackgroundTaskRunnerV2
{

    /**
     * Process singleton manager.
     * Controls destroying and creating a target process.
     */
    public class ProcessManager
    {
        // States emitted in OnProcessStateChange
        public enum ProcessStateChange
        {
            Start,
            End
        }

        // States emitted in OnProcessStartError
        public enum ProcessStartError
        {
            Create,
            Extension,
            FilePath,
            Active
        }

        // States emitted in OnProcessStopError
        public enum ProcessStopError
        {
            Inactive,
            Destroy
        }

        // Core events which emit real-time info about the current process
        public event Action<ProcessStateChange> OnProcessStateChange;
        public event Action<ProcessStopError, Exception> OnProcessStopError;
        public event Action<ProcessStartError, Exception> OnProcessStartError;
        
        private Process process;
        private ProcessStartInfo processStartInfo;
        private string[] acceptedExtensions;

        public ProcessManager(params string[] extensions)
        {
            this.acceptedExtensions = extensions;
        }

        /**
         * Returns true if a process has been creatd
         */
        public bool IsProcessAvailable
        {
            get { return process != null;  }
        }

        /**
         * Returns true if the given path ends with an extension accepted by this manager
         */
        public bool AcceptsPathExtension(string path)
        {
            if(path == null || path == string.Empty)
            {
                return false;
            }

            int extensionIndex = path.LastIndexOf(".");

            if(extensionIndex < 0 || extensionIndex >= path.Length)
            {
                return false;
            }

            return this.acceptedExtensions.Contains(path.Substring(extensionIndex));
        }

        /**
         * Start and maintain a Process instance for the executable at the given path.
         * Any errors encountered during the start routine  will be emitted in OnProcessStartError.
         */
        public void Start(string path)
        {

            // Don't start a process if we already have one active
            if (this.IsProcessAvailable)
            {
                this.OnProcessStartError(ProcessStartError.Active, null);
                return;
            }

            // Don't start the process if the given path is not an accepted file type
            if (!this.AcceptsPathExtension(path))
            {
                this.OnProcessStartError(ProcessStartError.Extension, null);
                return;
            }

            // Don't start the process if the file doesn't exist
            if(!File.Exists(path))
            {
                this.OnProcessStartError(ProcessStartError.FilePath, null);
                return;
            }

            // Attempt to create the process
            this.CreateProcess(path);
        }

        /**
         * Destroy the current process if one exists.
         * Any errors encountered during the stop routine will be emitted in OnProcessStopError.
         */
        public void RemoveCurrentProcess()
        {

            // Don't do anything if no process is active
            if (!this.IsProcessAvailable)
            {
                this.OnProcessStopError(ProcessStopError.Inactive, null);
                return;
            }

            // Attempt to kill and remove the current process
            this.DestroyProcess();
        }

        // ==================================================================
        // Internal Event Handlers
        // ==================================================================

        // Callback for the Process.Exited event
        private void OnProcessEnd(object sender, EventArgs e)
        {
            if (this.IsProcessAvailable)
            {
                this.OnProcessStateChange(ProcessStateChange.End);
            }
        }

        // Create and maintain a process from the executable at the given path
        private void CreateProcess(string path)
        {
            try
            {
                // Set the working directory to the file's directory to keep its references in-tact
                processStartInfo = new ProcessStartInfo(path);
                processStartInfo.WorkingDirectory = path.Substring(0, path.LastIndexOf("\\"));

                // Start the process and enable events on it
                process = Process.Start(processStartInfo);
                process.EnableRaisingEvents = true;
                process.Exited += OnProcessEnd;

                // Emit process start state change
                this.OnProcessStateChange(ProcessStateChange.Start);
            }
            catch (Exception ex)
            {
                this.OnProcessStartError(ProcessStartError.Create, ex);
            }
        }

        // Destroy the current process and clear the reference
        private void DestroyProcess()
        {
            try
            {
                // Allow time to settle if we're waking up from screen lock or screen saver
                Thread.Sleep(500);
                process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                this.OnProcessStopError(ProcessStopError.Destroy, ex);
            }

            this.process = null;
        }
    }
}
