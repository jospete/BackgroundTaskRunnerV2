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

        // Used to identify screensaver events
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_SCREENSAVE = 0xF140;

        // Core pause/resume states emitted by this manager
        public enum LockState
        {
            Sleep,
            WindowsLock,
            ScreenSaver
        }

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

        /**
         * Add hooks to static system classes
         */
        public void RegisterEventHandlers()
        {
            SystemEvents.SessionSwitch += this.sessionSwitchHandler;
            SystemEvents.PowerModeChanged += this.powerModeChangedHandler;
        }

        /**
         * Remove listener hooks from static system classes
         */
        public void DeregisterEventHandlers()
        {
            Application.Idle -= this.screenSaverIdleHandler;
            SystemEvents.SessionSwitch -= this.sessionSwitchHandler;
            SystemEvents.PowerModeChanged -= this.powerModeChangedHandler;
        }

        // ===============================================================
        // Core Event Handlers
        // ===============================================================

        // Emits pause event when screensaver application starts, and registers the screensaver idle listener
        public void HandleWndProc(ref Message m)
        {
            if(m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_SCREENSAVE)
            {
                this.Pause(LockState.ScreenSaver);
                Application.Idle += this.screenSaverIdleHandler;
            }
        }

        // Emits resume event when the screensaver application becomes idle
        private void ApplicationIdle_ScreenSaver(object sender, EventArgs e)
        {
            Application.Idle -= this.screenSaverIdleHandler;
            this.Resume(LockState.ScreenSaver);
        }

        // Emits pause/resume events when the user locks/unlocks their computer with Cmd+L
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock: this.Pause(LockState.WindowsLock); break;
                case SessionSwitchReason.SessionUnlock: this.Resume(LockState.WindowsLock); break;
                default: break;
            }
        }

        // Emits pause/resume events when the computer sleeps
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
