using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Events.Performance
{
    /// <summary>
    /// Centralized manager for optimizing trigger performance.
    /// Handles spatial partitioning, batching, and performance monitoring.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Trigger Manager")]
    public class TriggerManager : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableSpatialPartitioning = true;
        [SerializeField] private float cellSize = 50f;
        [SerializeField] private int maxTriggersPerFrame = 50;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        
        [Header("Update Settings")]
        [SerializeField] private float proximityUpdateInterval = 0.1f;
        [SerializeField] private bool useUnscaledTime = false;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showSpatialGrid = false;
        
        // Singleton instance
        public static TriggerManager Instance { get; private set; }
        
        // Spatial partitioning
        private Dictionary<Vector2Int, List<ITrigger>> spatialGrid = new Dictionary<Vector2Int, List<ITrigger>>();
        private HashSet<ITrigger> allTriggers = new HashSet<ITrigger>();
        
        // Performance monitoring
        private PerformanceMonitor performanceMonitor;
        
        // Update management
        private float lastProximityUpdate;
        private Queue<ITrigger> triggerUpdateQueue = new Queue<ITrigger>();
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            if (enablePerformanceMonitoring)
            {
                performanceMonitor = gameObject.AddComponent<PerformanceMonitor>();
            }
            
            LogDebug("TriggerManager initialized");
        }
        
        private void Update()
        {
            if (enablePerformanceMonitoring && performanceMonitor != null)
            {
                performanceMonitor.BeginFrame();
            }
            
            UpdateProximityTriggers();
            ProcessTriggerQueue();
            
            if (enablePerformanceMonitoring && performanceMonitor != null)
            {
                performanceMonitor.EndFrame();
            }
        }
        
        #region Trigger Registration
        
        /// <summary>
        /// Register a trigger with the manager for optimization.
        /// </summary>
        /// <param name="trigger">The trigger to register</param>
        public void RegisterTrigger(ITrigger trigger)
        {
            if (trigger == null || allTriggers.Contains(trigger))
            {
                return;
            }
            
            allTriggers.Add(trigger);
            
            if (enableSpatialPartitioning && trigger.TriggerSource != null)
            {
                AddToSpatialGrid(trigger);
            }
            
            LogDebug($"Registered trigger: {trigger.TriggerSource?.name}");
        }
        
        /// <summary>
        /// Unregister a trigger from the manager.
        /// </summary>
        /// <param name="trigger">The trigger to unregister</param>
        public void UnregisterTrigger(ITrigger trigger)
        {
            if (trigger == null || !allTriggers.Contains(trigger))
            {
                return;
            }
            
            allTriggers.Remove(trigger);
            
            if (enableSpatialPartitioning)
            {
                RemoveFromSpatialGrid(trigger);
            }
            
            LogDebug($"Unregistered trigger: {trigger.TriggerSource?.name}");
        }
        
        /// <summary>
        /// Update a trigger's position in the spatial grid.
        /// </summary>
        /// <param name="trigger">The trigger to update</param>
        public void UpdateTriggerPosition(ITrigger trigger)
        {
            if (!enableSpatialPartitioning || trigger?.TriggerSource == null)
            {
                return;
            }
            
            RemoveFromSpatialGrid(trigger);
            AddToSpatialGrid(trigger);
        }
        
        #endregion
        
        #region Spatial Partitioning
        
        private void AddToSpatialGrid(ITrigger trigger)
        {
            if (trigger?.TriggerSource == null) return;
            
            Vector2Int cell = GetGridCell(trigger.TriggerSource.transform.position);
            
            if (!spatialGrid.ContainsKey(cell))
            {
                spatialGrid[cell] = new List<ITrigger>();
            }
            
            spatialGrid[cell].Add(trigger);
        }
        
        private void RemoveFromSpatialGrid(ITrigger trigger)
        {
            if (trigger?.TriggerSource == null) return;
            
            Vector2Int cell = GetGridCell(trigger.TriggerSource.transform.position);
            
            if (spatialGrid.ContainsKey(cell))
            {
                spatialGrid[cell].Remove(trigger);
                
                if (spatialGrid[cell].Count == 0)
                {
                    spatialGrid.Remove(cell);
                }
            }
        }
        
        private Vector2Int GetGridCell(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / cellSize),
                Mathf.FloorToInt(worldPosition.z / cellSize)
            );
        }
        
        /// <summary>
        /// Get triggers in a specific spatial region.
        /// </summary>
        /// <param name="center">Center of the region</param>
        /// <param name="radius">Radius of the region</param>
        /// <returns>List of triggers in the region</returns>
        public List<ITrigger> GetTriggersInRegion(Vector3 center, float radius)
        {
            var result = new List<ITrigger>();
            
            if (!enableSpatialPartitioning)
            {
                result.AddRange(allTriggers);
                return result;
            }
            
            int cellRadius = Mathf.CeilToInt(radius / cellSize);
            Vector2Int centerCell = GetGridCell(center);
            
            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int z = -cellRadius; z <= cellRadius; z++)
                {
                    Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + z);
                    
                    if (spatialGrid.ContainsKey(cell))
                    {
                        result.AddRange(spatialGrid[cell]);
                    }
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region Update Management
        
        private void UpdateProximityTriggers()
        {
            float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            
            if (currentTime - lastProximityUpdate < proximityUpdateInterval)
            {
                return;
            }
            
            lastProximityUpdate = currentTime;
            
            // Queue proximity triggers for update
            foreach (var trigger in allTriggers)
            {
                if (trigger != null && trigger.IsActive)
                {
                    triggerUpdateQueue.Enqueue(trigger);
                }
            }
        }
        
        private void ProcessTriggerQueue()
        {
            int processedCount = 0;
            
            while (triggerUpdateQueue.Count > 0 && processedCount < maxTriggersPerFrame)
            {
                var trigger = triggerUpdateQueue.Dequeue();
                
                if (trigger != null && trigger.IsActive)
                {
                    // Here you could call specific update methods on proximity triggers
                    // This is a hook for custom trigger update logic
                }
                
                processedCount++;
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        /// <summary>
        /// Get performance statistics for the trigger system.
        /// </summary>
        /// <returns>Performance statistics</returns>
        public TriggerPerformanceStats GetPerformanceStats()
        {
            return new TriggerPerformanceStats
            {
                totalTriggers = allTriggers.Count,
                activeTriggers = GetActiveTriggerCount(),
                spatialCells = spatialGrid.Count,
                averageFrameTime = performanceMonitor?.AverageFrameTime ?? 0f,
                peakFrameTime = performanceMonitor?.PeakFrameTime ?? 0f
            };
        }
        
        private int GetActiveTriggerCount()
        {
            int count = 0;
            foreach (var trigger in allTriggers)
            {
                if (trigger != null && trigger.IsActive)
                {
                    count++;
                }
            }
            return count;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Enable or disable spatial partitioning.
        /// </summary>
        /// <param name="enabled">Whether spatial partitioning should be enabled</param>
        public void SetSpatialPartitioning(bool enabled)
        {
            if (enableSpatialPartitioning == enabled) return;
            
            enableSpatialPartitioning = enabled;
            
            if (!enabled)
            {
                spatialGrid.Clear();
            }
            else
            {
                // Rebuild spatial grid
                spatialGrid.Clear();
                foreach (var trigger in allTriggers)
                {
                    if (trigger?.TriggerSource != null)
                    {
                        AddToSpatialGrid(trigger);
                    }
                }
            }
            
            LogDebug($"Spatial partitioning {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set the maximum number of triggers to process per frame.
        /// </summary>
        /// <param name="max">Maximum triggers per frame</param>
        public void SetMaxTriggersPerFrame(int max)
        {
            maxTriggersPerFrame = Mathf.Max(1, max);
            LogDebug($"Max triggers per frame set to: {maxTriggersPerFrame}");
        }
        
        /// <summary>
        /// Set the proximity update interval.
        /// </summary>
        /// <param name="interval">Update interval in seconds</param>
        public void SetProximityUpdateInterval(float interval)
        {
            proximityUpdateInterval = Mathf.Max(0.01f, interval);
            LogDebug($"Proximity update interval set to: {proximityUpdateInterval:F3}s");
        }
        
        #endregion
        
        #region Logging
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[TriggerManager] {message}");
            }
        }
        
        #endregion
        
        #region Gizmos
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showSpatialGrid || !enableSpatialPartitioning) return;
            
            Gizmos.color = Color.cyan;
            
            foreach (var kvp in spatialGrid)
            {
                Vector2Int cell = kvp.Key;
                Vector3 cellCenter = new Vector3(
                    cell.x * cellSize + cellSize * 0.5f,
                    0f,
                    cell.y * cellSize + cellSize * 0.5f
                );
                
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 1f, cellSize));
                
                // Draw trigger count
                UnityEditor.Handles.Label(cellCenter, kvp.Value.Count.ToString());
            }
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Performance statistics for the trigger system.
    /// </summary>
    [System.Serializable]
    public struct TriggerPerformanceStats
    {
        public int totalTriggers;
        public int activeTriggers;
        public int spatialCells;
        public float averageFrameTime;
        public float peakFrameTime;
    }
}