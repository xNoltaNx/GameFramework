using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Conditions
{
    /// <summary>
    /// Abstract base class for all trigger conditions.
    /// Provides common functionality for condition evaluation.
    /// </summary>
    public abstract class BaseTriggerCondition : MonoBehaviour, ITriggerCondition
    {
        [Header("Condition Settings")]
        [SerializeField] protected bool isEnabled = true;
        [SerializeField] protected bool invertResult = false;
        
        [Header("Debug")]
        [SerializeField] protected bool debugMode = false;
        
        #region ITriggerCondition Implementation
        
        /// <summary>
        /// Whether this condition is currently enabled.
        /// </summary>
        public bool IsEnabled 
        { 
            get => isEnabled; 
            set => isEnabled = value; 
        }
        
        /// <summary>
        /// Evaluate the condition with the provided context.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        /// <returns>True if the condition is met, false otherwise</returns>
        public bool Evaluate(GameObject context = null)
        {
            if (!isEnabled)
            {
                LogDebug("Condition not evaluated - disabled");
                return false;
            }
            
            bool result = EvaluateCondition(context);
            
            if (invertResult)
            {
                result = !result;
            }
            
            LogDebug($"Condition evaluated: {result} (inverted: {invertResult})");
            
            return result;
        }
        
        /// <summary>
        /// Reset the condition to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            LogDebug("Condition reset");
        }
        
        #endregion
        
        /// <summary>
        /// Perform the actual condition evaluation. Override in derived classes.
        /// </summary>
        /// <param name="context">The context object</param>
        /// <returns>True if the condition is met</returns>
        protected abstract bool EvaluateCondition(GameObject context);
        
        #region Public API
        
        /// <summary>
        /// Enable or disable this condition.
        /// </summary>
        /// <param name="enabled">Whether the condition should be enabled</param>
        public virtual void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            LogDebug($"Condition {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set whether to invert the condition result.
        /// </summary>
        /// <param name="invert">Whether to invert the result</param>
        public virtual void SetInvertResult(bool invert)
        {
            invertResult = invert;
            LogDebug($"Invert result set to: {invert}");
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
        [ContextMenu("Test Condition")]
        private void TestCondition()
        {
            if (Application.isPlaying)
            {
                bool result = Evaluate(gameObject);
                Debug.Log($"[{GetType().Name}:{name}] Condition result: {result}");
            }
            else
            {
                Debug.Log($"[{GetType().Name}:{name}] Would evaluate condition (Editor mode)");
            }
        }
        
        [ContextMenu("Reset Condition")]
        private void ResetCondition()
        {
            Reset();
        }
        
        [ContextMenu("Toggle Enabled")]
        private void ToggleEnabled()
        {
            SetEnabled(!isEnabled);
        }
        #endif
        
        #endregion
    }
}