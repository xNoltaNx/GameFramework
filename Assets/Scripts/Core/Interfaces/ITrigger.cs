using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    /// <summary>
    /// Base interface for all trigger components.
    /// Provides common functionality for condition checking and event firing.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Whether this trigger is currently active and can fire events.
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Whether this trigger can fire multiple times or only once.
        /// </summary>
        bool CanRepeat { get; }
        
        /// <summary>
        /// Whether this trigger has already fired (for non-repeating triggers).
        /// </summary>
        bool HasFired { get; }
        
        /// <summary>
        /// The GameObject that owns this trigger.
        /// </summary>
        GameObject TriggerSource { get; }
        
        /// <summary>
        /// Enable the trigger.
        /// </summary>
        void Enable();
        
        /// <summary>
        /// Disable the trigger.
        /// </summary>
        void Disable();
        
        /// <summary>
        /// Reset the trigger to its initial state.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Force the trigger to fire immediately.
        /// </summary>
        void ForceFire();
    }
}