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
        public enum ProcessStateChange
        {
            Start,
            End
        }

        public enum ProcessStartError
        {
            Create,
            Extension,
            FilePath,
            Active
        }

        public enum ProcessStopError
        {
            Inactive,
            Exited,
            Destroy
        }

        public event Action<ProcessStateChange> OnProcessStateChange;
        public event Action<ProcessStopError, Exception> OnProcessStopError;
        public event Action<ProcessStartError, Exception> OnProcessStartError;

        public string[] AcceptedFileExtensions;

        private Process process;
        private ProcessStartInfo processStartInfo;

        public ProcessManager(params string[] extensions)
        {
            this.AcceptedFileExtensions = extensions;
        }
        
        public bool IsProcessAvailable
        {
            get { return process != null;  }
        }

        public bool AcceptsPathExtension(string path)
        {
            return path != null && AcceptedFileExtensions.Contains(path.Substring(path.LastIndexOf(".")));
        }

        public void Start(string path)
        {

            if (this.IsProcessAvailable)
            {
                this.OnProcessStartError(ProcessStartError.Active, null);
                return;
            }

            if (!this.AcceptsPathExtension(path))
            {
                this.OnProcessStartError(ProcessStartError.Extension, null);
                return;
            }

            if(!File.Exists(path))
            {
                this.OnProcessStartError(ProcessStartError.FilePath, null);
                return;
            }

            this.CreateProcess(path);
        }

        public void RemoveCurrentProcess()
        {

            if (!this.IsProcessAvailable)
            {
                this.OnProcessStopError(ProcessStopError.Inactive, null);
                return;
            }

            if (process.HasExited)
            {
                this.OnProcessStopError(ProcessStopError.Exited, new Exception("Exit Code " + process.ExitCode));
                return;
            }

            this.DestroyProcess();
        }

        // ==================================================================
        // Internal Event Handlers
        // ==================================================================

        private void OnProcessEnd(object sender, EventArgs e)
        {
            if (this.IsProcessAvailable)
            {
                this.OnProcessStateChange(ProcessStateChange.End);
            }
        }

        private void CreateProcess(string path)
        {
            try
            {
                processStartInfo = new ProcessStartInfo(path);
                processStartInfo.WorkingDirectory = path.Substring(0, path.LastIndexOf("\\"));
                process = Process.Start(processStartInfo);
                process.EnableRaisingEvents = true;
                process.Exited += OnProcessEnd;
                this.OnProcessStateChange(ProcessStateChange.Start);
            }
            catch (Exception ex)
            {
                this.OnProcessStartError(ProcessStartError.Create, ex);
            }
        }

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
