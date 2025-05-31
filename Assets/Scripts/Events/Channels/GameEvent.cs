using System;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Channels
{
    /// <summary>
    /// ScriptableObject-based event channel for events with no parameters.
    /// Perfect for simple game events like "player died", "level complete", etc.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/Game Event", fileName = "New Game Event")]
    public class GameEvent : BaseEventChannel, IGameEvent
    {
        /// <summary>
        /// Event fired when this channel is raised.
        /// C# event for performance-critical subscriptions.
        /// </summary>
        public event Action OnEventRaised;
        
        [Header("Event Tracking")]
        [SerializeField, ReadOnly] private int raisedCount;
        [SerializeField, ReadOnly] private float lastRaisedTime;
        
        /// <summary>
        /// Number of times this event has been raised.
        /// </summary>
        public int RaisedCount => raisedCount;
        
        /// <summary>
        /// Time when this event was last raised.
        /// </summary>
        public float LastRaisedTime => lastRaisedTime;
        
        /// <summary>
        /// Raise the event, notifying all subscribers.
        /// </summary>
        public void RaiseEvent()
        {
            if (!isActive)
            {
                LogDebug("Event not raised - channel is inactive");
                return;
            }
            
            raisedCount++;
            lastRaisedTime = Time.time;
            
            LogDebug($"Event raised (Count: {raisedCount})");
            
            try
            {
                OnEventRaised?.Invoke();
            }
            catch (Exception e)
            {
                LogWarning($"Exception in event handler: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// Reset the channel to its initial state.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            raisedCount = 0;
            lastRaisedTime = 0f;
        }
        
        /// <summary>
        /// Get the number of current subscribers.
        /// Useful for debugging and performance monitoring.
        /// </summary>
        public int GetSubscriberCount()
        {
            return OnEventRaised?.GetInvocationList()?.Length ?? 0;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event (Editor Only)")]
        private void RaiseEventEditor()
        {
            if (Application.isPlaying)
            {
                RaiseEvent();
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised (Editor mode)");
            }
        }
        
        [ContextMenu("Reset Event")]
        private void ResetEventEditor()
        {
            Reset();
        }
        #endif
    }
}