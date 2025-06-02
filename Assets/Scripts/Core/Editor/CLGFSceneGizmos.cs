using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using GameFramework.Events.Triggers;
using GameFramework.Events.Actions;
using GameFramework.Events.Channels;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Provides 3D scene view gizmos for CLGF components.
    /// Handles connection lines, action previews, and component visualizations.
    /// </summary>
    public static class CLGFSceneGizmos
    {
        private static readonly Dictionary<Type, Action<MonoBehaviour>> GizmoDrawers = new Dictionary<Type, Action<MonoBehaviour>>();
        
        static CLGFSceneGizmos()
        {
            RegisterGizmoDrawers();
        }
        
        private static void RegisterGizmoDrawers()
        {
            // Register gizmo drawers for specific component types
            GizmoDrawers[typeof(GameFramework.Events.Actions.MoveAction)] = DrawMoveActionGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Actions.RotateAction)] = DrawRotateActionGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Actions.ScaleAction)] = DrawScaleActionGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Actions.AudioAction)] = DrawAudioActionGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Actions.LightAction)] = DrawLightActionGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Triggers.ProximityTrigger)] = DrawProximityTriggerGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Triggers.CollisionTrigger)] = DrawCollisionTriggerGizmo;
            GizmoDrawers[typeof(GameFramework.Events.Listeners.GameEventListener)] = DrawGameEventListenerGizmo;
        }
        
        #region Public API
        
        /// <summary>
        /// Draw gizmos for a CLGF component if visualization is enabled.
        /// </summary>
        /// <param name="component">The component to draw gizmos for</param>
        /// <param name="selected">Whether the component's GameObject is selected</param>
        public static void DrawComponentGizmos(MonoBehaviour component, bool selected = false)
        {
            if (!CLGFVisualizationSettings.ShowSceneGizmos || component == null)
                return;
            
            Type componentType = component.GetType();
            
            // Try exact type match first
            if (GizmoDrawers.TryGetValue(componentType, out var drawer))
            {
                drawer(component);
            }
            else
            {
                // Try base type matches
                foreach (var kvp in GizmoDrawers)
                {
                    if (kvp.Key.IsAssignableFrom(componentType))
                    {
                        kvp.Value(component);
                        break;
                    }
                }
            }
            
            // Draw connection lines if this is a trigger and it's selected
            if (selected && CLGFVisualizationSettings.ShowConnectionLines && component is BaseTrigger trigger)
            {
                DrawTriggerConnections(trigger);
            }
        }
        
        /// <summary>
        /// Draw connection lines from a trigger to its target objects.
        /// </summary>
        /// <param name="trigger">The trigger component</param>
        public static void DrawTriggerConnections(BaseTrigger trigger)
        {
            if (!CLGFVisualizationSettings.ShowConnectionLines || trigger == null)
                return;
            
            Vector3 triggerPos = trigger.transform.position;
            Color triggerColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Collision);
            
            // Find all actions on the same GameObject
            var actions = trigger.GetComponents<BaseTriggerAction>();
            var actionsList = new List<BaseTriggerAction>(actions);
            
            foreach (var action in actionsList)
            {
                if (action == null) continue;
                
                Vector3? targetPos = GetActionTargetPosition(action);
                if (targetPos.HasValue && Vector3.Distance(triggerPos, targetPos.Value) > 0.1f)
                {
                    DrawConnectionLine(triggerPos, targetPos.Value, GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl));
                    
                    // Draw a small indicator at the action position
                    Gizmos.color = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
                    Gizmos.DrawWireSphere(targetPos.Value, 0.1f * CLGFVisualizationSettings.GizmoSize);
                }
            }
            
            // Draw connections to actions on other GameObjects referenced in Unity Events
            DrawUnityEventConnections(trigger);
            
            // Draw connections to GameEvent listeners
            DrawGameEventConnections(trigger);
            
            // Draw trigger indicator with multi-action label
            Gizmos.color = triggerColor;
            float triggerSize = 0.3f * CLGFVisualizationSettings.GizmoSize;
            Gizmos.DrawWireSphere(triggerPos, triggerSize);
            
        }
        
        #endregion
        
        #region Specific Gizmo Drawers
        
        private static void DrawMoveActionGizmo(MonoBehaviour component)
        {
            var moveAction = component as GameFramework.Events.Actions.MoveAction;
            if (moveAction == null) return;
            
            Vector3 currentPos = moveAction.transform.position;
            Vector3 targetPos = GetMoveActionTargetPosition(moveAction);
            
            if (CLGFVisualizationSettings.ShowActionPreviews)
            {
                // Draw target position as a ghost
                Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
                gizmoColor.a = 0.8f;
                
                float size = 0.5f * CLGFVisualizationSettings.GizmoSize;
                
                // Draw larger, more visible target position
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(targetPos, Vector3.one * size);
                
                // Draw a solid inner cube for better visibility
                gizmoColor.a = 0.3f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawCube(targetPos, Vector3.one * size * 0.8f);
                
                // Draw arrow from current to target
                gizmoColor.a = 1f;
                DrawArrow(currentPos, targetPos, gizmoColor);
                
                // Always show label for move actions
                DrawGizmoLabel(targetPos + Vector3.up * size, "📐 MOVE TARGET", gizmoColor);
            }
        }
        
        private static void DrawRotateActionGizmo(MonoBehaviour component)
        {
            var rotateAction = component as GameFramework.Events.Actions.RotateAction;
            if (rotateAction == null) return;
            
            if (CLGFVisualizationSettings.ShowActionPreviews)
            {
                Vector3 position = rotateAction.transform.position;
                Vector3 targetRotation = GetRotateActionTargetRotation(rotateAction);
                
                Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
                gizmoColor.a = 0.7f;
                
                Gizmos.color = gizmoColor;
                
                // Draw rotation arc
                float radius = 0.5f * CLGFVisualizationSettings.GizmoSize;
                DrawRotationGizmo(position, targetRotation, radius, gizmoColor);
                
                if (CLGFVisualizationSettings.DebugMode)
                {
                    DrawGizmoLabel(position + Vector3.up * 0.5f, "🔄 ROTATE TARGET", gizmoColor);
                }
            }
        }
        
        private static void DrawScaleActionGizmo(MonoBehaviour component)
        {
            var scaleAction = component as GameFramework.Events.Actions.ScaleAction;
            if (scaleAction == null) return;
            
            if (CLGFVisualizationSettings.ShowActionPreviews)
            {
                Vector3 position = scaleAction.transform.position;
                Vector3 targetScale = GetScaleActionTargetScale(scaleAction);
                
                Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
                gizmoColor.a = 0.5f;
                
                Gizmos.color = gizmoColor;
                
                // Draw target scale as a wireframe
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(position, scaleAction.transform.rotation, targetScale);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = oldMatrix;
                
                if (CLGFVisualizationSettings.DebugMode)
                {
                    DrawGizmoLabel(position + Vector3.up * 0.5f, "📏 SCALE TARGET", gizmoColor);
                }
            }
        }
        
        private static void DrawProximityTriggerGizmo(MonoBehaviour component)
        {
            var proximityTrigger = component as GameFramework.Events.Triggers.ProximityTrigger;
            if (proximityTrigger == null) return;
            
            Vector3 position = proximityTrigger.transform.position;
            float range = GetProximityTriggerRange(proximityTrigger);
            
            Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Collision);
            gizmoColor.a = 0.3f;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(position, range);
            
            if (CLGFVisualizationSettings.DebugMode)
            {
                DrawGizmoLabel(position + Vector3.up * range, "📡 PROXIMITY", gizmoColor);
            }
        }
        
        private static void DrawCollisionTriggerGizmo(MonoBehaviour component)
        {
            var collisionTrigger = component as GameFramework.Events.Triggers.CollisionTrigger;
            if (collisionTrigger == null) return;
            
            Vector3 position = collisionTrigger.transform.position;
            
            Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Collision);
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(position, Vector3.one * 0.3f * CLGFVisualizationSettings.GizmoSize);
            
            if (CLGFVisualizationSettings.DebugMode)
            {
                DrawGizmoLabel(position + Vector3.up * 0.5f, "⚡ TRIGGER", gizmoColor);
            }
        }
        
        private static void DrawAudioActionGizmo(MonoBehaviour component)
        {
            var audioAction = component as GameFramework.Events.Actions.AudioAction;
            if (audioAction == null) return;
            
            Vector3 position = audioAction.transform.position;
            
            // Get audio source range if 3D audio is enabled
            float range = GetAudioActionRange(audioAction);
            bool is3D = GetAudioActionIs3D(audioAction);
            
            Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
            gizmoColor.a = 0.3f;
            
            if (is3D && range > 0f && CLGFVisualizationSettings.ShowActionPreviews)
            {
                // Draw audio range sphere
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(position, range);
                
                // Draw inner sphere for min distance
                float minDistance = GetAudioActionMinDistance(audioAction);
                if (minDistance > 0f)
                {
                    gizmoColor.a = 0.6f;
                    Gizmos.color = gizmoColor;
                    Gizmos.DrawWireSphere(position, minDistance);
                }
            }
            
            // Draw audio icon
            Gizmos.color = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
            Gizmos.DrawWireCube(position, Vector3.one * 0.2f * CLGFVisualizationSettings.GizmoSize);
            
            if (CLGFVisualizationSettings.DebugMode)
            {
                string label = is3D ? "🔊 AUDIO 3D" : "🔊 AUDIO 2D";
                DrawGizmoLabel(position + Vector3.up * 0.5f, label, GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl));
            }
        }
        
        private static void DrawLightActionGizmo(MonoBehaviour component)
        {
            var lightAction = component as GameFramework.Events.Actions.LightAction;
            if (lightAction == null) return;
            
            Vector3 position = lightAction.transform.position;
            Light targetLight = GetLightActionTargetLight(lightAction);
            
            Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.ObjectControl);
            
            if (targetLight != null && CLGFVisualizationSettings.ShowActionPreviews)
            {
                // Draw light range if it has one
                if (targetLight.type == LightType.Point || targetLight.type == LightType.Spot)
                {
                    gizmoColor.a = 0.2f;
                    Gizmos.color = gizmoColor;
                    
                    if (targetLight.type == LightType.Point)
                    {
                        Gizmos.DrawWireSphere(position, targetLight.range);
                    }
                    else if (targetLight.type == LightType.Spot)
                    {
                        DrawSpotLightGizmo(position, targetLight.transform.forward, targetLight.range, targetLight.spotAngle, gizmoColor);
                    }
                }
                
                // Draw directional arrow for directional lights
                if (targetLight.type == LightType.Directional)
                {
                    Vector3 direction = targetLight.transform.forward;
                    Vector3 endPos = position + direction * 2f * CLGFVisualizationSettings.GizmoSize;
                    DrawArrow(position, endPos, gizmoColor);
                }
            }
            
            // Draw light bulb icon
            gizmoColor.a = 1f;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(position, 0.1f * CLGFVisualizationSettings.GizmoSize);
            
            if (CLGFVisualizationSettings.DebugMode)
            {
                string lightType = targetLight != null ? targetLight.type.ToString() : "UNKNOWN";
                DrawGizmoLabel(position + Vector3.up * 0.5f, $"💡 LIGHT {lightType}", gizmoColor);
            }
        }
        
        private static void DrawGameEventListenerGizmo(MonoBehaviour component)
        {
            var listener = component as GameFramework.Events.Listeners.GameEventListener;
            if (listener == null || !CLGFVisualizationSettings.ShowListeners) return;
            
            Vector3 position = listener.transform.position;
            Color gizmoColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Event);
            
            // Draw listener sphere
            gizmoColor.a = 0.7f;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(position, 0.2f * CLGFVisualizationSettings.GizmoSize);
            
            // Draw solid inner sphere for better visibility
            gizmoColor.a = 0.3f;
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(position, 0.15f * CLGFVisualizationSettings.GizmoSize);
            
            // Always show listener label when visualization is enabled
            gizmoColor.a = 1f;
            string gameEventName = GetGameEventListenerEventName(listener);
            string label = string.IsNullOrEmpty(gameEventName) ? "🎧 LISTENER" : $"🎧 {gameEventName}";
            DrawGizmoLabel(position + Vector3.up * 0.4f, label, gizmoColor);
        }
        
        #endregion
        
        #region Helper Methods
        
        private static Vector3 GetMoveActionTargetPosition(GameFramework.Events.Actions.MoveAction moveAction)
        {
            // Use SerializedObject to access the fields more reliably
            try
            {
                SerializedObject serializedObject = new SerializedObject(moveAction);
                SerializedProperty useTargetTransformProp = serializedObject.FindProperty("useTargetTransform");
                SerializedProperty targetPositionProp = serializedObject.FindProperty("targetPosition");
                SerializedProperty targetTransformProp = serializedObject.FindProperty("targetTransform");
                
                if (useTargetTransformProp != null && useTargetTransformProp.boolValue)
                {
                    if (targetTransformProp != null && targetTransformProp.objectReferenceValue != null)
                    {
                        Transform targetTransform = targetTransformProp.objectReferenceValue as Transform;
                        return targetTransform.position;
                    }
                }
                
                if (targetPositionProp != null)
                {
                    return targetPositionProp.vector3Value;
                }
                
                // Fallback - offset from current position for demonstration
                return moveAction.transform.position + Vector3.forward * 2f;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not get MoveAction target position: {e.Message}");
                // Return a visible offset for demonstration
                return moveAction.transform.position + Vector3.forward * 2f;
            }
        }
        
        private static Vector3 GetRotateActionTargetRotation(GameFramework.Events.Actions.RotateAction rotateAction)
        {
            try
            {
                var targetRotationField = rotateAction.GetType().GetField("targetRotation", BindingFlags.NonPublic | BindingFlags.Instance);
                return (Vector3)(targetRotationField?.GetValue(rotateAction) ?? Vector3.zero);
            }
            catch
            {
                return Vector3.zero;
            }
        }
        
        private static Vector3 GetScaleActionTargetScale(GameFramework.Events.Actions.ScaleAction scaleAction)
        {
            try
            {
                var targetScaleField = scaleAction.GetType().GetField("targetScale", BindingFlags.NonPublic | BindingFlags.Instance);
                var uniformScaleField = scaleAction.GetType().GetField("uniformScale", BindingFlags.NonPublic | BindingFlags.Instance);
                var uniformScaleValueField = scaleAction.GetType().GetField("uniformScaleValue", BindingFlags.NonPublic | BindingFlags.Instance);
                
                bool uniformScale = (bool)(uniformScaleField?.GetValue(scaleAction) ?? false);
                
                if (uniformScale)
                {
                    float uniformValue = (float)(uniformScaleValueField?.GetValue(scaleAction) ?? 1f);
                    return Vector3.one * uniformValue;
                }
                else
                {
                    return (Vector3)(targetScaleField?.GetValue(scaleAction) ?? Vector3.one);
                }
            }
            catch
            {
                return Vector3.one;
            }
        }
        
        private static float GetProximityTriggerRange(GameFramework.Events.Triggers.ProximityTrigger proximityTrigger)
        {
            try
            {
                var rangeField = proximityTrigger.GetType().GetField("detectionRange", BindingFlags.NonPublic | BindingFlags.Instance);
                return (float)(rangeField?.GetValue(proximityTrigger) ?? 1f);
            }
            catch
            {
                return 1f;
            }
        }
        
        private static Vector3? GetActionTargetPosition(BaseTriggerAction action)
        {
            return action switch
            {
                GameFramework.Events.Actions.MoveAction moveAction => GetMoveActionTargetPosition(moveAction),
                _ => null
            };
        }
        
        private static void DrawConnectionLine(Vector3 start, Vector3 end, Color color)
        {
            // Make lines much more visible with bezier curves
            Vector3 direction = (end - start);
            Vector3 midPoint = start + direction * 0.5f;
            Vector3 controlPoint1 = start + Vector3.up * direction.magnitude * 0.3f;
            Vector3 controlPoint2 = end + Vector3.up * direction.magnitude * 0.3f;
            
            // Draw thick bezier line
            Handles.color = color;
            for (int i = 0; i < Mathf.RoundToInt(CLGFVisualizationSettings.LineThickness); i++)
            {
                Vector3 offset = Vector3.up * i * 0.02f;
                Handles.DrawBezier(start + offset, end + offset, controlPoint1 + offset, controlPoint2 + offset, color, null, 3f);
            }
            
            // Draw directional arrow at end
            Vector3 arrowDirection = (end - controlPoint2).normalized;
            DrawArrowHead(end, arrowDirection, color, 0.3f * CLGFVisualizationSettings.GizmoSize);
            
            // Draw connection points
            Gizmos.color = color;
            float pointSize = 0.15f * CLGFVisualizationSettings.GizmoSize;
            Gizmos.DrawSphere(start, pointSize);
            Gizmos.DrawSphere(end, pointSize);
        }
        
        private static void DrawArrow(Vector3 start, Vector3 end, Color color)
        {
            Handles.color = color;
            Handles.DrawLine(start, end, CLGFVisualizationSettings.LineThickness);
            
            Vector3 direction = (end - start).normalized;
            DrawArrowHead(end, direction, color, 0.2f * CLGFVisualizationSettings.GizmoSize);
        }
        
        private static void DrawArrowHead(Vector3 position, Vector3 direction, Color color, float size)
        {
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, direction).normalized;
            
            Vector3 arrowPoint1 = position - direction * size + right * size * 0.5f;
            Vector3 arrowPoint2 = position - direction * size - right * size * 0.5f;
            Vector3 arrowPoint3 = position - direction * size + up * size * 0.5f;
            Vector3 arrowPoint4 = position - direction * size - up * size * 0.5f;
            
            Handles.color = color;
            Handles.DrawLine(position, arrowPoint1);
            Handles.DrawLine(position, arrowPoint2);
            Handles.DrawLine(position, arrowPoint3);
            Handles.DrawLine(position, arrowPoint4);
        }
        
        private static void DrawRotationGizmo(Vector3 position, Vector3 targetRotation, float radius, Color color)
        {
            Handles.color = color;
            
            // Draw rotation arcs for each axis
            Vector3 eulerAngles = targetRotation;
            
            if (Mathf.Abs(eulerAngles.y) > 0.1f)
            {
                Handles.DrawWireArc(position, Vector3.up, Vector3.forward, eulerAngles.y, radius);
            }
            
            if (Mathf.Abs(eulerAngles.x) > 0.1f)
            {
                Handles.DrawWireArc(position, Vector3.right, Vector3.up, eulerAngles.x, radius);
            }
            
            if (Mathf.Abs(eulerAngles.z) > 0.1f)
            {
                Handles.DrawWireArc(position, Vector3.forward, Vector3.right, eulerAngles.z, radius);
            }
        }
        
        private static void DrawGizmoLabel(Vector3 position, string text, Color color)
        {
            if (Camera.current == null) return;
            
            // Add small deterministic offset to prevent overlapping labels
            float hash = (position.x + position.y + position.z + text.GetHashCode()) * 0.1f;
            Vector3 offset = new Vector3(
                Mathf.Sin(hash) * 0.05f,
                Mathf.Cos(hash) * 0.05f,
                Mathf.Sin(hash * 1.7f) * 0.02f
            );
            position += offset;
            
            // Calculate distance for both scaling and alpha fading
            float distance = Vector3.Distance(Camera.current.transform.position, position);
            float userScale = CLGFVisualizationSettings.GizmoSize;
            
            // Alpha fading using tunable parameters
            float maxFadeDistance = CLGFVisualizationSettings.LabelDistanceFadeEnd;
            float minFadeDistance = CLGFVisualizationSettings.LabelDistanceFadeStart;
            float alpha = Mathf.Clamp01(1f - ((distance - minFadeDistance) / (maxFadeDistance - minFadeDistance)));
            alpha = Mathf.Max(alpha, CLGFVisualizationSettings.LabelMinAlpha);
            
            // Apply user-controlled text alpha
            alpha *= CLGFVisualizationSettings.LabelTextAlpha;
            
            // Simple, reliable distance-based scaling
            DrawSimpleScreenSpaceLabel(position, text, color, distance, userScale, alpha);
        }
        
        private static void DrawSimpleScreenSpaceLabel(Vector3 position, string text, Color textColor, float distance, float userScale, float alpha)
        {
            // Check if position is in front of camera
            Vector3 screenPos = Camera.current.WorldToScreenPoint(position);
            if (screenPos.z <= 0) return;
            
            Handles.BeginGUI();
            
            // Convert world position to GUI coordinates
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(position);
            
            // Simple, predictable font size calculation with tunable max distance
            // Base size that looks good, scaled by user preference, font size setting, and distance
            float baseSize = 16f * userScale * CLGFVisualizationSettings.LabelFontSize;
            float maxDistance = CLGFVisualizationSettings.MaxLabelDistance;
            
            float distanceScale;
            if (distance > maxDistance)
            {
                // Beyond max distance: shrink to user-controlled scale
                distanceScale = CLGFVisualizationSettings.LabelDistanceScaleBeyondMax;
            }
            else
            {
                // Within max distance: scale from 50% to 100%
                distanceScale = Mathf.Lerp(0.5f, 1.0f, distance / maxDistance);
            }
            
            int fontSize = Mathf.RoundToInt(baseSize * distanceScale);
            fontSize = Mathf.Clamp(fontSize, CLGFVisualizationSettings.LabelMinFontSize, CLGFVisualizationSettings.LabelMaxFontSize);
            
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(textColor.r, textColor.g, textColor.b, alpha) }
            };
            
            // Calculate text size
            GUIContent content = new GUIContent(text);
            Vector2 textSize = style.CalcSize(content);
            
            // Simple background sizing with tunable padding
            float padding = 8f * userScale * CLGFVisualizationSettings.LabelPadding;
            Vector2 bgSize = new Vector2(textSize.x + padding * 2f, textSize.y + padding);
            
            // Center everything
            Rect bgRect = new Rect(guiPos.x - bgSize.x * 0.5f, guiPos.y - bgSize.y * 0.5f, bgSize.x, bgSize.y);
            Rect textRect = new Rect(guiPos.x - textSize.x * 0.5f, guiPos.y - textSize.y * 0.5f, textSize.x, textSize.y);
            
            // Draw simple rounded background with tunable alpha
            Color bgColor = new Color(0f, 0f, 0f, CLGFVisualizationSettings.LabelBackgroundAlpha * alpha);
            DrawSimpleRoundedRect(bgRect, bgColor, 4f);
            
            // Draw text
            GUI.Label(textRect, content, style);
            
            Handles.EndGUI();
        }
        
        private static void DrawSimpleRoundedRect(Rect rect, Color color, float cornerRadius)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            
            // For now, just draw a simple rectangle - clean and predictable
            // We can add proper rounded corners later if needed
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            
            GUI.color = oldColor;
        }
        
        private static void DrawMultiActionLabel(Vector3 position, BaseTrigger trigger, List<BaseTriggerAction> actions)
        {
            if (!CLGFVisualizationSettings.ShowActionPreviews && !CLGFVisualizationSettings.DebugMode)
                return;
                
            // Build label with receiver emoji + background + action emojis
            string receiverEmoji = "🎯"; // Default trigger emoji
            Color backgroundColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Collision);
            
            // Get specific receiver emoji based on trigger type
            if (trigger is GameFramework.Events.Triggers.ProximityTrigger) receiverEmoji = "📡";
            else if (trigger is GameFramework.Events.Triggers.CollisionTrigger) receiverEmoji = "💥";
            else if (trigger is GameFramework.Events.Triggers.TimerTrigger) receiverEmoji = "⏰";
            
            // Build action emojis string
            string actionEmojis = "";
            foreach (var action in actions)
            {
                actionEmojis += GetActionEmoji(action);
            }
            
            // Create composite label
            string fullLabel = $"{receiverEmoji} {actionEmojis}";
            
            // Draw with special multi-action styling
            DrawEnhancedGizmoLabel(position, receiverEmoji, actionEmojis, backgroundColor);
        }
        
        private static void DrawEnhancedGizmoLabel(Vector3 position, string receiverEmoji, string actionEmojis, Color backgroundColor)
        {
            if (Camera.current == null) return;
            
            // Calculate distance for alpha fading and scaling
            float distance = Vector3.Distance(Camera.current.transform.position, position);
            float userScale = CLGFVisualizationSettings.GizmoSize;
            
            // Alpha fading using tunable parameters
            float maxFadeDistance = CLGFVisualizationSettings.LabelDistanceFadeEnd;
            float minFadeDistance = CLGFVisualizationSettings.LabelDistanceFadeStart;
            float alpha = Mathf.Clamp01(1f - ((distance - minFadeDistance) / (maxFadeDistance - minFadeDistance)));
            alpha = Mathf.Max(alpha, CLGFVisualizationSettings.LabelMinAlpha);
            
            // Apply user-controlled text alpha
            alpha *= CLGFVisualizationSettings.LabelTextAlpha;
            
            // Draw enhanced multi-part label using simplified approach
            DrawSimpleMultiLabel(position, receiverEmoji, actionEmojis, backgroundColor, Color.white, distance, userScale, alpha);
        }
        
        private static void DrawSimpleMultiLabel(Vector3 position, string leftText, string rightText, Color leftBgColor, Color textColor, float distance, float userScale, float alpha)
        {
            // Check if position is in front of camera
            Vector3 screenPos = Camera.current.WorldToScreenPoint(position);
            if (screenPos.z <= 0) return;
            
            Handles.BeginGUI();
            
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(position);
            
            // Simple font size calculation with tunable max distance
            float baseSize = 18f * userScale * CLGFVisualizationSettings.LabelFontSize;
            float maxDistance = CLGFVisualizationSettings.MaxLabelDistance;
            
            float distanceScale;
            if (distance > maxDistance)
            {
                // Beyond max distance: shrink to user-controlled scale
                distanceScale = CLGFVisualizationSettings.LabelDistanceScaleBeyondMax;
            }
            else
            {
                // Within max distance: scale from 50% to 100%
                distanceScale = Mathf.Lerp(0.5f, 1.0f, distance / maxDistance);
            }
            
            int leftFontSize = Mathf.RoundToInt(baseSize * distanceScale);
            int rightFontSize = Mathf.RoundToInt(baseSize * 0.85f * distanceScale);
            leftFontSize = Mathf.Clamp(leftFontSize, CLGFVisualizationSettings.LabelMinFontSize, CLGFVisualizationSettings.LabelMaxFontSize);
            rightFontSize = Mathf.Clamp(rightFontSize, Mathf.Max(1, CLGFVisualizationSettings.LabelMinFontSize - 1), CLGFVisualizationSettings.LabelMaxFontSize);
            
            GUIStyle leftStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = leftFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(textColor.r, textColor.g, textColor.b, alpha) }
            };
            
            GUIStyle rightStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = rightFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(textColor.r, textColor.g, textColor.b, alpha) }
            };
            
            // Calculate sizes
            Vector2 leftSize = leftStyle.CalcSize(new GUIContent(leftText));
            Vector2 rightSize = string.IsNullOrEmpty(rightText) ? Vector2.zero : rightStyle.CalcSize(new GUIContent(rightText));
            
            // Simple layout with tunable padding
            float spacing = 6f * userScale;
            float padding = 8f * userScale * CLGFVisualizationSettings.LabelPadding;
            
            Vector2 leftBgSize = new Vector2(leftSize.x + padding * 2f, leftSize.y + padding);
            Vector2 rightBgSize = rightSize.magnitude > 0 ? new Vector2(rightSize.x + padding * 2f, rightSize.y + padding) : Vector2.zero;
            
            float totalWidth = leftBgSize.x + (rightBgSize.x > 0 ? spacing + rightBgSize.x : 0);
            
            // Position left element
            Vector2 leftPos = new Vector2(guiPos.x - totalWidth * 0.5f, guiPos.y - leftBgSize.y * 0.5f);
            Rect leftBgRect = new Rect(leftPos.x, leftPos.y, leftBgSize.x, leftBgSize.y);
            Rect leftTextRect = new Rect(leftPos.x, leftPos.y, leftBgSize.x, leftBgSize.y);
            
            // Draw left background and text with tunable background alpha
            Color leftBg = new Color(leftBgColor.r, leftBgColor.g, leftBgColor.b, CLGFVisualizationSettings.LabelBackgroundAlpha * alpha);
            DrawSimpleRoundedRect(leftBgRect, leftBg, 4f);
            GUI.Label(leftTextRect, leftText, leftStyle);
            
            // Draw right element if present
            if (rightBgSize.x > 0)
            {
                Vector2 rightPos = new Vector2(leftPos.x + leftBgSize.x + spacing, guiPos.y - rightBgSize.y * 0.5f);
                Rect rightBgRect = new Rect(rightPos.x, rightPos.y, rightBgSize.x, rightBgSize.y);
                Rect rightTextRect = new Rect(rightPos.x, rightPos.y, rightBgSize.x, rightBgSize.y);
                
                Color rightBg = new Color(0f, 0f, 0f, CLGFVisualizationSettings.LabelBackgroundAlpha * alpha);
                DrawSimpleRoundedRect(rightBgRect, rightBg, 4f);
                GUI.Label(rightTextRect, rightText, rightStyle);
            }
            
            Handles.EndGUI();
        }
        
        private static string GetActionEmoji(BaseTriggerAction action)
        {
            return action switch
            {
                GameFramework.Events.Actions.MoveAction => "📐",
                GameFramework.Events.Actions.RotateAction => "🔄", 
                GameFramework.Events.Actions.ScaleAction => "📏",
                GameFramework.Events.Actions.AudioAction => "🔊",
                GameFramework.Events.Actions.LightAction => "💡",
                GameFramework.Events.Actions.GameObjectActivateAction => "👁️",
                GameFramework.Events.Actions.InstantiateAction => "✨",
                GameFramework.Events.Actions.DestroyAction => "💥",
                GameFramework.Events.Actions.ComponentToggleAction => "🔧",
                GameFramework.Events.Actions.AnimationAction => "🎬",
                GameFramework.Events.Actions.ParticleAction => "✨",
                GameFramework.Events.Actions.PhysicsAction => "⚽",
                GameFramework.Events.Actions.RaiseGameEventAction => "📡",
                _ => "🚀" // Default action emoji
            };
        }
        
        private static void DrawUnityEventConnections(BaseTrigger trigger)
        {
            if (!CLGFVisualizationSettings.ShowConnectionLines) return;
            
            try
            {
                // Access the UnityEvent through SerializedObject
                SerializedObject serializedObject = new SerializedObject(trigger);
                SerializedProperty onTriggeredProp = serializedObject.FindProperty("onTriggered");
                
                if (onTriggeredProp != null)
                {
                    // Iterate through persistent calls in the UnityEvent
                    SerializedProperty persistentCallsProp = onTriggeredProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (persistentCallsProp != null)
                    {
                        for (int i = 0; i < persistentCallsProp.arraySize; i++)
                        {
                            SerializedProperty callProp = persistentCallsProp.GetArrayElementAtIndex(i);
                            SerializedProperty targetProp = callProp.FindPropertyRelative("m_Target");
                            
                            if (targetProp != null && targetProp.objectReferenceValue != null)
                            {
                                GameObject targetObject = null;
                                
                                if (targetProp.objectReferenceValue is Component comp)
                                {
                                    targetObject = comp.gameObject;
                                }
                                else if (targetProp.objectReferenceValue is GameObject go)
                                {
                                    targetObject = go;
                                }
                                
                                if (targetObject != null && targetObject != trigger.gameObject)
                                {
                                    Vector3 triggerPos = trigger.transform.position;
                                    Vector3 targetPos = targetObject.transform.position;
                                    
                                    Color connectionColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Event);
                                    connectionColor.a = 0.8f;
                                    
                                    DrawConnectionLine(triggerPos, targetPos, connectionColor);
                                    
                                    // Draw event indicator at target
                                    Gizmos.color = connectionColor;
                                    Gizmos.DrawWireCube(targetPos, Vector3.one * 0.2f * CLGFVisualizationSettings.GizmoSize);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                if (CLGFVisualizationSettings.DebugMode)
                {
                    Debug.LogWarning($"Could not analyze Unity Events for trigger {trigger.name}: {e.Message}");
                }
            }
        }
        
        private static void DrawGameEventConnections(BaseTrigger trigger)
        {
            if (!CLGFVisualizationSettings.ShowConnectionLines) return;
            
            try
            {
                // Look for RaiseGameEventAction components on the trigger
                var raiseEventActions = trigger.GetComponents<GameFramework.Events.Actions.RaiseGameEventAction>();
                
                foreach (var raiseAction in raiseEventActions)
                {
                    if (raiseAction == null) continue;
                    
                    // Access the GameEvents list through SerializedObject
                    SerializedObject actionSO = new SerializedObject(raiseAction);
                    SerializedProperty gameEventsProp = actionSO.FindProperty("gameEvents");
                    
                    if (gameEventsProp != null && gameEventsProp.isArray)
                    {
                        for (int i = 0; i < gameEventsProp.arraySize; i++)
                        {
                            SerializedProperty gameEventProp = gameEventsProp.GetArrayElementAtIndex(i);
                            if (gameEventProp.objectReferenceValue != null)
                            {
                                // Find all GameEventListeners in the scene that listen to this event
                                var listeners = UnityEngine.Object.FindObjectsOfType<GameFramework.Events.Listeners.GameEventListener>();
                                
                                foreach (var listener in listeners)
                                {
                                    if (listener == null) continue;
                                    
                                    // Check if this listener listens to our GameEvent
                                    SerializedObject listenerSO = new SerializedObject(listener);
                                    SerializedProperty gameEventRefProp = listenerSO.FindProperty("gameEvent");
                                    
                                    if (gameEventRefProp != null && 
                                        gameEventRefProp.objectReferenceValue == gameEventProp.objectReferenceValue)
                                    {
                                        // Draw connection from trigger to listener
                                        Vector3 triggerPos = trigger.transform.position;
                                        Vector3 listenerPos = listener.transform.position;
                                        
                                        if (Vector3.Distance(triggerPos, listenerPos) > 0.1f)
                                        {
                                            Color eventColor = GetThemeColor(CLGFBaseEditor.CLGFTheme.Event);
                                            eventColor.a = 0.9f;
                                            
                                            DrawConnectionLine(triggerPos, listenerPos, eventColor);
                                            
                                            // Draw listener indicator
                                            Gizmos.color = eventColor;
                                            Gizmos.DrawWireSphere(listenerPos, 0.15f * CLGFVisualizationSettings.GizmoSize);
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                if (CLGFVisualizationSettings.DebugMode)
                {
                    Debug.LogWarning($"Could not analyze GameEvent connections for trigger {trigger.name}: {e.Message}");
                }
            }
        }
        
        private static Color GetThemeColor(CLGFBaseEditor.CLGFTheme theme)
        {
            return theme switch
            {
                CLGFBaseEditor.CLGFTheme.Event => new Color(0.3f, 0.7f, 0.9f, 1f),
                CLGFBaseEditor.CLGFTheme.Action => new Color(0.3f, 0.9f, 0.4f, 1f),
                CLGFBaseEditor.CLGFTheme.Collision => new Color(0.9f, 0.7f, 0.3f, 1f),
                CLGFBaseEditor.CLGFTheme.ObjectControl => new Color(0.3f, 0.9f, 0.4f, 1f),
                CLGFBaseEditor.CLGFTheme.Character => new Color(0.8f, 0.4f, 0.9f, 1f),
                CLGFBaseEditor.CLGFTheme.Camera => new Color(0.4f, 0.9f, 0.8f, 1f),
                CLGFBaseEditor.CLGFTheme.UI => new Color(0.9f, 0.5f, 0.7f, 1f),
                CLGFBaseEditor.CLGFTheme.System => new Color(0.9f, 0.3f, 0.3f, 1f),
                _ => Color.white
            };
        }
        
        
        private static float GetAudioActionRange(GameFramework.Events.Actions.AudioAction audioAction)
        {
            try
            {
                var maxDistanceField = audioAction.GetType().GetField("maxDistance", BindingFlags.NonPublic | BindingFlags.Instance);
                return (float)(maxDistanceField?.GetValue(audioAction) ?? 10f);
            }
            catch
            {
                return 10f;
            }
        }
        
        private static float GetAudioActionMinDistance(GameFramework.Events.Actions.AudioAction audioAction)
        {
            try
            {
                var minDistanceField = audioAction.GetType().GetField("minDistance", BindingFlags.NonPublic | BindingFlags.Instance);
                return (float)(minDistanceField?.GetValue(audioAction) ?? 1f);
            }
            catch
            {
                return 1f;
            }
        }
        
        private static bool GetAudioActionIs3D(GameFramework.Events.Actions.AudioAction audioAction)
        {
            try
            {
                var use3DField = audioAction.GetType().GetField("use3D", BindingFlags.NonPublic | BindingFlags.Instance);
                return (bool)(use3DField?.GetValue(audioAction) ?? false);
            }
            catch
            {
                return false;
            }
        }
        
        private static Light GetLightActionTargetLight(GameFramework.Events.Actions.LightAction lightAction)
        {
            try
            {
                var targetLightField = lightAction.GetType().GetField("targetLight", BindingFlags.NonPublic | BindingFlags.Instance);
                return targetLightField?.GetValue(lightAction) as Light;
            }
            catch
            {
                return null;
            }
        }
        
        private static void DrawSpotLightGizmo(Vector3 position, Vector3 direction, float range, float spotAngle, Color color)
        {
            float radius = range * Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad);
            Vector3 endCenter = position + direction * range;
            
            // Draw the cone outline
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, direction).normalized;
            
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                Vector3 endPoint = endCenter + offset;
                
                Handles.color = color;
                Handles.DrawLine(position, endPoint);
            }
            
            // Draw end circle
            Handles.DrawWireDisc(endCenter, direction, radius);
        }
        
        private static string GetGameEventListenerEventName(GameFramework.Events.Listeners.GameEventListener listener)
        {
            try
            {
                SerializedObject serializedObject = new SerializedObject(listener);
                SerializedProperty gameEventProp = serializedObject.FindProperty("gameEvent");
                
                if (gameEventProp != null && gameEventProp.objectReferenceValue != null)
                {
                    return gameEventProp.objectReferenceValue.name;
                }
                
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not get GameEvent name for listener: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Unity DrawGizmo method for automatically drawing gizmos for GameEventListeners.
        /// </summary>
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawGameEventListenerGizmos(GameFramework.Events.Listeners.GameEventListener listener, GizmoType gizmoType)
        {
            if (!CLGFVisualizationSettings.ShowSceneGizmos || !CLGFVisualizationSettings.ShowListeners) return;
            
            bool isSelected = (gizmoType & GizmoType.InSelectionHierarchy) != 0;
            DrawComponentGizmos(listener, isSelected);
        }
        
        #endregion
    }
}