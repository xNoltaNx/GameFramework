using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    /// <summary>
    /// Interface for conditions that can be evaluated by triggers.
    /// </summary>
    public interface ITriggerCondition
    {
        /// <summary>
        /// Whether this condition is currently enabled.
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Evaluate the condition with the provided context.
        /// </summary>
        /// <param name="context">The context object (often the triggering GameObject)</param>
        /// <returns>True if the condition is met, false otherwise</returns>
        bool Evaluate(GameObject context = null);
        
        /// <summary>
        /// Reset the condition to its initial state.
        /// </summary>
        void Reset();
    }
}