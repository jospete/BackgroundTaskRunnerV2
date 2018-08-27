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
            screenSaverIdleHandler = new EventHandler(ApplicationIdle_ScreenSaver);
            sessionSwitchHandler = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            powerModeChangedHandler = new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            syscommandHandlers = new Dictionary<int, Action<int>>()
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
                case WM_SYSCOMMAND: return GetMapAction(syscommandHandlers, stateType);
                default: return null;
            }
        }

        // Helper for safely extracting an action type from the given dictionary
        private static Action<int> GetMapAction(Dictionary<int, Action<int>> dictionary, int stateType)
        {
            return dictionary.ContainsKey(stateType) ? dictionary[stateType] : null;
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
            OnResume(LockState.ScreenSaver);
        }

        // Begin pause/resume cycle when screen saver becomes active
        private void HandleScreenSaverStart()
        {
            OnPause(LockState.ScreenSaver);
            // FIXME: need to find the "correct" way to detect screensaver stop
            // Application.Idle += screenSaverIdleHandler;
        }

        // Emits pause/resume events when the computer's monitor power is toggling
        // FIXME: 'PoweringOn' state never triggers
        private void HandleMonitorPowerStateChange(int state)
        {
            ConsumeLockChange(LockState.MonitorPower, MonitorPowerState.PoweringOff, MonitorPowerState.PoweringOn, (MonitorPowerState)state);
        }

        // Emits pause/resume events when the user locks/unlocks their computer with Cmd+L
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            ConsumeLockChange(LockState.WindowsLock, SessionSwitchReason.SessionLock, SessionSwitchReason.SessionUnlock, e.Reason);
        }

        // Emits pause/resume events when the computer sleeps
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            ConsumeLockChange(LockState.Sleep, PowerModes.Suspend, PowerModes.Resume, e.Mode);
        }

        // Base lock state handler for all events
        private void ConsumeLockChange<T>(LockState state, T pause, T resume, T value)
        {
            if(pause.Equals(value))
            {
                OnPause(state);
            } else if (resume.Equals(value))
            {
                OnResume(state);
            }
        }
    }
}
