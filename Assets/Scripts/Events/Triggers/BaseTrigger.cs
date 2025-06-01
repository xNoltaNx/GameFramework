using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Triggers
{
    /// <summary>
    /// Abstract base class for all trigger components.
    /// Provides common functionality for condition checking and response execution.
    /// </summary>
    public abstract class BaseTrigger : MonoBehaviour, ITrigger
    {
        [Header("Trigger Settings")]
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected bool canRepeat = true;
        [SerializeField] protected float cooldownTime = 0f;
        
        [Header("Conditions")]
        [SerializeField] protected List<MonoBehaviour> conditions = new List<MonoBehaviour>();
        [SerializeField] protected bool requireAllConditions = true;
        
        [Header("Unity Events")]
        [SerializeField] protected UnityEvent onTriggered = new UnityEvent();
        [SerializeField] protected UnityEvent onEnabled = new UnityEvent();
        [SerializeField] protected UnityEvent onDisabled = new UnityEvent();
        
        [Header("Debug")]
        [SerializeField] protected bool debugMode = false;
        
        // Internal state
        protected bool hasFired = false;
        protected float lastFireTime = -1f;
        protected List<ITriggerCondition> triggerConditions = new List<ITriggerCondition>();
        protected List<ITriggerAction> triggerActions = new List<ITriggerAction>();
        
        // C# Events for performance-critical scenarios
        public event Action OnTriggered;
        public event Action OnEnabled;
        public event Action OnDisabled;
        
        #region ITrigger Implementation
        
        /// <summary>
        /// Whether this trigger is currently active and can fire events.
        /// </summary>
        public bool IsActive 
        { 
            get => isActive; 
            set => SetActive(value);
        }
        
        /// <summary>
        /// Whether this trigger can fire multiple times or only once.
        /// </summary>
        public bool CanRepeat => canRepeat;
        
        /// <summary>
        /// Whether this trigger has already fired (for non-repeating triggers).
        /// </summary>
        public bool HasFired => hasFired;
        
        /// <summary>
        /// The GameObject that owns this trigger.
        /// </summary>
        public GameObject TriggerSource => gameObject;
        
        /// <summary>
        /// Public access to the Unity event that fires when the trigger is activated.
        /// </summary>
        public UnityEvent OnTriggeredEvent => onTriggered;
        
        /// <summary>
        /// Enable the trigger.
        /// </summary>
        public virtual void Enable()
        {
            SetActive(true);
        }
        
        /// <summary>
        /// Disable the trigger.
        /// </summary>
        public virtual void Disable()
        {
            SetActive(false);
        }
        
        /// <summary>
        /// Reset the trigger to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            hasFired = false;
            lastFireTime = -1f;
            LogDebug("Trigger reset");
        }
        
        /// <summary>
        /// Force the trigger to fire immediately.
        /// </summary>
        public virtual void ForceFire()
        {
            if (!isActive)
            {
                LogDebug("Cannot force fire - trigger is inactive");
                return;
            }
            
            LogDebug("Force firing trigger");
            ExecuteTrigger(gameObject);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            CacheComponents();
        }
        
        protected virtual void Start()
        {
            Initialize();
        }
        
        protected virtual void OnEnable()
        {
            if (isActive)
            {
                OnTriggerEnabled();
            }
        }
        
        protected virtual void OnDisable()
        {
            OnTriggerDisabled();
        }
        
        #endregion
        
        #region Core Trigger Logic
        
        /// <summary>
        /// Set the active state of the trigger.
        /// </summary>
        /// <param name="active">Whether the trigger should be active</param>
        protected virtual void SetActive(bool active)
        {
            if (isActive == active) return;
            
            isActive = active;
            
            if (active)
            {
                OnTriggerEnabled();
            }
            else
            {
                OnTriggerDisabled();
            }
        }
        
        /// <summary>
        /// Called when the trigger is enabled.
        /// </summary>
        protected virtual void OnTriggerEnabled()
        {
            LogDebug("Trigger enabled");
            
            try
            {
                onEnabled?.Invoke();
                OnEnabled?.Invoke();
            }
            catch (Exception e)
            {
                LogWarning($"Exception in enabled handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Called when the trigger is disabled.
        /// </summary>
        protected virtual void OnTriggerDisabled()
        {
            LogDebug("Trigger disabled");
            
            try
            {
                onDisabled?.Invoke();
                OnDisabled?.Invoke();
            }
            catch (Exception e)
            {
                LogWarning($"Exception in disabled handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Check if the trigger should fire based on conditions and cooldown.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        /// <returns>True if the trigger should fire</returns>
        protected virtual bool ShouldTrigger(GameObject context = null)
        {
            if (!isActive)
            {
                LogDebug("Trigger not fired - inactive");
                return false;
            }
            
            if (!canRepeat && hasFired)
            {
                LogDebug("Trigger not fired - already fired and cannot repeat");
                return false;
            }
            
            if (cooldownTime > 0f && Time.time - lastFireTime < cooldownTime)
            {
                LogDebug($"Trigger not fired - still in cooldown ({Time.time - lastFireTime:F2}s / {cooldownTime:F2}s)");
                return false;
            }
            
            if (!EvaluateConditions(context))
            {
                LogDebug("Trigger not fired - conditions not met");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Execute the trigger, firing all events and actions.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        protected virtual void ExecuteTrigger(GameObject context = null)
        {
            if (!ShouldTrigger(context))
            {
                return;
            }
            
            hasFired = true;
            lastFireTime = Time.time;
            
            LogDebug($"Trigger fired with context: {(context ? context.name : "null")}");
            
            // Fire C# events
            try
            {
                OnTriggered?.Invoke();
            }
            catch (Exception e)
            {
                LogWarning($"Exception in C# event handlers: {e.Message}");
                Debug.LogException(e, this);
            }
            
            // Fire Unity events
            try
            {
                onTriggered?.Invoke();
            }
            catch (Exception e)
            {
                LogWarning($"Exception in Unity event handlers: {e.Message}");
                Debug.LogException(e, this);
            }
            
            // Execute trigger actions
            ExecuteActions(context);
        }
        
        /// <summary>
        /// Evaluate all conditions to determine if the trigger should fire.
        /// </summary>
        /// <param name="context">The context object for condition evaluation</param>
        /// <returns>True if conditions are met</returns>
        protected virtual bool EvaluateConditions(GameObject context = null)
        {
            if (triggerConditions.Count == 0)
            {
                return true; // No conditions means always pass
            }
            
            if (requireAllConditions)
            {
                // All conditions must be true
                foreach (var condition in triggerConditions)
                {
                    if (condition == null || !condition.IsEnabled || !condition.Evaluate(context))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                // At least one condition must be true
                foreach (var condition in triggerConditions)
                {
                    if (condition != null && condition.IsEnabled && condition.Evaluate(context))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        /// <summary>
        /// Execute all trigger actions.
        /// </summary>
        /// <param name="context">The context object for action execution</param>
        protected virtual void ExecuteActions(GameObject context = null)
        {
            foreach (var action in triggerActions)
            {
                if (action != null && action.IsEnabled)
                {
                    try
                    {
                        action.Execute(context);
                    }
                    catch (Exception e)
                    {
                        LogWarning($"Exception executing action {action.GetType().Name}: {e.Message}");
                        Debug.LogException(e, this);
                    }
                }
            }
        }
        
        #endregion
        
        #region Component Management
        
        /// <summary>
        /// Cache condition and action components.
        /// </summary>
        protected virtual void CacheComponents()
        {
            triggerConditions.Clear();
            triggerActions.Clear();
            
            // Cache conditions from the serialized list
            foreach (var conditionMB in conditions)
            {
                if (conditionMB is ITriggerCondition condition)
                {
                    triggerConditions.Add(condition);
                }
            }
            
            // Find all trigger actions on this GameObject
            var actions = GetComponents<ITriggerAction>();
            triggerActions.AddRange(actions);
            
            LogDebug($"Cached {triggerConditions.Count} conditions and {triggerActions.Count} actions");
        }
        
        /// <summary>
        /// Initialize the trigger. Override in derived classes for specific setup.
        /// </summary>
        protected virtual void Initialize()
        {
            LogDebug("Trigger initialized");
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
            if (Application.isPlaying)
            {
                CacheComponents();
            }
        }
        
        [ContextMenu("Test Trigger")]
        private void TestTrigger()
        {
            if (Application.isPlaying)
            {
                ForceFire();
            }
            else
            {
                Debug.Log($"[{GetType().Name}:{name}] Would fire trigger (Editor mode)");
            }
        }
        
        [ContextMenu("Reset Trigger")]
        private void ResetTrigger()
        {
            Reset();
        }
        
        [ContextMenu("Enable Trigger")]
        private void EnableTrigger()
        {
            Enable();
        }
        
        [ContextMenu("Disable Trigger")]
        private void DisableTrigger()
        {
            Disable();
        }
        #endif
        
        #endregion
    }
}