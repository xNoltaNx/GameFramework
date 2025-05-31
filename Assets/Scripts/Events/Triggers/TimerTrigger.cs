using UnityEngine;
using UnityEngine.Events;
using GameFramework.Core;

namespace GameFramework.Events.Triggers
{
    /// <summary>
    /// Trigger that fires based on time conditions.
    /// Supports countdown timers, interval timers, and delayed execution.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Timer Trigger")]
    public class TimerTrigger : BaseTrigger
    {
        [System.Serializable]
        public enum TimerMode
        {
            Countdown,      // Fire once after a delay
            Interval,       // Fire repeatedly at intervals
            Delayed         // Fire once after a delay, but can be reset
        }
        
        [Header("Timer Settings")]
        [SerializeField] private TimerMode timerMode = TimerMode.Countdown;
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool unscaledTime = false;
        
        [Header("Interval Settings")]
        [SerializeField] private int maxIterations = -1; // -1 for infinite
        [SerializeField] private bool randomizeInterval = false;
        [SerializeField] private float intervalVariation = 0.1f;
        
        [Header("Timer Events")]
        [SerializeField] private UnityEvent onTimerStarted;
        [SerializeField] private UnityEvent onTimerStopped;
        [SerializeField] private UnityEvent onTimerPaused;
        [SerializeField] private UnityEvent onTimerResumed;
        [SerializeField] private UnityEvent<float> onTimerProgress; // 0.0 to 1.0
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showProgress = false;
        [SerializeField, ReadOnly] private float currentTime;
        [SerializeField, ReadOnly] private float targetTime;
        [SerializeField, ReadOnly] private int iterations;
        
        private bool isRunning = false;
        private bool isPaused = false;
        private float startTime;
        private float pausedTime;
        
        // C# Events for performance-critical scenarios
        public event System.Action OnTimerStarted;
        public event System.Action OnTimerStopped;
        public event System.Action OnTimerPaused;
        public event System.Action OnTimerResumed;
        public event System.Action<float> OnTimerProgress;
        
        /// <summary>
        /// Whether the timer is currently running.
        /// </summary>
        public bool IsRunning => isRunning && !isPaused;
        
        /// <summary>
        /// Whether the timer is currently paused.
        /// </summary>
        public bool IsPaused => isPaused;
        
        /// <summary>
        /// Current progress of the timer (0.0 to 1.0).
        /// </summary>
        public float Progress 
        { 
            get 
            { 
                if (targetTime <= 0f) return 1f;
                return Mathf.Clamp01(currentTime / targetTime);
            } 
        }
        
        /// <summary>
        /// Time remaining until the timer fires.
        /// </summary>
        public float TimeRemaining => Mathf.Max(0f, targetTime - currentTime);
        
        protected override void Start()
        {
            base.Start();
            
            if (startOnAwake && isActive)
            {
                StartTimer();
            }
        }
        
        private void Update()
        {
            if (!isRunning || isPaused || !isActive) return;
            
            UpdateTimer();
        }
        
        /// <summary>
        /// Update the timer based on the current mode.
        /// </summary>
        private void UpdateTimer()
        {
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            currentTime += deltaTime;
            
            // Fire progress event
            if (showProgress)
            {
                float progress = Progress;
                onTimerProgress?.Invoke(progress);
                OnTimerProgress?.Invoke(progress);
            }
            
            // Check if timer should fire
            if (currentTime >= targetTime)
            {
                HandleTimerComplete();
            }
        }
        
