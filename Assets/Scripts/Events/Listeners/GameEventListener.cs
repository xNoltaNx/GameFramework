using UnityEngine;
using UnityEngine.Events;
using GameFramework.Events.Channels;

namespace GameFramework.Events.Listeners
{
    /// <summary>
    /// MonoBehaviour component that listens to GameEvent channels and triggers UnityEvents.
    /// Provides designer-friendly drag-and-drop event configuration.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Game Event Listener")]
    public class GameEventListener : MonoBehaviour
    {
        [Header("Event Configuration")]
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private bool autoSubscribe = true;
        
        [Header("Response")]
        [SerializeField] private UnityEvent onEventRaised;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private bool isSubscribed = false;
        
        /// <summary>
        /// The GameEvent this listener is subscribed to.
        /// </summary>
        public GameEvent GameEvent 
        { 
            get => gameEvent; 
            set => SetGameEvent(value); 
        }
        
        /// <summary>
        /// The UnityEvent triggered when the GameEvent is raised.
        /// </summary>
        public UnityEvent OnEventRaised => onEventRaised;
        
        private void Awake()
        {
            if (autoSubscribe && gameEvent != null)
            {
                Subscribe();
            }
        }
        
        private void OnEnable()
        {
            if (autoSubscribe && gameEvent != null && !isSubscribed)
            {
                Subscribe();
            }
        }
        
        private void OnDisable()
        {
            if (isSubscribed)
            {
                Unsubscribe();
            }
        }
        
        private void OnDestroy()
        {
            if (isSubscribed)
            {
                Unsubscribe();
            }
        }
        
        /// <summary>
        /// Subscribe to the GameEvent.
        /// </summary>
        public void Subscribe()
        {
            if (gameEvent == null)
            {
                LogWarning("Cannot subscribe - GameEvent is null");
                return;
            }
            
            if (isSubscribed)
            {
                LogDebug("Already subscribed to GameEvent");
                return;
            }
            
            gameEvent.OnEventRaised += HandleEventRaised;
            isSubscribed = true;
            
            LogDebug($"Subscribed to GameEvent: {gameEvent.ChannelName}");
        }
        
        /// <summary>
        /// Unsubscribe from the GameEvent.
        /// </summary>
        public void Unsubscribe()
        {
            if (gameEvent == null || !isSubscribed)
            {
                return;
            }
            
            gameEvent.OnEventRaised -= HandleEventRaised;
            isSubscribed = false;
            
            LogDebug($"Unsubscribed from GameEvent: {gameEvent.ChannelName}");
        }
        
        /// <summary>
        /// Set the GameEvent to listen to.
        /// </summary>
        /// <param name="newEvent">The new GameEvent to listen to</param>
        public void SetGameEvent(GameEvent newEvent)
        {
            if (gameEvent == newEvent) return;
            
            if (isSubscribed)
            {
                Unsubscribe();
            }
            
            gameEvent = newEvent;
            
            if (autoSubscribe && gameEvent != null && gameObject.activeInHierarchy)
            {
                Subscribe();
            }
        }
        
        /// <summary>
        /// Handle the event being raised.
        /// </summary>
        private void HandleEventRaised()
        {
            LogDebug($"Event raised: {gameEvent?.ChannelName}");
            
            try
            {
                onEventRaised?.Invoke();
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
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[GameEventListener:{name}] {message}", this);
            }
        }
        
        /// <summary>
        /// Log a warning message.
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GameEventListener:{name}] {message}", this);
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure we're not subscribed to a different event after inspector changes
            if (Application.isPlaying && isSubscribed && gameEvent != null)
            {
                Unsubscribe();
                Subscribe();
            }
        }
        #endif
    }
}