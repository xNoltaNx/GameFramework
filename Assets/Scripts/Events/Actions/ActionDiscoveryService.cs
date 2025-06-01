#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Data class representing a discovered action definition.
    /// </summary>
    public class ActionDefinition
    {
        public string ActionId { get; set; }
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public Type ComponentType { get; set; }
        
        public ActionDefinition(string actionId, string icon, string displayName, string description, string category, int priority, Type componentType)
        {
            ActionId = actionId;
            Icon = icon;
            DisplayName = displayName;
            Description = description;
            Category = category;
            Priority = priority;
            ComponentType = componentType;
        }
    }
    
    /// <summary>
    /// Service for automatically discovering and managing trigger actions using reflection.
    /// </summary>
    public static class ActionDiscoveryService
    {
        private static Dictionary<string, ActionDefinition> _actionDefinitions;
        private static List<ActionDefinition> _sortedActions;
        
        /// <summary>
        /// Gets all discovered action definitions, indexed by action ID.
        /// </summary>
        public static Dictionary<string, ActionDefinition> GetAllActions()
        {
            if (_actionDefinitions == null)
            {
                DiscoverActions();
            }
            return _actionDefinitions;
        }
        
        /// <summary>
        /// Gets all discovered actions sorted by priority and name.
        /// </summary>
        public static List<ActionDefinition> GetSortedActions()
        {
            if (_sortedActions == null)
            {
                var actions = GetAllActions().Values;
                _sortedActions = actions.OrderBy(a => a.Priority).ThenBy(a => a.DisplayName).ToList();
            }
            return _sortedActions;
        }
        
        /// <summary>
        /// Gets action definitions for a specific category.
        /// </summary>
        public static List<ActionDefinition> GetActionsByCategory(string category)
        {
            return GetSortedActions().Where(a => a.Category == category).ToList();
        }
        
        /// <summary>
        /// Gets all unique categories from discovered actions.
        /// </summary>
        public static List<string> GetCategories()
        {
            return GetAllActions().Values.Select(a => a.Category).Distinct().OrderBy(c => c).ToList();
        }
        
        /// <summary>
        /// Creates an action component on the target GameObject.
        /// </summary>
        public static BaseTriggerAction CreateActionComponent(GameObject target, string actionId)
        {
            if (target == null)
            {
                Debug.LogError("Cannot create action component: target GameObject is null");
                return null;
            }
            
            var actions = GetAllActions();
            if (!actions.TryGetValue(actionId, out var actionDef))
            {
                Debug.LogError($"Action with ID '{actionId}' not found in discovered actions");
                return null;
            }
            
            try
            {
                var component = target.AddComponent(actionDef.ComponentType) as BaseTriggerAction;
                if (component == null)
                {
                    Debug.LogError($"Failed to cast component to BaseTriggerAction for action '{actionId}'");
                }
                return component;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception creating action component '{actionId}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets an action definition by ID.
        /// </summary>
        public static ActionDefinition GetAction(string actionId)
        {
            var actions = GetAllActions();
            return actions.TryGetValue(actionId, out var action) ? action : null;
        }
        
        /// <summary>
        /// Forces a refresh of the action discovery cache.
        /// Call this when new actions are added or assemblies are reloaded.
        /// </summary>
        public static void RefreshActions()
        {
            _actionDefinitions = null;
            _sortedActions = null;
            DiscoverActions();
        }
        
        private static void DiscoverActions()
        {
            _actionDefinitions = new Dictionary<string, ActionDefinition>();
            
            try
            {
                // Get all loaded assemblies
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Get all types that inherit from BaseTriggerAction
                        var actionTypes = assembly.GetTypes()
                            .Where(type => type.IsSubclassOf(typeof(BaseTriggerAction)) && !type.IsAbstract)
                            .ToList();
                        
                        foreach (var actionType in actionTypes)
                        {
                            var attribute = actionType.GetCustomAttribute<ActionDefinitionAttribute>();
                            if (attribute != null)
                            {
                                // Use attribute data
                                var definition = new ActionDefinition(
                                    attribute.ActionId,
                                    attribute.Icon,
                                    attribute.DisplayName,
                                    attribute.Description,
                                    attribute.Category,
                                    attribute.Priority,
                                    actionType
                                );
                                
                                if (_actionDefinitions.ContainsKey(attribute.ActionId))
                                {
                                    Debug.LogWarning($"Duplicate action ID '{attribute.ActionId}' found in types {_actionDefinitions[attribute.ActionId].ComponentType.Name} and {actionType.Name}. Using first occurrence.");
                                }
                                else
                                {
                                    _actionDefinitions[attribute.ActionId] = definition;
                                }
                            }
                            else
                            {
                                // Create fallback definition for actions without attributes
                                var actionId = actionType.Name.Replace("Action", "").ToLowerInvariant();
                                var displayName = actionType.Name.Replace("Action", " Action");
                                
                                // Don't add if we already have an action with this ID
                                if (!_actionDefinitions.ContainsKey(actionId))
                                {
                                    var definition = new ActionDefinition(
                                        actionId,
                                        "🎯", // Default icon
                                        displayName,
                                        $"Action component: {actionType.Name}",
                                        "Other",
                                        999, // Low priority for non-attributed actions
                                        actionType
                                    );
                                    
                                    _actionDefinitions[actionId] = definition;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip assemblies that can't be loaded (common in Unity)
                        Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
                    }
                }
                
                Debug.Log($"Action Discovery: Found {_actionDefinitions.Count} action definitions");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during action discovery: {ex.Message}");
                // Ensure we have an empty dictionary even if discovery fails
                _actionDefinitions = new Dictionary<string, ActionDefinition>();
            }
        }
    }
}
#endif