        /// <summary>
        /// Handle timer completion based on the timer mode.
        /// </summary>
        private void HandleTimerComplete()
        {
            LogDebug($"Timer completed - Mode: {timerMode}, Iteration: {iterations}");
            
            // Execute the trigger
            ExecuteTrigger(gameObject);
            
            iterations++;
            
            switch (timerMode)
            {
                case TimerMode.Countdown:
                case TimerMode.Delayed:
                    StopTimer();
                    break;
                    
                case TimerMode.Interval:
                    if (maxIterations > 0 && iterations >= maxIterations)
                    {
                        StopTimer();
                    }
                    else
                    {
                        RestartInterval();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Restart the interval timer with optional randomization.
        /// </summary>
        private void RestartInterval()
        {
            currentTime = 0f;
            
            if (randomizeInterval && intervalVariation > 0f)
            {
                float variation = Random.Range(-intervalVariation, intervalVariation);
                targetTime = duration + variation;
                targetTime = Mathf.Max(0.01f, targetTime); // Ensure positive duration
            }
            else
            {
                targetTime = duration;
            }
            
            LogDebug($"Interval restarted - Next duration: {targetTime:F2}s");
        }
        
        #region Public API
        
        /// <summary>
        /// Start the timer.
        /// </summary>
        public void StartTimer()
        {
            if (isRunning)
            {
                LogDebug("Timer already running");
                return;
            }
            
            isRunning = true;
            isPaused = false;
            currentTime = 0f;
            targetTime = duration;
            iterations = 0;
            startTime = unscaledTime ? Time.unscaledTime : Time.time;
            
            LogDebug($"Timer started - Mode: {timerMode}, Duration: {duration:F2}s");
            
            try
            {
                onTimerStarted?.Invoke();
                OnTimerStarted?.Invoke();
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in timer started handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void StopTimer()
        {
            if (!isRunning)
            {
                LogDebug("Timer not running");
                return;
            }
            
            isRunning = false;
            isPaused = false;
            
            LogDebug($"Timer stopped - Completed {iterations} iterations");
            
            try
            {
                onTimerStopped?.Invoke();
                OnTimerStopped?.Invoke();
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in timer stopped handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void PauseTimer()
        {
            if (!isRunning || isPaused)
            {
                LogDebug("Timer not running or already paused");
                return;
            }
            
            isPaused = true;
            pausedTime = unscaledTime ? Time.unscaledTime : Time.time;
            
            LogDebug("Timer paused");
            
            try
            {
                onTimerPaused?.Invoke();
                OnTimerPaused?.Invoke();
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in timer paused handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Resume the timer.
        /// </summary>
        public void ResumeTimer()
        {
            if (!isRunning || !isPaused)
            {
                LogDebug("Timer not running or not paused");
                return;
            }
            
            isPaused = false;
            
            LogDebug("Timer resumed");
            
            try
            {
                onTimerResumed?.Invoke();
                OnTimerResumed?.Invoke();
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in timer resumed handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Reset the timer to its initial state.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            
            StopTimer();
            currentTime = 0f;
            targetTime = duration;
            iterations = 0;
            
            LogDebug("Timer reset");
        }
        
        /// <summary>
        /// Set the timer duration.
        /// </summary>
        /// <param name="newDuration">The new duration in seconds</param>
        public void SetDuration(float newDuration)
        {
            duration = Mathf.Max(0.01f, newDuration);
            
            if (!isRunning)
            {
                targetTime = duration;
            }
            
            LogDebug($"Duration set to: {duration:F2}s");
        }
        
        /// <summary>
        /// Set the timer mode.
        /// </summary>
        /// <param name="mode">The new timer mode</param>
        public void SetTimerMode(TimerMode mode)
        {
            timerMode = mode;
            LogDebug($"Timer mode set to: {mode}");
        }
        
        /// <summary>
        /// Add time to the current timer.
        /// </summary>
        /// <param name="additionalTime">Time to add in seconds</param>
        public void AddTime(float additionalTime)
        {
            if (isRunning)
            {
                targetTime += additionalTime;
                LogDebug($"Added {additionalTime:F2}s to timer. New target: {targetTime:F2}s");
            }
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        [ContextMenu("Start Timer")]
        private void StartTimerEditor()
        {
            if (Application.isPlaying)
            {
                StartTimer();
            }
        }
        
        [ContextMenu("Stop Timer")]
        private void StopTimerEditor()
        {
            if (Application.isPlaying)
            {
                StopTimer();
            }
        }
        
        [ContextMenu("Pause Timer")]
        private void PauseTimerEditor()
        {
            if (Application.isPlaying)
            {
                PauseTimer();
            }
        }
        
        [ContextMenu("Resume Timer")]
        private void ResumeTimerEditor()
        {
            if (Application.isPlaying)
            {
                ResumeTimer();
            }
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            duration = Mathf.Max(0.01f, duration);
            intervalVariation = Mathf.Max(0f, intervalVariation);
            
            if (!Application.isPlaying)
            {
                currentTime = 0f;
                targetTime = duration;
            }
        }
        #endif
        
        #endregion
    }
}