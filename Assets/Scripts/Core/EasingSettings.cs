using UnityEngine;

namespace GameFramework.Core
{
    [System.Serializable]
    public class EasingSettings
    {
        [SerializeField] private EasingType easingType = EasingType.Linear;
        [SerializeField] private AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public EasingType EasingType => easingType;
        public AnimationCurve CustomCurve => customCurve;
        
        public EasingSettings()
        {
            easingType = EasingType.Linear;
            customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        
        public EasingSettings(EasingType type)
        {
            easingType = type;
            customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        
        public EasingSettings(AnimationCurve curve)
        {
            easingType = EasingType.CustomCurve;
            customCurve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        
        /// <summary>
        /// Evaluate the easing function at time t (0-1)
        /// </summary>
        public float Evaluate(float t)
        {
            return Easing.Evaluate(easingType, t, easingType == EasingType.CustomCurve ? customCurve : null);
        }
        
        /// <summary>
        /// Lerp between two float values using this easing
        /// </summary>
        public float Lerp(float a, float b, float t)
        {
            return Easing.Lerp(a, b, t, easingType, easingType == EasingType.CustomCurve ? customCurve : null);
        }
        
        /// <summary>
        /// Lerp between two Vector3 values using this easing
        /// </summary>
        public Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Easing.Lerp(a, b, t, easingType, easingType == EasingType.CustomCurve ? customCurve : null);
        }
        
        /// <summary>
        /// Lerp between two Color values using this easing
        /// </summary>
        public Color Lerp(Color a, Color b, float t)
        {
            return Easing.Lerp(a, b, t, easingType, easingType == EasingType.CustomCurve ? customCurve : null);
        }
        
        /// <summary>
        /// Create a copy of this EasingSettings
        /// </summary>
        public EasingSettings Clone()
        {
            var clone = new EasingSettings(easingType);
            clone.customCurve = new AnimationCurve(customCurve.keys);
            return clone;
        }
        
        /// <summary>
        /// Get a user-friendly name for the current easing type
        /// </summary>
        public string GetDisplayName()
        {
            return easingType switch
            {
                EasingType.Linear => "Linear",
                EasingType.EaseInQuad => "Ease In Quad",
                EasingType.EaseOutQuad => "Ease Out Quad",
                EasingType.EaseInOutQuad => "Ease In-Out Quad",
                EasingType.EaseInCubic => "Ease In Cubic",
                EasingType.EaseOutCubic => "Ease Out Cubic",
                EasingType.EaseInOutCubic => "Ease In-Out Cubic",
                EasingType.EaseInQuart => "Ease In Quart",
                EasingType.EaseOutQuart => "Ease Out Quart",
                EasingType.EaseInOutQuart => "Ease In-Out Quart",
                EasingType.EaseInQuint => "Ease In Quint",
                EasingType.EaseOutQuint => "Ease Out Quint",
                EasingType.EaseInOutQuint => "Ease In-Out Quint",
                EasingType.EaseInSine => "Ease In Sine",
                EasingType.EaseOutSine => "Ease Out Sine",
                EasingType.EaseInOutSine => "Ease In-Out Sine",
                EasingType.EaseInExpo => "Ease In Expo",
                EasingType.EaseOutExpo => "Ease Out Expo",
                EasingType.EaseInOutExpo => "Ease In-Out Expo",
                EasingType.EaseInCirc => "Ease In Circ",
                EasingType.EaseOutCirc => "Ease Out Circ",
                EasingType.EaseInOutCirc => "Ease In-Out Circ",
                EasingType.EaseInBack => "Ease In Back",
                EasingType.EaseOutBack => "Ease Out Back",
                EasingType.EaseInOutBack => "Ease In-Out Back",
                EasingType.EaseInElastic => "Ease In Elastic",
                EasingType.EaseOutElastic => "Ease Out Elastic",
                EasingType.EaseInOutElastic => "Ease In-Out Elastic",
                EasingType.EaseInBounce => "Ease In Bounce",
                EasingType.EaseOutBounce => "Ease Out Bounce",
                EasingType.EaseInOutBounce => "Ease In-Out Bounce",
                EasingType.CustomCurve => "Custom Curve",
                _ => easingType.ToString()
            };
        }
    }
}