using UnityEngine;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.Events.Performance
{
    /// <summary>
    /// Performance monitor for tracking trigger system performance.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Performance Monitor")]
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitor Settings")]
        [SerializeField] private int sampleCount = 60;
        [SerializeField] private bool logPerformanceWarnings = true;
        [SerializeField] private float frameTimeWarningThreshold = 16.67f; // 60 FPS
        
        [Header("Performance Data")]
        [SerializeField, ReadOnly] private float currentFrameTime;
        [SerializeField, ReadOnly] private float averageFrameTime;
        [SerializeField, ReadOnly] private float peakFrameTime;
        [SerializeField, ReadOnly] private int totalSamples;
        
        private Queue<float> frameTimes = new Queue<float>();
        private float frameStartTime;
        private float totalFrameTime;
        
        /// <summary>
        /// Current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTime => currentFrameTime;
        
        /// <summary>
        /// Average frame time in milliseconds.
        /// </summary>
        public float AverageFrameTime => averageFrameTime;
        
        /// <summary>
        /// Peak frame time in milliseconds.
        /// </summary>
        public float PeakFrameTime => peakFrameTime;
        
        /// <summary>
        /// Total number of samples collected.
        /// </summary>
        public int TotalSamples => totalSamples;
        
        /// <summary>
        /// Begin frame timing.
        /// </summary>
        public void BeginFrame()
        {
            frameStartTime = Time.realtimeSinceStartup * 1000f; // Convert to milliseconds
        }
        
        /// <summary>
        /// End frame timing and record the measurement.
        /// </summary>
        public void EndFrame()
        {
            float frameEndTime = Time.realtimeSinceStartup * 1000f;
            currentFrameTime = frameEndTime - frameStartTime;
            
            RecordFrameTime(currentFrameTime);
        }
        
        private void RecordFrameTime(float frameTime)
        {
            // Add to queue
            frameTimes.Enqueue(frameTime);
            totalFrameTime += frameTime;
            totalSamples++;
            
            // Update peak
            if (frameTime > peakFrameTime)
            {
                peakFrameTime = frameTime;
            }
            
            // Remove old samples if we exceed the limit
            if (frameTimes.Count > sampleCount)
            {
                float removedTime = frameTimes.Dequeue();
                totalFrameTime -= removedTime;
            }
            
            // Calculate average
            averageFrameTime = totalFrameTime / frameTimes.Count;
            
            // Check for performance warnings
            if (logPerformanceWarnings && frameTime > frameTimeWarningThreshold)
            {
                Debug.LogWarning($"[PerformanceMonitor] High frame time detected: {frameTime:F2}ms (threshold: {frameTimeWarningThreshold:F2}ms)");
            }
        }
        
        /// <summary>
        /// Reset all performance data.
        /// </summary>
        public void Reset()
        {
            frameTimes.Clear();
            totalFrameTime = 0f;
            totalSamples = 0;
            currentFrameTime = 0f;
            averageFrameTime = 0f;
            peakFrameTime = 0f;
            
            Debug.Log("[PerformanceMonitor] Performance data reset");
        }
        
        /// <summary>
        /// Get a summary of performance data.
        /// </summary>
        /// <returns>Performance summary string</returns>
        public string GetPerformanceSummary()
        {
            return $"Performance Summary:\n" +
                   $"Current Frame: {currentFrameTime:F2}ms\n" +
                   $"Average Frame: {averageFrameTime:F2}ms\n" +
                   $"Peak Frame: {peakFrameTime:F2}ms\n" +
                   $"Total Samples: {totalSamples}\n" +
                   $"Sample Window: {frameTimes.Count}/{sampleCount}";
        }
        
        /// <summary>
        /// Check if performance is currently good.
        /// </summary>
        /// <returns>True if performance is within acceptable limits</returns>
        public bool IsPerformanceGood()
        {
            return averageFrameTime <= frameTimeWarningThreshold;
        }
        
        /// <summary>
        /// Get the current FPS based on average frame time.
        /// </summary>
        /// <returns>Frames per second</returns>
        public float GetFPS()
        {
            if (averageFrameTime <= 0f) return 0f;
            return 1000f / averageFrameTime;
        }
        
        /// <summary>
        /// Set the performance warning threshold.
        /// </summary>
        /// <param name="threshold">Threshold in milliseconds</param>
        public void SetWarningThreshold(float threshold)
        {
            frameTimeWarningThreshold = Mathf.Max(0.1f, threshold);
        }
        
        /// <summary>
        /// Set the sample count for averaging.
        /// </summary>
        /// <param name="count">Number of samples to keep</param>
        public void SetSampleCount(int count)
        {
            sampleCount = Mathf.Max(1, count);
            
            // Trim existing samples if necessary
            while (frameTimes.Count > sampleCount)
            {
                float removedTime = frameTimes.Dequeue();
                totalFrameTime -= removedTime;
            }
            
            // Recalculate average
            if (frameTimes.Count > 0)
            {
                averageFrameTime = totalFrameTime / frameTimes.Count;
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Reset Performance Data")]
        private void ResetPerformanceData()
        {
            Reset();
        }
        
        [ContextMenu("Log Performance Summary")]
        private void LogPerformanceSummary()
        {
            Debug.Log(GetPerformanceSummary());
        }
        #endif
    }
}