using System.Collections;
using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Abstract base class for all trigger actions.
    /// Provides common functionality for execution delay and state management.
    /// </summary>
    public abstract class BaseTriggerAction : MonoBehaviour, ITriggerAction
    {
        [Header("Action Settings")]
        [SerializeField] protected bool isEnabled = true;
        [SerializeField] protected float executionDelay = 0f;
        [SerializeField] protected bool stopOnDisable = true;
        
        [Header("Debug")]
        [SerializeField] protected bool debugMode = false;
        
        protected bool isExecuting = false;
        protected Coroutine executionCoroutine;
        
        #region ITriggerAction Implementation
        
        /// <summary>
        /// Whether this action is currently enabled.
        /// </summary>
        public bool IsEnabled 
        { 
            get => isEnabled; 
            set => isEnabled = value; 
        }
        
        /// <summary>
        /// The delay before this action executes.
        /// </summary>
        public float ExecutionDelay => executionDelay;
        
        /// <summary>
        /// Whether the action is currently executing.
        /// </summary>
        public bool IsExecuting => isExecuting;
        
        /// <summary>
        /// Execute the action with the provided context.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        public virtual void Execute(GameObject context = null)
        {
            if (!isEnabled)
            {
                LogDebug("Action not executed - disabled");
                return;
            }
            
            if (isExecuting)
            {
                LogDebug("Action not executed - already executing");
                return;
            }
            
            LogDebug($"Executing action with context: {(context ? context.name : "null")}");
            
            if (executionDelay > 0f)
            {
                executionCoroutine = StartCoroutine(ExecuteWithDelay(context));
            }
            else
            {
                ExecuteImmediate(context);
            }
        }
        
        /// <summary>
        /// Stop the action if it's currently executing.
        /// </summary>
        public virtual void Stop()
        {
            if (!isExecuting)
            {
                return;
            }
            
            LogDebug("Action stopped");
            
            if (executionCoroutine != null)
            {
                StopCoroutine(executionCoroutine);
                executionCoroutine = null;
            }
            
            OnActionStopped();
            isExecuting = false;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void OnDisable()
        {
            if (stopOnDisable && isExecuting)
            {
                Stop();
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (isExecuting)
            {
                Stop();
            }
        }
        
        #endregion
        
        #region Execution Logic
        
        /// <summary>
        /// Execute the action with delay using a coroutine.
        /// </summary>
        /// <param name="context">The context object</param>
        /// <returns>Coroutine enumerator</returns>
        protected virtual IEnumerator ExecuteWithDelay(GameObject context)
        {
            isExecuting = true;
            OnActionStarted(context);
            
            LogDebug($"Waiting {executionDelay:F2}s before executing");
            yield return new WaitForSeconds(executionDelay);
            
            if (isExecuting) // Check if still executing (might have been stopped)
            {
                ExecuteImmediate(context);
            }
        }
        
        /// <summary>
        /// Execute the action immediately without delay.
        /// </summary>
        /// <param name="context">The context object</param>
        protected virtual void ExecuteImmediate(GameObject context)
        {
            if (!isExecuting && executionDelay <= 0f)
            {
                isExecuting = true;
                OnActionStarted(context);
            }
            
            try
            {
                PerformAction(context);
                OnActionCompleted(context);
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception during action execution: {e.Message}");
                Debug.LogException(e, this);
                OnActionFailed(context, e);
            }
            finally
            {
                isExecuting = false;
                executionCoroutine = null;
            }
        }
        
        /// <summary>
        /// Perform the actual action logic. Override in derived classes.
        /// </summary>
        /// <param name="context">The context object</param>
        protected abstract void PerformAction(GameObject context);
        
        #endregion
        
        #region Virtual Event Methods
        
        /// <summary>
        /// Called when the action starts executing.
        /// </summary>
        /// <param name="context">The context object</param>
        protected virtual void OnActionStarted(GameObject context)
        {
            LogDebug("Action started");
        }
        
        /// <summary>
        /// Called when the action completes successfully.
        /// </summary>
        /// <param name="context">The context object</param>
        protected virtual void OnActionCompleted(GameObject context)
        {
            LogDebug("Action completed");
        }
        
        /// <summary>
        /// Called when the action is stopped before completion.
        /// </summary>
        protected virtual void OnActionStopped()
        {
            LogDebug("Action stopped");
        }
        
        /// <summary>
        /// Called when the action fails with an exception.
        /// </summary>
        /// <param name="context">The context object</param>
        /// <param name="exception">The exception that occurred</param>
        protected virtual void OnActionFailed(GameObject context, System.Exception exception)
        {
            LogWarning($"Action failed: {exception.Message}");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set the execution delay for this action.
        /// </summary>
        /// <param name="delay">The new delay in seconds</param>
        public virtual void SetExecutionDelay(float delay)
        {
            executionDelay = Mathf.Max(0f, delay);
            LogDebug($"Execution delay set to: {executionDelay:F2}s");
        }
        
        /// <summary>
        /// Enable or disable this action.
        /// </summary>
        /// <param name="enabled">Whether the action should be enabled</param>
        public virtual void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            LogDebug($"Action {(enabled ? "enabled" : "disabled")}");
            
            if (!enabled && isExecuting)
            {
                Stop();
            }
        }
        
        #endregion
        
        #region Logging
        
        /// <summary>
        /// Log a debug message if debug mode is enabled.
        /// </summary>
        protected void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}:{name}] {message}", this);
            }
        }
        
        /// <summary>
        /// Log a warning message.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}:{name}] {message}", this);
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            executionDelay = Mathf.Max(0f, executionDelay);
        }
        
        [ContextMenu("Test Execute")]
        private void TestExecute()
        {
            if (Application.isPlaying)
            {
                Execute(gameObject);
            }
            else
            {
                Debug.Log($"[{GetType().Name}:{name}] Would execute action (Editor mode)");
            }
        }
        
        [ContextMenu("Stop Action")]
        private void StopAction()
        {
            if (Application.isPlaying)
            {
                Stop();
            }
        }
        #endif
        
        #endregion
    }
}