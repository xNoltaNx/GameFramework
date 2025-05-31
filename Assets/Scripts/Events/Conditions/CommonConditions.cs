using UnityEngine;
using System.Collections.Generic;

namespace GameFramework.Events.Conditions
{
    /// <summary>
    /// Condition that checks if a GameObject has a specific tag.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Tag Condition")]
    public class TagCondition : BaseTriggerCondition
    {
        [Header("Tag Settings")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool useContext = true;
        [SerializeField] private GameObject targetObject;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            GameObject target = useContext ? context : targetObject;
            
            if (target == null)
            {
                LogDebug("Target is null");
                return false;
            }
            
            bool hasTag = target.CompareTag(requiredTag);
            LogDebug($"Checking tag '{requiredTag}' on {target.name}: {hasTag}");
            
            return hasTag;
        }
        
        public void SetRequiredTag(string tag)
        {
            requiredTag = tag;
        }
        
        public void SetTargetObject(GameObject target)
        {
            targetObject = target;
            useContext = false;
        }
        
        public void SetUseContext(bool use)
        {
            useContext = use;
        }
    }
    
    /// <summary>
    /// Condition that checks if a GameObject is on a specific layer.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Layer Condition")]
    public class LayerCondition : BaseTriggerCondition
    {
        [Header("Layer Settings")]
        [SerializeField] private LayerMask allowedLayers = -1;
        [SerializeField] private bool useContext = true;
        [SerializeField] private GameObject targetObject;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            GameObject target = useContext ? context : targetObject;
            
            if (target == null)
            {
                LogDebug("Target is null");
                return false;
            }
            
            bool onAllowedLayer = (allowedLayers.value & (1 << target.layer)) != 0;
            LogDebug($"Checking layer {LayerMask.LayerToName(target.layer)} on {target.name}: {onAllowedLayer}");
            
            return onAllowedLayer;
        }
        
        public void SetAllowedLayers(LayerMask layers)
        {
            allowedLayers = layers;
        }
        
        public void SetTargetObject(GameObject target)
        {
            targetObject = target;
            useContext = false;
        }
        
