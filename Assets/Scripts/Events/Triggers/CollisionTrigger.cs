using UnityEngine;
using UnityEngine.Events;

namespace GameFramework.Events.Triggers
{
    /// <summary>
    /// Trigger that fires based on physics collision events.
    /// Supports OnTriggerEnter, OnTriggerExit, and OnTriggerStay events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Collision Trigger")]
    [RequireComponent(typeof(Collider))]
    public class CollisionTrigger : BaseTrigger
    {
        [System.Serializable]
        public enum TriggerEvent
        {
            OnEnter,
            OnExit,
            OnStay
        }
        
        [Header("Inspector Display")]
        [SerializeField] private bool showTriggerSettings = true;
        [SerializeField] private bool showCollisionSettings = true;
        
        [Header("Collision Settings")]
        [SerializeField] private TriggerEvent triggerEvent = TriggerEvent.OnEnter;
        [SerializeField] private LayerMask triggerLayers = -1;
        [SerializeField] private string requiredTag = "";
        [SerializeField] private bool requireRigidbody = false;
        
        [Header("Collision Events")]
        [SerializeField] private UnityEvent<GameObject> onObjectEntered;
        [SerializeField] private UnityEvent<GameObject> onObjectExited;
        [SerializeField] private UnityEvent<GameObject> onObjectStaying;
        
        private Collider triggerCollider;
        
        // C# Events for performance-critical scenarios
        public event System.Action<GameObject> OnObjectEntered;
        public event System.Action<GameObject> OnObjectExited;
        public event System.Action<GameObject> OnObjectStaying;
        
        protected override void Awake()
        {
            base.Awake();
            
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
            else
            {
                LogWarning("No Collider component found! Collision trigger will not work.");
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (IsValidCollision(other))
            {
                LogDebug($"Collision Enter: {other.name}");
                
                // Always fire collision events if they have listeners
                FireCollisionEvents(other.gameObject, TriggerEvent.OnEnter);
                
                // Only execute main trigger logic if this is the selected trigger event
                if (triggerEvent == TriggerEvent.OnEnter)
                {
                    ExecuteTrigger(other.gameObject);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (IsValidCollision(other))
            {
                LogDebug($"Collision Exit: {other.name}");
                
                // Always fire collision events if they have listeners
                FireCollisionEvents(other.gameObject, TriggerEvent.OnExit);
                
                // Only execute main trigger logic if this is the selected trigger event
                if (triggerEvent == TriggerEvent.OnExit)
                {
                    ExecuteTrigger(other.gameObject);
                }
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (IsValidCollision(other))
            {
                LogDebug($"Collision Stay: {other.name}");
                
                // Always fire collision events if they have listeners
                FireCollisionEvents(other.gameObject, TriggerEvent.OnStay);
                
                // Only execute main trigger logic if this is the selected trigger event
                if (triggerEvent == TriggerEvent.OnStay)
                {
                    ExecuteTrigger(other.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Check if the collision is valid based on layer mask, tag, and rigidbody requirements.
        /// </summary>
        /// <param name="other">The collider that triggered the event</param>
        /// <returns>True if the collision is valid</returns>
        private bool IsValidCollision(Collider other)
        {
            if (other == null) return false;
            
            // Check layer mask
            if ((triggerLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                LogDebug($"Collision rejected - wrong layer: {LayerMask.LayerToName(other.gameObject.layer)}");
                return false;
            }
            
            // Check required tag
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            {
                LogDebug($"Collision rejected - wrong tag: {other.tag} (required: {requiredTag})");
                return false;
            }
            
            // Check rigidbody requirement
            if (requireRigidbody && other.attachedRigidbody == null)
            {
                LogDebug("Collision rejected - no rigidbody");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Fire the appropriate collision events.
        /// </summary>
        /// <param name="collisionObject">The object that caused the collision</param>
        /// <param name="eventType">The type of collision event</param>
        private void FireCollisionEvents(GameObject collisionObject, TriggerEvent eventType)
        {
            try
            {
                switch (eventType)
                {
                    case TriggerEvent.OnEnter:
                        onObjectEntered?.Invoke(collisionObject);
                        OnObjectEntered?.Invoke(collisionObject);
                        break;
                        
                    case TriggerEvent.OnExit:
                        onObjectExited?.Invoke(collisionObject);
                        OnObjectExited?.Invoke(collisionObject);
                        break;
                        
                    case TriggerEvent.OnStay:
                        onObjectStaying?.Invoke(collisionObject);
                        OnObjectStaying?.Invoke(collisionObject);
                        break;
                }
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in collision event handlers: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
        /// <summary>
        /// Set the trigger event type.
        /// </summary>
        /// <param name="eventType">The new trigger event type</param>
        public void SetTriggerEvent(TriggerEvent eventType)
        {
            triggerEvent = eventType;
            LogDebug($"Trigger event set to: {eventType}");
        }
        
        /// <summary>
        /// Set the layer mask for valid collisions.
        /// </summary>
        /// <param name="layers">The layer mask</param>
        public void SetTriggerLayers(LayerMask layers)
        {
            triggerLayers = layers;
            LogDebug($"Trigger layers set to: {layers.value}");
        }
        
        /// <summary>
        /// Set the required tag for valid collisions.
        /// </summary>
        /// <param name="tag">The required tag (empty string for no requirement)</param>
        public void SetRequiredTag(string tag)
        {
            requiredTag = tag;
            LogDebug($"Required tag set to: '{tag}'");
        }
        
        /// <summary>
        /// Set whether a rigidbody is required for valid collisions.
        /// </summary>
        /// <param name="required">Whether a rigidbody is required</param>
        public void SetRequireRigidbody(bool required)
        {
            requireRigidbody = required;
            LogDebug($"Require rigidbody set to: {required}");
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (triggerCollider != null)
            {
                Gizmos.color = isActive ? Color.green : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (triggerCollider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (triggerCollider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
                else if (triggerCollider is CapsuleCollider capsule)
                {
                    // Draw a simple representation for capsule
                    Gizmos.DrawWireSphere(capsule.center, capsule.radius);
                }
            }
        }
        #endif
    }
}