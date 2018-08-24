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
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SCREENSAVE = 0xF140;
        private const int SC_MONITORPOWER = 0xF170;

        // Core pause/resume states emitted by this manager
        public enum LockState
        {
            Sleep,
            WindowsLock,
            ScreenSaver,
            MonitorPower
        }

        // Monitor power states received by the SC_MONITORPOWER event
        public enum MonitorPowerState
        {
            LowPower = 1,
            PoweringOn = -1,
            PoweringOff = 2
        }

        public event Action<LockState> Pause;
        public event Action<LockState> Resume;

        private EventHandler screenSaverIdleHandler;
        private SessionSwitchEventHandler sessionSwitchHandler;
        private PowerModeChangedEventHandler powerModeChangedHandler;

        /**
         * Create a new SystemEventManager instance and its associated event handlers
         */
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

        // Emits pause/resume events when the computer's monitor power is toggling
        private void HandleMonitorPowerStateChange(int state)
        {
            switch (state)
            {
                case (int)MonitorPowerState.PoweringOff: this.Pause(LockState.MonitorPower); break;
                case (int)MonitorPowerState.PoweringOn: this.Resume(LockState.MonitorPower); break;
                default: break;
            }
        }

        // Emits resume event when the screensaver application becomes idle
        private void ApplicationIdle_ScreenSaver(object sender, EventArgs e)
        {
            Application.Idle -= this.screenSaverIdleHandler;
            this.Resume(LockState.ScreenSaver);
        }

        // ===============================================================
        // WndProc() Receiver
        // ===============================================================

        // Emits pause event when screensaver application starts, and registers the screensaver idle listener
        public void HandleWndProc(ref Message m)
        {
            if(m.Msg != WM_SYSCOMMAND)
            {
                return;
            }

            int state = m.WParam.ToInt32();

            switch (state)
            {
                case SC_SCREENSAVE:
                    this.Pause(LockState.ScreenSaver);
                    Application.Idle += this.screenSaverIdleHandler;
                    break;
                case SC_MONITORPOWER:
                    HandleMonitorPowerStateChange(state);
                    break;
                default:
                    break;
            }
        }
    }
}
