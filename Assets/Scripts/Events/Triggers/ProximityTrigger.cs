using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.Events.Triggers
{
    /// <summary>
    /// Trigger that fires based on distance to target objects.
    /// Supports enter/exit range detection with customizable ranges and targets.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Proximity Trigger")]
    public class ProximityTrigger : BaseTrigger
    {
        [System.Serializable]
        public enum ProximityEvent
        {
            OnEnterRange,
            OnExitRange,
            OnWithinRange
        }
        
        [System.Serializable]
        public enum TargetMode
        {
            SpecificObjects,
            FindByTag,
            FindByLayer,
            FindByComponent
        }
        
        [Header("Proximity Settings")]
        [SerializeField] private ProximityEvent proximityEvent = ProximityEvent.OnEnterRange;
        [SerializeField] private float triggerDistance = 5f;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private bool use3DDistance = true;
        
        [Header("Target Settings")]
        [SerializeField] private TargetMode targetMode = TargetMode.SpecificObjects;
        [SerializeField] private List<GameObject> specificTargets = new List<GameObject>();
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private string targetComponentType = "";
        
        [Header("Proximity Events")]
        [SerializeField] private UnityEvent<GameObject> onObjectEnteredRange;
        [SerializeField] private UnityEvent<GameObject> onObjectExitedRange;
        [SerializeField] private UnityEvent<GameObject> onObjectWithinRange;
        
        [Header("Range Visualization")]
        [SerializeField] private bool showRangeGizmo = true;
        [SerializeField] private Color rangeColor = Color.yellow;
        
        private HashSet<GameObject> objectsInRange = new HashSet<GameObject>();
        private float lastCheckTime;
        
        // C# Events for performance-critical scenarios
        public event System.Action<GameObject> OnObjectEnteredRange;
        public event System.Action<GameObject> OnObjectExitedRange;
        public event System.Action<GameObject> OnObjectWithinRange;
        
        protected override void Start()
        {
            base.Start();
            lastCheckTime = Time.time;
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckProximity();
                lastCheckTime = Time.time;
            }
        }
        
        /// <summary>
        /// Check proximity to all target objects.
        /// </summary>
        private void CheckProximity()
        {
            var targets = GetTargetObjects();
            var currentlyInRange = new HashSet<GameObject>();
            
            foreach (var target in targets)
            {
                if (target == null || target == gameObject) continue;
                
                float distance = CalculateDistance(target);
                bool withinRange = distance <= triggerDistance;
                
                if (withinRange)
                {
                    currentlyInRange.Add(target);
                    
                    // Check for enter event
                    if (!objectsInRange.Contains(target))
                    {
                        HandleProximityEvent(target, ProximityEvent.OnEnterRange);
                    }
                    
                    // Check for within range event
                    if (proximityEvent == ProximityEvent.OnWithinRange)
                    {
                        HandleProximityEvent(target, ProximityEvent.OnWithinRange);
                    }
                }
            }
            
            // Check for exit events
            var exitedObjects = new List<GameObject>();
            foreach (var obj in objectsInRange)
            {
                if (!currentlyInRange.Contains(obj))
                {
                    exitedObjects.Add(obj);
                }
            }
            
            foreach (var obj in exitedObjects)
            {
                HandleProximityEvent(obj, ProximityEvent.OnExitRange);
            }
            
            objectsInRange = currentlyInRange;
        }
        
        /// <summary>
        /// Handle proximity events based on the configured event type.
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="eventType">The type of proximity event</param>
        private void HandleProximityEvent(GameObject target, ProximityEvent eventType)
        {
            if (proximityEvent != eventType) return;
            
            LogDebug($"Proximity {eventType}: {target.name} (distance: {CalculateDistance(target):F2})");
            
            ExecuteTrigger(target);
            FireProximityEvents(target, eventType);
        }
        
        /// <summary>
        /// Fire the appropriate proximity events.
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="eventType">The type of proximity event</param>
        private void FireProximityEvents(GameObject target, ProximityEvent eventType)
        {
            try
            {
                switch (eventType)
                {
                    case ProximityEvent.OnEnterRange:
                        onObjectEnteredRange?.Invoke(target);
                        OnObjectEnteredRange?.Invoke(target);
                        break;
                        
                    case ProximityEvent.OnExitRange:
                        onObjectExitedRange?.Invoke(target);
                        OnObjectExitedRange?.Invoke(target);
                        break;
                        
                    case ProximityEvent.OnWithinRange:
                        onObjectWithinRange?.Invoke(target);
                        OnObjectWithinRange?.Invoke(target);
                        break;
                }
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in proximity event handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Calculate distance to target object.
        /// </summary>
        /// <param name="target">The target object</param>
        /// <returns>The distance to the target</returns>
        private float CalculateDistance(GameObject target)
        {
            Vector3 thisPos = transform.position;
            Vector3 targetPos = target.transform.position;
            
            if (use3DDistance)
            {
                return Vector3.Distance(thisPos, targetPos);
            }
            else
            {
                // 2D distance (ignore Y axis)
                thisPos.y = 0f;
                targetPos.y = 0f;
                return Vector3.Distance(thisPos, targetPos);
            }
        }
        
        /// <summary>
        /// Get target objects based on the configured target mode.
        /// </summary>
        /// <returns>List of target objects</returns>
        private List<GameObject> GetTargetObjects()
        {
            var targets = new List<GameObject>();
            
            switch (targetMode)
            {
                case TargetMode.SpecificObjects:
                    targets.AddRange(specificTargets);
                    break;
                    
                case TargetMode.FindByTag:
                    if (!string.IsNullOrEmpty(targetTag))
                    {
                        var taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
                        targets.AddRange(taggedObjects);
                    }
                    break;
                    
                case TargetMode.FindByLayer:
                    var allObjects = FindObjectsOfType<GameObject>();
                    foreach (var obj in allObjects)
                    {
                        if ((targetLayers.value & (1 << obj.layer)) != 0)
                        {
                            targets.Add(obj);
                        }
                    }
                    break;
                    
                case TargetMode.FindByComponent:
                    if (!string.IsNullOrEmpty(targetComponentType))
                    {
                        var componentType = System.Type.GetType(targetComponentType);
                        if (componentType != null)
                        {
                            var components = FindObjectsOfType(componentType);
                            foreach (var comp in components)
                            {
                                if (comp is MonoBehaviour mb)
                                {
                                    targets.Add(mb.gameObject);
                                }
                            }
                        }
                    }
                    break;
            }
            
            return targets;
        }
        
        #region Public API
        
        /// <summary>
        /// Set the proximity event type.
        /// </summary>
        /// <param name="eventType">The new proximity event type</param>
        public void SetProximityEvent(ProximityEvent eventType)
        {
            proximityEvent = eventType;
            LogDebug($"Proximity event set to: {eventType}");
        }
        
        /// <summary>
        /// Set the trigger distance.
        /// </summary>
        /// <param name="distance">The new trigger distance</param>
        public void SetTriggerDistance(float distance)
        {
            triggerDistance = Mathf.Max(0f, distance);
            LogDebug($"Trigger distance set to: {triggerDistance}");
        }
        
        /// <summary>
        /// Set the check interval.
        /// </summary>
        /// <param name="interval">The new check interval in seconds</param>
        public void SetCheckInterval(float interval)
        {
            checkInterval = Mathf.Max(0.01f, interval);
            LogDebug($"Check interval set to: {checkInterval}");
        }
        
        /// <summary>
        /// Add a specific target object.
        /// </summary>
        /// <param name="target">The target to add</param>
        public void AddTarget(GameObject target)
        {
            if (target != null && !specificTargets.Contains(target))
            {
                specificTargets.Add(target);
                LogDebug($"Added target: {target.name}");
            }
        }
        
        /// <summary>
        /// Remove a specific target object.
        /// </summary>
        /// <param name="target">The target to remove</param>
        public void RemoveTarget(GameObject target)
        {
            if (specificTargets.Remove(target))
            {
                LogDebug($"Removed target: {target.name}");
            }
        }
        
        /// <summary>
        /// Get all objects currently in range.
        /// </summary>
        /// <returns>Array of objects currently in range</returns>
        public GameObject[] GetObjectsInRange()
        {
            var array = new GameObject[objectsInRange.Count];
            objectsInRange.CopyTo(array);
            return array;
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showRangeGizmo) return;
            
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            
            // Draw line to objects in range
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                foreach (var obj in objectsInRange)
                {
                    if (obj != null)
                    {
                        Gizmos.DrawLine(transform.position, obj.transform.position);
                    }
                }
            }
        }
        
        [ContextMenu("Force Check Proximity")]
        private void ForceCheckProximity()
        {
            if (Application.isPlaying)
            {
                CheckProximity();
            }
        }
        #endif
        
        #endregion
    }
}