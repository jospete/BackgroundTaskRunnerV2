using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BackgroundTaskRunnerV2
{
    /**
     * Core lock state handler for system events.
     * This controls the pause/resume cycle.
     */
    public class SystemEventManager
    {

        // Used to identify WndProc system events
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SCREENSAVE = 0xF140;
        private const int SC_MONITORPOWER = 0xF170;

        // Core pause/resume states emitted by this manager
        public enum LockState
        {
            WindowsLock,
            ScreenSaver,
            MonitorPower,
            Sleep
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
        private Dictionary<int, Action<int>> syscommandHandlers;

        /**
         * Create a new SystemEventManager instance and its associated event handlers
         */
        public SystemEventManager()
        {
            this.screenSaverIdleHandler = new EventHandler(ApplicationIdle_ScreenSaver);
            this.sessionSwitchHandler = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            this.powerModeChangedHandler = new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            this.syscommandHandlers = new Dictionary<int, Action<int>>()
            {
                { SC_SCREENSAVE, stateValue => HandleScreenSaverStart() },
                { SC_MONITORPOWER, stateValue => HandleMonitorPowerStateChange(stateValue) }
            };
        }

        /**
         * Add hooks to static system classes
         */
        public void RegisterEventHandlers()
        {
            SystemEvents.SessionSwitch += sessionSwitchHandler;
            SystemEvents.PowerModeChanged += powerModeChangedHandler;
        }

        /**
         * Remove listener hooks from static system classes
         */
        public void DeregisterEventHandlers()
        {
            Application.Idle -= screenSaverIdleHandler;
            SystemEvents.SessionSwitch -= sessionSwitchHandler;
            SystemEvents.PowerModeChanged -= powerModeChangedHandler;
        }

        /**
         * Get the action associated with the given event.
         * Returns null if no such action exists.
         */
        public Action<int> GetWndProcHandler(int message, int stateType)
        {
            switch (message)
            {
                case WM_SYSCOMMAND:
                    return syscommandHandlers.ContainsKey(stateType) ? syscommandHandlers[stateType] : null;
                default:
                    return null;
            }
        }

        // ===============================================================
        // Event wrappers to sanitize calls
        // ===============================================================

        // Emits the Pause event if there are listeners registered to it
        private void OnPause(LockState state)
        {
            Pause?.Invoke(state);
        }

        // Emits the Resume event if there are listeners registered to it
        private void OnResume(LockState state)
        {
            Resume?.Invoke(state);
        }

        // ===============================================================
        // Core Event Handlers
        // ===============================================================

        // Emits resume event when the screensaver application becomes idle
        private void ApplicationIdle_ScreenSaver(object sender, EventArgs e)
        {
            Application.Idle -= screenSaverIdleHandler;
            Resume(LockState.ScreenSaver);
        }

        // Begin pause/resume cycle when screen saver becomes active
        private void HandleScreenSaverStart()
        {
            Pause(LockState.ScreenSaver);
            // FIXME: need to find the "correct" way to detect screensaver stop
            // Application.Idle += screenSaverIdleHandler;
        }

        // Emits pause/resume events when the computer's monitor power is toggling
        private void HandleMonitorPowerStateChange(int state)
        {
            switch (state)
            {
                case (int)MonitorPowerState.PoweringOff: OnPause(LockState.MonitorPower); break;
                case (int)MonitorPowerState.PoweringOn: OnResume(LockState.MonitorPower); break;
                default: break;
            }
        }

        // Emits pause/resume events when the user locks/unlocks their computer with Cmd+L
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock: OnPause(LockState.WindowsLock); break;
                case SessionSwitchReason.SessionUnlock: OnResume(LockState.WindowsLock); break;
                default: break;
            }
        }

        // Emits pause/resume events when the computer sleeps
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend: OnPause(LockState.Sleep); break;
                case PowerModes.Resume: OnResume(LockState.Sleep); break;
                default: break;
            }
        }
    }
}
