using UnityEngine;
using System.Collections.Generic;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that enables or disables a GameObject.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/GameObject Activate Action")]
    [ActionDefinition("gameobject-activate", "👁️", "GameObject Activate Action", "Enables or disables GameObjects and their children", "GameObject", 20)]
    public class GameObjectActivateAction : BaseTriggerAction
    {
        [Header("Activation Settings")]
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();
        [SerializeField] private bool activate = true;
        [SerializeField] private bool affectChildren = false;
        [SerializeField] private bool useContext = false;
        
        protected override void PerformAction(GameObject context)
        {
            var targets = GetTargetObjects(context);
            
            foreach (var target in targets)
            {
                if (target != null)
                {
                    LogDebug($"{(activate ? "Activating" : "Deactivating")} {target.name}");
                    target.SetActive(activate);
                    
                    if (affectChildren)
                    {
                        SetChildrenActive(target, activate);
                    }
                }
            }
        }
        
        private List<GameObject> GetTargetObjects(GameObject context)
        {
            if (useContext && context != null)
            {
                return new List<GameObject> { context };
            }
            
            return targetObjects;
        }
        
        private void SetChildrenActive(GameObject parent, bool active)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;
                child.SetActive(active);
            }
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
        
        public void SetActivate(bool value)
        {
            activate = value;
        }
    }
    
    /// <summary>
    /// Action that instantiates a prefab.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Instantiate Action")]
    [ActionDefinition("instantiate", "✨", "Instantiate Action", "Creates new instances of prefabs at specified locations", "GameObject", 30)]
    public class InstantiateAction : BaseTriggerAction
    {
        [Header("Instantiate Settings")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool useRandomPosition = false;
        [SerializeField] private Vector3 positionOffset = Vector3.zero;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;
        [SerializeField] private bool inheritRotation = true;
        [SerializeField] private bool setAsChild = false;
        
        [Header("Random Position")]
        [SerializeField] private Vector3 randomRange = Vector3.one;
        
        private List<GameObject> instantiatedObjects = new List<GameObject>();
        
        protected override void PerformAction(GameObject context)
        {
            if (prefab == null)
            {
                LogWarning("Cannot instantiate - prefab is null");
                return;
            }
            
            Vector3 position = GetSpawnPosition();
            Quaternion rotation = GetSpawnRotation();
            Transform parent = setAsChild ? (spawnPoint ?? transform) : null;
            
            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instantiatedObjects.Add(instance);
            
            LogDebug($"Instantiated {prefab.name} at {position}");
        }
        
        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition = spawnPoint ? spawnPoint.position : transform.position;
            basePosition += positionOffset;
            
            if (useRandomPosition)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-randomRange.x, randomRange.x),
                    Random.Range(-randomRange.y, randomRange.y),
                    Random.Range(-randomRange.z, randomRange.z)
                );
                basePosition += randomOffset;
            }
            
            return basePosition;
        }
        
        private Quaternion GetSpawnRotation()
        {
            Quaternion baseRotation = inheritRotation && spawnPoint ? spawnPoint.rotation : Quaternion.identity;
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            return baseRotation * offsetRotation;
        }
        
        public void SetPrefab(GameObject newPrefab)
        {
            prefab = newPrefab;
        }
        
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
        }
        
        public GameObject[] GetInstantiatedObjects()
        {
            // Remove null references
            instantiatedObjects.RemoveAll(obj => obj == null);
            return instantiatedObjects.ToArray();
        }
        
        public void ClearInstantiatedObjects()
        {
            instantiatedObjects.Clear();
        }
    }
    
    /// <summary>
    /// Action that destroys GameObjects.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Destroy Action")]
    [ActionDefinition("destroy", "💥", "Destroy Action", "Destroys GameObjects with optional delay and effects", "GameObject", 40)]
    public class DestroyAction : BaseTriggerAction
    {
        [Header("Destroy Settings")]
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();
        [SerializeField] private bool useContext = false;
        [SerializeField] private bool destroyChildren = false;
        [SerializeField] private float destroyDelay = 0f;
        [SerializeField] private bool showDestroyEffect = false;
        [SerializeField] private GameObject destroyEffectPrefab;
        
        protected override void PerformAction(GameObject context)
        {
            var targets = GetTargetObjects(context);
            
            foreach (var target in targets)
            {
                if (target != null)
                {
                    LogDebug($"Destroying {target.name}");
                    
                    if (showDestroyEffect && destroyEffectPrefab != null)
                    {
                        Instantiate(destroyEffectPrefab, target.transform.position, target.transform.rotation);
                    }
                    
                    if (destroyChildren)
                    {
                        DestroyChildren(target);
                    }
                    
                    if (destroyDelay > 0f)
                    {
                        Destroy(target, destroyDelay);
                    }
                    else
                    {
                        Destroy(target);
                    }
                }
            }
        }
        
        private List<GameObject> GetTargetObjects(GameObject context)
        {
            if (useContext && context != null)
            {
                return new List<GameObject> { context };
            }
            
            return targetObjects;
        }
        
        private void DestroyChildren(GameObject parent)
        {
            var children = new List<GameObject>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                children.Add(parent.transform.GetChild(i).gameObject);
            }
            
            foreach (var child in children)
            {
                if (destroyDelay > 0f)
                {
                    Destroy(child, destroyDelay);
                }
                else
                {
                    Destroy(child);
                }
            }
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
        
        public void SetDestroyDelay(float delay)
        {
            destroyDelay = Mathf.Max(0f, delay);
        }
    }
    
    /// <summary>
    /// Action that enables or disables components.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Component Toggle Action")]
    [ActionDefinition("component-toggle", "🔧", "Component Toggle Action", "Enables or disables specific components on GameObjects", "GameObject", 50)]
    public class ComponentToggleAction : BaseTriggerAction
    {
        [Header("Component Settings")]
        [SerializeField] private List<MonoBehaviour> targetComponents = new List<MonoBehaviour>();
        [SerializeField] private bool enable = true;
        [SerializeField] private string componentTypeName = "";
        [SerializeField] private bool useTypeName = false;
        [SerializeField] private bool affectSelf = false;
        
        protected override void PerformAction(GameObject context)
        {
            if (useTypeName && !string.IsNullOrEmpty(componentTypeName))
            {
                ToggleComponentsByType(context);
            }
            else
            {
                ToggleSpecificComponents();
            }
            
            if (affectSelf)
            {
                ToggleComponentsOnGameObject(gameObject);
            }
        }
        
        private void ToggleSpecificComponents()
        {
            foreach (var component in targetComponents)
            {
                if (component != null)
                {
                    LogDebug($"{(enable ? "Enabling" : "Disabling")} component {component.GetType().Name} on {component.name}");
                    component.enabled = enable;
                }
            }
        }
        
        private void ToggleComponentsByType(GameObject context)
        {
            var type = System.Type.GetType(componentTypeName);
            if (type == null)
            {
                LogWarning($"Component type '{componentTypeName}' not found");
                return;
            }
            
            GameObject target = context ?? gameObject;
            var components = target.GetComponents(type);
            
            foreach (var component in components)
            {
                if (component is MonoBehaviour mb)
                {
                    LogDebug($"{(enable ? "Enabling" : "Disabling")} component {type.Name} on {target.name}");
                    mb.enabled = enable;
                }
            }
        }
        
        private void ToggleComponentsOnGameObject(GameObject target)
        {
            if (string.IsNullOrEmpty(componentTypeName))
            {
                return;
            }
            
            var type = System.Type.GetType(componentTypeName);
            if (type == null)
            {
                return;
            }
            
            var components = target.GetComponents(type);
            foreach (var component in components)
            {
                if (component is MonoBehaviour mb)
                {
                    mb.enabled = enable;
                }
            }
        }
        
        public void AddComponent(MonoBehaviour component)
        {
            if (component != null && !targetComponents.Contains(component))
            {
                targetComponents.Add(component);
            }
        }
        
        public void RemoveComponent(MonoBehaviour component)
        {
            targetComponents.Remove(component);
        }
        
        public void SetEnable(bool value)
        {
            enable = value;
        }
        
        public void SetComponentType(string typeName)
        {
            componentTypeName = typeName;
            useTypeName = !string.IsNullOrEmpty(typeName);
        }
    }
}