using System;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Channels
{
    /// <summary>
    /// Generic ScriptableObject-based event channel for typed events.
    /// Base class for all typed event channels.
    /// </summary>
    /// <typeparam name="T">The type of data this channel transmits</typeparam>
    public abstract class TypedEventChannel<T> : BaseEventChannel, IEventChannel<T>
    {
        /// <summary>
        /// Event fired when this channel is raised.
        /// C# event for performance-critical subscriptions.
        /// </summary>
        public event Action<T> OnEventRaised;
        
        [Header("Event Tracking")]
        [SerializeField, ReadOnly] private int raisedCount;
        [SerializeField, ReadOnly] private float lastRaisedTime;
        [SerializeField, ReadOnly] private string lastDataString;
        
        /// <summary>
        /// Number of times this event has been raised.
        /// </summary>
        public int RaisedCount => raisedCount;
        
        /// <summary>
        /// Time when this event was last raised.
        /// </summary>
        public float LastRaisedTime => lastRaisedTime;
        
        /// <summary>
        /// String representation of the last data sent.
        /// </summary>
        public string LastDataString => lastDataString;
        
        /// <summary>
        /// Raise the event with the provided data.
        /// </summary>
        /// <param name="data">The data to send with the event</param>
        public void RaiseEvent(T data)
        {
            if (!isActive)
            {
                LogDebug($"Event not raised - channel is inactive. Data: {data}");
                return;
            }
            
            raisedCount++;
            lastRaisedTime = Time.time;
            lastDataString = data?.ToString() ?? "null";
            
            LogDebug($"Event raised with data: {data} (Count: {raisedCount})");
            
            try
            {
                OnEventRaised?.Invoke(data);
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
            lastDataString = string.Empty;
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
        [ContextMenu("Reset Event")]
        private void ResetEventEditor()
        {
            Reset();
        }
        #endif
    }
    
    /// <summary>
    /// Event channel for integer values.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/Int Event", fileName = "New Int Event")]
    public class IntEventChannel : TypedEventChannel<int>
    {
        [Header("Test Data")]
        [SerializeField] private int testValue = 0;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event with Test Value")]
        private void RaiseTestEvent()
        {
            if (Application.isPlaying)
            {
                RaiseEvent(testValue);
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised with value: {testValue} (Editor mode)");
            }
        }
        #endif
    }
    
    /// <summary>
    /// Event channel for float values.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/Float Event", fileName = "New Float Event")]
    public class FloatEventChannel : TypedEventChannel<float>
    {
        [Header("Test Data")]
        [SerializeField] private float testValue = 0f;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event with Test Value")]
        private void RaiseTestEvent()
        {
            if (Application.isPlaying)
            {
                RaiseEvent(testValue);
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised with value: {testValue} (Editor mode)");
            }
        }
        #endif
    }
    
    /// <summary>
    /// Event channel for string values.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/String Event", fileName = "New String Event")]
    public class StringEventChannel : TypedEventChannel<string>
    {
        [Header("Test Data")]
        [SerializeField] private string testValue = "";
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event with Test Value")]
        private void RaiseTestEvent()
        {
            if (Application.isPlaying)
            {
                RaiseEvent(testValue);
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised with value: {testValue} (Editor mode)");
            }
        }
        #endif
    }
    
    /// <summary>
    /// Event channel for Vector3 values.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/Vector3 Event", fileName = "New Vector3 Event")]
    public class Vector3EventChannel : TypedEventChannel<Vector3>
    {
        [Header("Test Data")]
        [SerializeField] private Vector3 testValue = Vector3.zero;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event with Test Value")]
        private void RaiseTestEvent()
        {
            if (Application.isPlaying)
            {
                RaiseEvent(testValue);
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised with value: {testValue} (Editor mode)");
            }
        }
        #endif
    }
    
    /// <summary>
    /// Event channel for GameObject references.
    /// </summary>
    [CreateAssetMenu(menuName = "GameFramework/Events/GameObject Event", fileName = "New GameObject Event")]
    public class GameObjectEventChannel : TypedEventChannel<GameObject>
    {
        [Header("Test Data")]
        [SerializeField] private GameObject testValue;
        
        #if UNITY_EDITOR
        [ContextMenu("Raise Event with Test Value")]
        private void RaiseTestEvent()
        {
            if (Application.isPlaying)
            {
                RaiseEvent(testValue);
            }
            else
            {
                Debug.Log($"[{ChannelName}] Event would be raised with GameObject: {(testValue ? testValue.name : "null")} (Editor mode)");
            }
        }
        #endif
    }
}