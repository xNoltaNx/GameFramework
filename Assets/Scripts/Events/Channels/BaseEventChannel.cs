using System;
using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Channels
{
    /// <summary>
    /// Abstract base class for all event channels.
    /// Provides common functionality for ScriptableObject-based event communication.
    /// </summary>
    public abstract class BaseEventChannel : ScriptableObject, IEventChannel
    {
        [Header("Channel Settings")]
        [SerializeField] protected string channelName;
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected bool debugMode = false;
        
        [Header("Description")]
        [SerializeField, TextArea(3, 5)] protected string description;
        
        /// <summary>
        /// The name of this event channel.
        /// </summary>
        public string ChannelName => string.IsNullOrEmpty(channelName) ? name : channelName;
        
        /// <summary>
        /// Whether this channel is currently active.
        /// </summary>
        public bool IsActive 
        { 
            get => isActive; 
            set => isActive = value; 
        }
        
        /// <summary>
        /// Whether debug logging is enabled for this channel.
        /// </summary>
        public bool DebugMode 
        { 
            get => debugMode; 
            set => debugMode = value; 
        }
        
        /// <summary>
        /// Reset the channel to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            if (debugMode)
            {
                Debug.Log($"[EventChannel] Reset: {ChannelName}");
            }
        }
        
        /// <summary>
        /// Log a debug message if debug mode is enabled.
        /// </summary>
        /// <param name="message">The message to log</param>
        protected void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[{ChannelName}] {message}");
            }
        }
        
        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="message">The message to log</param>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{ChannelName}] {message}");
        }
        
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(channelName))
            {
                channelName = name;
            }
        }
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(channelName))
            {
                channelName = name;
            }
        }
    }
}