        public void SetUseContext(bool use)
        {
            useContext = use;
        }
    }
    
    /// <summary>
    /// Condition that checks distance between two objects.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Distance Condition")]
    public class DistanceCondition : BaseTriggerCondition
    {
        [System.Serializable]
        public enum ComparisonType
        {
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            Equal,
            NotEqual
        }
        
        [Header("Distance Settings")]
        [SerializeField] private Transform referencePoint;
        [SerializeField] private float targetDistance = 5f;
        [SerializeField] private ComparisonType comparison = ComparisonType.LessThan;
        [SerializeField] private bool use3DDistance = true;
        [SerializeField] private bool useContext = true;
        [SerializeField] private GameObject targetObject;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            if (referencePoint == null)
            {
                LogWarning("Reference point is null");
                return false;
            }
            
            GameObject target = useContext ? context : targetObject;
            if (target == null)
            {
                LogDebug("Target is null");
                return false;
            }
            
            float distance = CalculateDistance(referencePoint.position, target.transform.position);
            bool result = CompareDistance(distance, targetDistance);
            
            LogDebug($"Distance {distance:F2} {comparison} {targetDistance:F2}: {result}");
            
            return result;
        }
        
        private float CalculateDistance(Vector3 pos1, Vector3 pos2)
        {
            if (use3DDistance)
            {
                return Vector3.Distance(pos1, pos2);
            }
            else
            {
                pos1.y = 0f;
                pos2.y = 0f;
                return Vector3.Distance(pos1, pos2);
            }
        }
        
        private bool CompareDistance(float actual, float target)
        {
            const float epsilon = 0.01f;
            
            switch (comparison)
            {
                case ComparisonType.LessThan:
                    return actual < target;
                case ComparisonType.LessThanOrEqual:
                    return actual <= target;
                case ComparisonType.GreaterThan:
                    return actual > target;
                case ComparisonType.GreaterThanOrEqual:
                    return actual >= target;
                case ComparisonType.Equal:
                    return Mathf.Abs(actual - target) < epsilon;
                case ComparisonType.NotEqual:
                    return Mathf.Abs(actual - target) >= epsilon;
                default:
                    return false;
            }
        }
        
        public void SetReferencePoint(Transform point)
        {
            referencePoint = point;
        }
        
        public void SetTargetDistance(float distance)
        {
            targetDistance = distance;
        }
        
        public void SetComparison(ComparisonType comp)
        {
            comparison = comp;
        }
    }
    
    /// <summary>
    /// Condition that checks if a GameObject has a specific component.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Component Condition")]
    public class ComponentCondition : BaseTriggerCondition
    {
        [Header("Component Settings")]
        [SerializeField] private string componentTypeName = "";
        [SerializeField] private bool checkEnabled = false;
        [SerializeField] private bool useContext = true;
        [SerializeField] private GameObject targetObject;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            if (string.IsNullOrEmpty(componentTypeName))
            {
                LogWarning("Component type name is empty");
                return false;
            }
            
            GameObject target = useContext ? context : targetObject;
            if (target == null)
            {
                LogDebug("Target is null");
                return false;
            }
            
            var type = System.Type.GetType(componentTypeName);
            if (type == null)
            {
                LogWarning($"Component type '{componentTypeName}' not found");
                return false;
            }
            
            var component = target.GetComponent(type);
            if (component == null)
            {
                LogDebug($"Component {componentTypeName} not found on {target.name}");
                return false;
            }
            
            if (checkEnabled && component is MonoBehaviour mb)
            {
                bool enabled = mb.enabled;
                LogDebug($"Component {componentTypeName} on {target.name} enabled: {enabled}");
                return enabled;
            }
            
            LogDebug($"Component {componentTypeName} found on {target.name}");
            return true;
        }
        
        public void SetComponentType(string typeName)
        {
            componentTypeName = typeName;
        }
        
        public void SetCheckEnabled(bool check)
        {
            checkEnabled = check;
        }
        
        public void SetTargetObject(GameObject target)
        {
            targetObject = target;
            useContext = false;
        }
        
        public void SetUseContext(bool use)
        {
            useContext = use;
        }
    }
    
    /// <summary>
    /// Condition that checks if GameObjects are active.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Active State Condition")]
    public class ActiveStateCondition : BaseTriggerCondition
    {
        [Header("Active State Settings")]
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();
        [SerializeField] private bool requireAllActive = true;
        [SerializeField] private bool checkActiveInHierarchy = false;
        [SerializeField] private bool useContext = false;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            var targets = useContext && context != null ? 
                new List<GameObject> { context } : targetObjects;
            
            if (targets.Count == 0)
            {
                LogDebug("No targets to check");
                return false;
            }
            
            int activeCount = 0;
            
            foreach (var target in targets)
            {
                if (target == null) continue;
                
                bool isActive = checkActiveInHierarchy ? 
                    target.activeInHierarchy : target.activeSelf;
                
                if (isActive)
                {
                    activeCount++;
                }
            }
            
            bool result = requireAllActive ? 
                activeCount == targets.Count : activeCount > 0;
            
            LogDebug($"Active objects: {activeCount}/{targets.Count}, Result: {result}");
            
            return result;
        }
        
        public void AddTarget(GameObject target)
        {
            if (target != null && !targetObjects.Contains(target))
            {
                targetObjects.Add(target);
            }
        }
        
        public void RemoveTarget(GameObject target)
        {
            targetObjects.Remove(target);
        }
        
        public void SetRequireAllActive(bool require)
        {
            requireAllActive = require;
        }
        
        public void SetUseContext(bool use)
        {
            useContext = use;
        }
    }
    
    /// <summary>
    /// Condition that always evaluates to true or false.
    /// Useful for testing or as a placeholder.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Conditions/Constant Condition")]
    public class ConstantCondition : BaseTriggerCondition
    {
        [Header("Constant Settings")]
        [SerializeField] private bool constantValue = true;
        
        protected override bool EvaluateCondition(GameObject context)
        {
            LogDebug($"Constant condition: {constantValue}");
            return constantValue;
        }
        
        public void SetConstantValue(bool value)
        {
            constantValue = value;
        }
    }
}