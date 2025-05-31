using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for actions that can be executed by triggers.
    /// </summary>
    public interface ITriggerAction
    {
        /// <summary>
        /// Whether this action is currently enabled.
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// The delay before this action executes.
        /// </summary>
        float ExecutionDelay { get; }
        
        /// <summary>
        /// Execute the action with the provided context.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        void Execute(GameObject context = null);
        
        /// <summary>
        /// Stop the action if it's currently executing.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Whether the action is currently running.
        /// </summary>
        bool IsExecuting { get; }
    }
}