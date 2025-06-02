using UnityEngine;
using UnityEditor;
using GameFramework.Events.Triggers;
using GameFramework.Events.Actions;

namespace GameFramework.Core.Editor
{
    /// <summary>
    /// Integrates CLGF gizmo drawing with Unity's gizmo system.
    /// Automatically draws gizmos for CLGF components when they are selected or when visualization is enabled.
    /// </summary>
    public static class CLGFGizmoIntegration
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
        static void DrawBaseTriggerGizmos(BaseTrigger trigger, GizmoType gizmoType)
        {
            if (trigger == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(trigger, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
        static void DrawBaseTriggerActionGizmos(BaseTriggerAction action, GizmoType gizmoType)
        {
            if (action == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(action, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawMoveActionGizmos(GameFramework.Events.Actions.MoveAction moveAction, GizmoType gizmoType)
        {
            if (moveAction == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(moveAction, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawRotateActionGizmos(GameFramework.Events.Actions.RotateAction rotateAction, GizmoType gizmoType)
        {
            if (rotateAction == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(rotateAction, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawScaleActionGizmos(GameFramework.Events.Actions.ScaleAction scaleAction, GizmoType gizmoType)
        {
            if (scaleAction == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(scaleAction, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawProximityTriggerGizmos(GameFramework.Events.Triggers.ProximityTrigger proximityTrigger, GizmoType gizmoType)
        {
            if (proximityTrigger == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(proximityTrigger, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawCollisionTriggerGizmos(GameFramework.Events.Triggers.CollisionTrigger collisionTrigger, GizmoType gizmoType)
        {
            if (collisionTrigger == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(collisionTrigger, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawAudioActionGizmos(GameFramework.Events.Actions.AudioAction audioAction, GizmoType gizmoType)
        {
            if (audioAction == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(audioAction, isSelected);
            }
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawLightActionGizmos(GameFramework.Events.Actions.LightAction lightAction, GizmoType gizmoType)
        {
            if (lightAction == null) return;
            
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            bool shouldDraw = isSelected || CLGFVisualizationSettings.AlwaysShow;
            
            if (shouldDraw)
            {
                CLGFSceneGizmos.DrawComponentGizmos(lightAction, isSelected);
            }
        }
    }
}