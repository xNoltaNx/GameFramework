using UnityEngine;

namespace GameFramework.Core
{
    public static class EasingExtensions
    {
        // Float extensions
        public static float EaseTo(this float from, float to, float t, EasingSettings easing)
        {
            return easing.Lerp(from, to, t);
        }
        
        public static float EaseTo(this float from, float to, float t, EasingType easingType)
        {
            return Easing.Lerp(from, to, t, easingType);
        }
        
        // Vector3 extensions
        public static Vector3 EaseTo(this Vector3 from, Vector3 to, float t, EasingSettings easing)
        {
            return easing.Lerp(from, to, t);
        }
        
        public static Vector3 EaseTo(this Vector3 from, Vector3 to, float t, EasingType easingType)
        {
            return Easing.Lerp(from, to, t, easingType);
        }
        
        // Color extensions
        public static Color EaseTo(this Color from, Color to, float t, EasingSettings easing)
        {
            return easing.Lerp(from, to, t);
        }
        
        public static Color EaseTo(this Color from, Color to, float t, EasingType easingType)
        {
            return Easing.Lerp(from, to, t, easingType);
        }
        
        // Transform extensions for common animation tasks
        public static void EasePosition(this Transform transform, Vector3 from, Vector3 to, float t, EasingSettings easing)
        {
            transform.position = easing.Lerp(from, to, t);
        }
        
        public static void EaseLocalPosition(this Transform transform, Vector3 from, Vector3 to, float t, EasingSettings easing)
        {
            transform.localPosition = easing.Lerp(from, to, t);
        }
        
        public static void EaseScale(this Transform transform, Vector3 from, Vector3 to, float t, EasingSettings easing)
        {
            transform.localScale = easing.Lerp(from, to, t);
        }
        
        // Time-based helpers
        public static float GetTimeProgress(float startTime, float duration)
        {
            return Mathf.Clamp01((Time.time - startTime) / duration);
        }
        
        public static float GetDeltaTimeProgress(ref float currentTime, float duration)
        {
            currentTime += Time.deltaTime;
            return Mathf.Clamp01(currentTime / duration);
        }
    }
    
    // Static utility class for common easing presets
    public static class EasingPresets
    {
        public static readonly EasingSettings Linear = new EasingSettings(EasingType.Linear);
        public static readonly EasingSettings EaseOutQuart = new EasingSettings(EasingType.EaseOutQuart);
        public static readonly EasingSettings EaseInOutQuad = new EasingSettings(EasingType.EaseInOutQuad);
        public static readonly EasingSettings EaseOutBounce = new EasingSettings(EasingType.EaseOutBounce);
        public static readonly EasingSettings EaseOutElastic = new EasingSettings(EasingType.EaseOutElastic);
        public static readonly EasingSettings EaseInOutCubic = new EasingSettings(EasingType.EaseInOutCubic);
        
        // Common animation curves
        public static EasingSettings SmoothStep => new EasingSettings(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
        public static EasingSettings QuickStart => new EasingSettings(EasingType.EaseOutQuad);
        public static EasingSettings SlowStart => new EasingSettings(EasingType.EaseInQuad);
        public static EasingSettings Bounce => new EasingSettings(EasingType.EaseOutBounce);
        public static EasingSettings Elastic => new EasingSettings(EasingType.EaseOutElastic);
    }
}