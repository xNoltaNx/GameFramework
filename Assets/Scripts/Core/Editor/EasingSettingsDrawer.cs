using UnityEngine;
using UnityEditor;

namespace GameFramework.Core.Editor
{
    [CustomPropertyDrawer(typeof(EasingSettings))]
    public class EasingSettingsDrawer : PropertyDrawer
    {
        private const float SPACING = 2f;
        private const float CURVE_HEIGHT = 64f;
        private const float PREVIEW_HEIGHT = 128f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var easingTypeProperty = property.FindPropertyRelative("easingType");
            var customCurveProperty = property.FindPropertyRelative("customCurve");
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);
            
            // Main label
            EditorGUI.LabelField(currentRect, label, EditorStyles.boldLabel);
            currentRect.y += lineHeight + SPACING;
            
            // Indent for the easing settings
            EditorGUI.indentLevel++;
            
            // Easing type dropdown
            EditorGUI.PropertyField(currentRect, easingTypeProperty, new GUIContent("Easing Type"));
            currentRect.y += lineHeight + SPACING;
            
            // Show custom curve field only when CustomCurve is selected
            EasingType easingType = (EasingType)easingTypeProperty.enumValueIndex;
            if (easingType == EasingType.CustomCurve)
            {
                EditorGUI.PropertyField(currentRect, customCurveProperty, new GUIContent("Custom Curve"));
                currentRect.y += CURVE_HEIGHT + SPACING;
            }
            
            // Preview section
            DrawPreview(currentRect, easingType, customCurveProperty);
            
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var easingTypeProperty = property.FindPropertyRelative("easingType");
            EasingType easingType = (EasingType)easingTypeProperty.enumValueIndex;
            
            float height = EditorGUIUtility.singleLineHeight * 2 + SPACING * 2; // Label + dropdown
            height += PREVIEW_HEIGHT + SPACING; // Preview
            
            if (easingType == EasingType.CustomCurve)
            {
                height += CURVE_HEIGHT + SPACING; // Custom curve field
            }
            
            return height;
        }
        
        private void DrawPreview(Rect position, EasingType easingType, SerializedProperty customCurveProperty)
        {
            Rect previewRect = new Rect(position.x, position.y, position.width, PREVIEW_HEIGHT);
            
            // Draw background
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));
            
            // Draw grid
            DrawGrid(previewRect);
            
            // Draw easing curve
            DrawEasingCurve(previewRect, easingType, customCurveProperty);
            
            // Draw border
            Handles.color = Color.gray;
            Vector3[] corners = {
                new Vector3(previewRect.xMin, previewRect.yMin, 0f),
                new Vector3(previewRect.xMax, previewRect.yMin, 0f),
                new Vector3(previewRect.xMax, previewRect.yMax, 0f),
                new Vector3(previewRect.xMin, previewRect.yMax, 0f),
                new Vector3(previewRect.xMin, previewRect.yMin, 0f)
            };
            Handles.DrawPolyLine(corners);
        }
        
        private void DrawGrid(Rect rect)
        {
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Vertical lines
            for (int i = 1; i < 4; i++)
            {
                float x = rect.x + (rect.width * i / 4f);
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.y + rect.height));
            }
            
            // Horizontal lines
            for (int i = 1; i < 4; i++)
            {
                float y = rect.y + (rect.height * i / 4f);
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.x + rect.width, y));
            }
        }
        
        private void DrawEasingCurve(Rect rect, EasingType easingType, SerializedProperty customCurveProperty)
        {
            const int segments = 64;
            Handles.color = Color.cyan;
            
            Vector3 previousPoint = new Vector3(rect.x, rect.y + rect.height, 0f);
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float easedValue;
                
                if (easingType == EasingType.CustomCurve && customCurveProperty.animationCurveValue != null)
                {
                    easedValue = customCurveProperty.animationCurveValue.Evaluate(t);
                }
                else
                {
                    easedValue = Easing.Evaluate(easingType, t);
                }
                
                // Clamp and invert Y (Unity GUI coordinates)
                easedValue = Mathf.Clamp01(easedValue);
                float x = rect.x + (t * rect.width);
                float y = rect.y + rect.height - (easedValue * rect.height);
                
                Vector3 currentPoint = new Vector3(x, y, 0f);
                Handles.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}