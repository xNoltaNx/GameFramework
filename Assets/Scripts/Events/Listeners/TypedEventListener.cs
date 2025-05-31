using UnityEngine;
using UnityEngine.Events;
using GameFramework.Events.Channels;

namespace GameFramework.Events.Listeners
{
    /// <summary>
    /// Base class for typed event listeners.
    /// </summary>
    public abstract class TypedEventListener<T, TEvent, TUnityEvent> : MonoBehaviour 
        where TEvent : TypedEventChannel<T>
        where TUnityEvent : UnityEvent<T>, new()
    {
        [Header("Event Configuration")]
        [SerializeField] protected TEvent eventChannel;
        [SerializeField] protected bool autoSubscribe = true;
        
        [Header("Response")]
        [SerializeField] protected TUnityEvent onEventRaised = new TUnityEvent();
        
        [Header("Debug")]
        [SerializeField] protected bool debugMode = false;
        
        protected bool isSubscribed = false;
        
        /// <summary>
        /// The event channel this listener is subscribed to.
        /// </summary>
        public TEvent EventChannel 
        { 
            get => eventChannel; 
            set => SetEventChannel(value); 
        }
        
        /// <summary>
        /// The UnityEvent triggered when the event is raised.
        /// </summary>
        public TUnityEvent OnEventRaised => onEventRaised;
        
        protected virtual void Awake()
        {
            if (autoSubscribe && eventChannel != null)
            {
                Subscribe();
            }
        }
        
        protected virtual void OnEnable()
        {
            if (autoSubscribe && eventChannel != null && !isSubscribed)
            {
                Subscribe();
            }
        }
        
        protected virtual void OnDisable()
        {
            if (isSubscribed)
            {
                Unsubscribe();
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (isSubscribed)
            {
                Unsubscribe();
            }
        }
        
        /// <summary>
        /// Subscribe to the event channel.
        /// </summary>
        public virtual void Subscribe()
        {
            if (eventChannel == null)
            {
                LogWarning("Cannot subscribe - EventChannel is null");
                return;
            }
            
            if (isSubscribed)
            {
                LogDebug("Already subscribed to EventChannel");
                return;
            }
            
            eventChannel.OnEventRaised += HandleEventRaised;
            isSubscribed = true;
            
            LogDebug($"Subscribed to EventChannel: {eventChannel.ChannelName}");
        }
        
        /// <summary>
        /// Unsubscribe from the event channel.
        /// </summary>
        public virtual void Unsubscribe()
        {
            if (eventChannel == null || !isSubscribed)
            {
                return;
            }
            
            eventChannel.OnEventRaised -= HandleEventRaised;
            isSubscribed = false;
            
            LogDebug($"Unsubscribed from EventChannel: {eventChannel.ChannelName}");
        }
        
        /// <summary>
        /// Set the event channel to listen to.
        /// </summary>
        /// <param name="newChannel">The new event channel to listen to</param>
        public virtual void SetEventChannel(TEvent newChannel)
        {
            if (eventChannel == newChannel) return;
            
            if (isSubscribed)
            {
                Unsubscribe();
            }
            
            eventChannel = newChannel;
            
            if (autoSubscribe && eventChannel != null && gameObject.activeInHierarchy)
            {
                Subscribe();
            }
        }
        
        /// <summary>
        /// Handle the event being raised.
        /// </summary>
        protected virtual void HandleEventRaised(T data)
        {
            LogDebug($"Event raised: {eventChannel?.ChannelName} with data: {data}");
            
            try
            {
                onEventRaised?.Invoke(data);
            }
            catch (System.Exception e)
            {
                LogWarning($"Exception in UnityEvent handler: {e.Message}");
                Debug.LogException(e, this);
            }
        }
        
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
        
        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // Ensure we're not subscribed to a different event after inspector changes
            if (Application.isPlaying && isSubscribed && eventChannel != null)
            {
                Unsubscribe();
                Subscribe();
            }
        }
        #endif
    }
    
    /// <summary>
    /// UnityEvent for int values.
    /// </summary>
    [System.Serializable]
    public class IntUnityEvent : UnityEvent<int> { }
    
    /// <summary>
    /// UnityEvent for float values.
    /// </summary>
    [System.Serializable]
    public class FloatUnityEvent : UnityEvent<float> { }
    
    /// <summary>
    /// UnityEvent for string values.
    /// </summary>
    [System.Serializable]
    public class StringUnityEvent : UnityEvent<string> { }
    
    /// <summary>
    /// UnityEvent for Vector3 values.
    /// </summary>
    [System.Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3> { }
    
    /// <summary>
    /// UnityEvent for GameObject values.
    /// </summary>
    [System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
    
    /// <summary>
    /// Listener for int events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Int Event Listener")]
    public class IntEventListener : TypedEventListener<int, IntEventChannel, IntUnityEvent> { }
    
    /// <summary>
    /// Listener for float events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Float Event Listener")]
    public class FloatEventListener : TypedEventListener<float, FloatEventChannel, FloatUnityEvent> { }
    
    /// <summary>
    /// Listener for string events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/String Event Listener")]
    public class StringEventListener : TypedEventListener<string, StringEventChannel, StringUnityEvent> { }
    
    /// <summary>
    /// Listener for Vector3 events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Vector3 Event Listener")]
    public class Vector3EventListener : TypedEventListener<Vector3, Vector3EventChannel, Vector3UnityEvent> { }
    
    /// <summary>
    /// Listener for GameObject events.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/GameObject Event Listener")]
    public class GameObjectEventListener : TypedEventListener<GameObject, GameObjectEventChannel, GameObjectUnityEvent> { }
}