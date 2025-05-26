using UnityEngine;

namespace GameFramework.Core
{
    public enum EasingType
    {
        Linear,
        
        // Quadratic
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        
        // Cubic
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        
        // Quartic
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        
        // Quintic
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        
        // Sine
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        
        // Exponential
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        
        // Circular
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        
        // Back
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        
        // Elastic
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        
        // Bounce
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        
        // Custom curve
        CustomCurve
    }
    
    public static class Easing
    {
        private const float PI = Mathf.PI;
        private const float HALF_PI = PI / 2f;
        private const float TWO_PI = PI * 2f;
        
        // Back easing constants
        private const float c1 = 1.70158f;
        private const float c2 = c1 * 1.525f;
        private const float c3 = c1 + 1f;
        
        // Elastic easing constants
        private const float c4 = TWO_PI / 3f;
        private const float c5 = TWO_PI / 4.5f;
        
        // Bounce easing constants
        private const float n1 = 7.5625f;
        private const float d1 = 2.75f;
        
        public static float Evaluate(EasingType type, float t, AnimationCurve customCurve = null)
        {
            // Clamp t to [0, 1]
            t = Mathf.Clamp01(t);
            
            return type switch
            {
                EasingType.Linear => t,
                
                // Quadratic
                EasingType.EaseInQuad => t * t,
                EasingType.EaseOutQuad => 1f - (1f - t) * (1f - t),
                EasingType.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
                
                // Cubic
                EasingType.EaseInCubic => t * t * t,
                EasingType.EaseOutCubic => 1f - Mathf.Pow(1f - t, 3f),
                EasingType.EaseInOutCubic => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,
                
                // Quartic
                EasingType.EaseInQuart => t * t * t * t,
                EasingType.EaseOutQuart => 1f - Mathf.Pow(1f - t, 4f),
                EasingType.EaseInOutQuart => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f,
                
                // Quintic
                EasingType.EaseInQuint => t * t * t * t * t,
                EasingType.EaseOutQuint => 1f - Mathf.Pow(1f - t, 5f),
                EasingType.EaseInOutQuint => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f,
                
                // Sine
                EasingType.EaseInSine => 1f - Mathf.Cos(t * HALF_PI),
                EasingType.EaseOutSine => Mathf.Sin(t * HALF_PI),
                EasingType.EaseInOutSine => -(Mathf.Cos(PI * t) - 1f) / 2f,
                
                // Exponential
                EasingType.EaseInExpo => t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f)),
                EasingType.EaseOutExpo => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t),
                EasingType.EaseInOutExpo => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f,
                
                // Circular
                EasingType.EaseInCirc => 1f - Mathf.Sqrt(1f - Mathf.Pow(t, 2f)),
                EasingType.EaseOutCirc => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f)),
                EasingType.EaseInOutCirc => t < 0.5f ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f,
                
                // Back
                EasingType.EaseInBack => c3 * t * t * t - c1 * t * t,
                EasingType.EaseOutBack => 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f),
                EasingType.EaseInOutBack => t < 0.5f ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f,
                
                // Elastic
                EasingType.EaseInElastic => t == 0f ? 0f : t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4),
                EasingType.EaseOutElastic => t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f,
                EasingType.EaseInOutElastic => t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f,
                
                // Bounce
                EasingType.EaseInBounce => 1f - EaseOutBounce(1f - t),
                EasingType.EaseOutBounce => EaseOutBounce(t),
                EasingType.EaseInOutBounce => t < 0.5f ? (1f - EaseOutBounce(1f - 2f * t)) / 2f : (1f + EaseOutBounce(2f * t - 1f)) / 2f,
                
                // Custom curve
                EasingType.CustomCurve => customCurve?.Evaluate(t) ?? t,
                
                _ => t
            };
        }
        
        private static float EaseOutBounce(float t)
        {
            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
        
        // Utility methods for common use cases
        public static float Lerp(float a, float b, float t, EasingType easingType, AnimationCurve customCurve = null)
        {
            float easedT = Evaluate(easingType, t, customCurve);
            return Mathf.Lerp(a, b, easedT);
        }
        
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t, EasingType easingType, AnimationCurve customCurve = null)
        {
            float easedT = Evaluate(easingType, t, customCurve);
            return Vector3.Lerp(a, b, easedT);
        }
        
        public static Color Lerp(Color a, Color b, float t, EasingType easingType, AnimationCurve customCurve = null)
        {
            float easedT = Evaluate(easingType, t, customCurve);
            return Color.Lerp(a, b, easedT);
        }
    }
}