using System;

namespace GameFramework.Core.Interfaces
{
    /// <summary>
    /// Base interface for event channels that provide decoupled communication.
    /// </summary>
    public interface IEventChannel
    {
        /// <summary>
        /// The name of this event channel.
        /// </summary>
        string ChannelName { get; }
        
        /// <summary>
        /// Whether this channel is currently active.
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Reset the channel to its initial state.
        /// </summary>
        void Reset();
    }
    
    /// <summary>
    /// Generic event channel interface for typed events.
    /// </summary>
    /// <typeparam name="T">The type of data this channel transmits</typeparam>
    public interface IEventChannel<T> : IEventChannel
    {
        /// <summary>
        /// Event fired when this channel is raised.
        /// </summary>
        event Action<T> OnEventRaised;
        
        /// <summary>
        /// Raise the event with the provided data.
        /// </summary>
        /// <param name="data">The data to send with the event</param>
        void RaiseEvent(T data);
    }
    
    /// <summary>
    /// Event channel interface for events with no data.
    /// </summary>
    public interface IGameEvent : IEventChannel
    {
        /// <summary>
        /// Event fired when this channel is raised.
        /// </summary>
        event Action OnEventRaised;
        
        /// <summary>
        /// Raise the event.
        /// </summary>
        void RaiseEvent();
    }
}