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

        // Create a new instance that restricts processes to files with 
        // one of the given extensions
        public ProcessManager(params string[] extensions)
        {
            acceptedExtensions = extensions;
        }

        /**
         * Returns true if a process has been creatd
         */
        public bool IsProcessDefined
        {
            get { return process != null;  }
        }

        /**
         * Returns true if the given path ends with an extension accepted by this manager
         */
        public bool AcceptsPathExtension(string path)
        {
            int idx = path != null ? path.LastIndexOf(".") : -1;
            bool validIdx = idx >= 0 && idx < path.Length;
            return validIdx && acceptedExtensions.Contains(path.Substring(idx));
        }

        /**
         * Destroy the current process if one exists.
         * Any errors encountered during the stop routine will be emitted in OnProcessStopError.
         */
        public void Stop()
        {

            // Don't do anything if no process is active
            if (!IsProcessDefined)
            {
                OnProcessStopError(ProcessError.Inactive, null);
                return;
            }

            // Attempt to kill and remove the current process
            DestroyProcess();
        }

        /**
         * Start and maintain a Process instance for the executable at the given path.
         * Any errors encountered during the start routine  will be emitted in OnProcessStartError.
         */
        public void Start(string path)
        {

            // Don't start a process if we already have one active
            if (IsProcessDefined)
            {
                OnProcessStartError(ProcessError.Active, null);
                return;
            }

            // Don't start the process if the given path is not an accepted file type
            if (!AcceptsPathExtension(path))
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
            if (IsProcessDefined)
            {
                OnProcessStateChange(ProcessState.End);
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
    }
}
