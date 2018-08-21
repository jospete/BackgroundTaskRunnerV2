using System;
using Microsoft.Win32;
using System.Windows.Forms;

namespace BackgroundTaskRunnerV2
{
    /**
     * Core lock state handler for system events.
     * This controls the pause/resume cycle.
     */
    public class SystemEventManager
    {

        public enum LockState
        {
            Sleep,
            WindowsLock,
            ScreenSaver
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_SCREENSAVE = 0xF140;

        public event Action<LockState> Pause;
        public event Action<LockState> Resume;

        private EventHandler screenSaverIdleHandler;
        private SessionSwitchEventHandler sessionSwitchHandler;
        private PowerModeChangedEventHandler powerModeChangedHandler;

        public SystemEventManager()
        {
            this.screenSaverIdleHandler = new EventHandler(ApplicationIdle_ScreenSaver);
            this.sessionSwitchHandler = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            this.powerModeChangedHandler = new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
        }

        public void RegisterEventHandlers()
        {
            SystemEvents.SessionSwitch += this.sessionSwitchHandler;
            SystemEvents.PowerModeChanged += this.powerModeChangedHandler;
        }

        public void DeregisterEventHandlers()
        {
            Application.Idle -= this.screenSaverIdleHandler;
            SystemEvents.SessionSwitch -= this.sessionSwitchHandler;
            SystemEvents.PowerModeChanged -= this.powerModeChangedHandler;
        }

        // ===============================================================
        // Core Event Handlers
        // ===============================================================

        public void HandleWndProc(ref Message m)
        {
            if(m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_SCREENSAVE)
            {
                this.Pause(LockState.ScreenSaver);
                Application.Idle += this.screenSaverIdleHandler;
            }
        }

        private void ApplicationIdle_ScreenSaver(object sender, EventArgs e)
        {
            Application.Idle -= this.screenSaverIdleHandler;
            this.Resume(LockState.ScreenSaver);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock: this.Pause(LockState.WindowsLock); break;
                case SessionSwitchReason.SessionUnlock: this.Resume(LockState.WindowsLock); break;
                default: break;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend: this.Pause(LockState.Sleep); break;
                case PowerModes.Resume: this.Resume(LockState.Sleep); break;
                default: break;
            }
        }
    }
}
