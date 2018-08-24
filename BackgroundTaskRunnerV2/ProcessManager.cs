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
        public enum ProcessState
        {
            Start,
            End
        }

        // States emitted on error manipulating the process state
        public enum ProcessError
        {
            Create,
            Extension,
            FilePath,
            Active,
            Inactive,
            Destroy
        }

        // Core events which emit real-time info about the current process
        public event Action<ProcessState> ProcessStateChange;
        public event Action<ProcessError, Exception> ProcessStopError;
        public event Action<ProcessError, Exception> ProcessStartError;
        
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

            return acceptedExtensions.Contains(path.Substring(extensionIndex));
        }

        /**
         * Start and maintain a Process instance for the executable at the given path.
         * Any errors encountered during the start routine  will be emitted in OnProcessStartError.
         */
        public void Start(string path)
        {

            // Don't start a process if we already have one active
            if (IsProcessAvailable)
            {
                OnProcessStartError(ProcessError.Active, null);
                return;
            }

            // Don't start the process if the given path is not an accepted file type
            if (!this.AcceptsPathExtension(path))
            {
                OnProcessStartError(ProcessError.Extension, null);
                return;
            }

            // Don't start the process if the file doesn't exist
            if(!File.Exists(path))
            {
                OnProcessStartError(ProcessError.FilePath, null);
                return;
            }

            // Attempt to create the process
            CreateProcess(path);
        }

        /**
         * Destroy the current process if one exists.
         * Any errors encountered during the stop routine will be emitted in OnProcessStopError.
         */
        public void RemoveCurrentProcess()
        {

            // Don't do anything if no process is active
            if (!IsProcessAvailable)
            {
                OnProcessStopError(ProcessError.Inactive, null);
                return;
            }

            // Attempt to kill and remove the current process
            DestroyProcess();
        }

        // ===============================================================
        // Event wrappers to sanitize calls
        // ===============================================================

        // Emits the ProcessStateChange event if there are listeners registered to it
        private void OnProcessStateChange(ProcessState state)
        {
            ProcessStateChange?.Invoke(state);
        }

        // Emits the ProcessStartError event if there are listeners registered to it
        private void OnProcessStartError(ProcessError error, Exception exception)
        {
            ProcessStartError?.Invoke(error, exception);
        }

        // Emits the ProcessStopError event if there are listeners registered to it
        private void OnProcessStopError(ProcessError error, Exception exception)
        {
            ProcessStopError?.Invoke(error, exception);
        }

        // ==================================================================
        // Internal Event Handlers
        // ==================================================================

        // Callback for the Process.Exited event
        private void OnProcessEnd(object sender, EventArgs e)
        {
            if (IsProcessAvailable)
            {
                OnProcessStateChange(ProcessState.End);
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
                OnProcessStateChange(ProcessState.Start);
            }
            catch (Exception ex)
            {
                OnProcessStartError(ProcessError.Create, ex);
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
                OnProcessStopError(ProcessError.Destroy, ex);
            }

            process = null;
        }
    }
}
