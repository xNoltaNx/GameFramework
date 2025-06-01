using System;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Attribute that defines metadata for trigger actions.
    /// Used by the action discovery system to automatically find and configure actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActionDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Unique identifier for this action type.
        /// </summary>
        public string ActionId { get; }
        
        /// <summary>
        /// Icon emoji displayed in the UI.
        /// </summary>
        public string Icon { get; }
        
        /// <summary>
        /// Display name shown in the wizard.
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// Description of what this action does.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Category for grouping actions in the UI.
        /// </summary>
        public string Category { get; }
        
        /// <summary>
        /// Priority for sorting actions (lower numbers appear first).
        /// </summary>
        public int Priority { get; }
        
        public ActionDefinitionAttribute(
            string actionId, 
            string icon, 
            string displayName, 
            string description = "", 
            string category = "General",
            int priority = 100)
        {
            ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
            Icon = icon ?? "🎯";
            DisplayName = displayName ?? actionId;
            Description = description;
            Category = category;
            Priority = priority;
        }
    }
}