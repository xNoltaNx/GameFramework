using System.Collections;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that animates light properties over time.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Light Action")]
    [ActionDefinition("light-control", "💡", "Light Action", "Animates light properties including intensity, color, range, and spot angle over time", "Visual", 160)]
    public class LightAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum LightPropertyType
        {
            Intensity,
            Color,
            Range,
            SpotAngle,
            Enable,
            Multiple
        }
        
        [Header("Light Settings")]
        [SerializeField] private LightPropertyType propertyType = LightPropertyType.Intensity;
        [SerializeField] private Light targetLight;
        [SerializeField] private bool createLightIfMissing = false;
        
        [Header("Animation")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private EasingSettings easingSettings = new EasingSettings();
        [SerializeField] private bool animateFromCurrent = true;
        
        [Header("Intensity Settings")]
        [SerializeField] private float targetIntensity = 1f;
        [SerializeField] private float startIntensity = 0f;
        
        [Header("Color Settings")]
        [SerializeField] private Color targetColor = Color.white;
        [SerializeField] private Color startColor = Color.black;
        [SerializeField] private bool animateTemperature = false;
        [SerializeField] private float targetTemperature = 6570f;
        [SerializeField] private float startTemperature = 2700f;
        
        [Header("Range Settings")]
        [SerializeField] private float targetRange = 10f;
        [SerializeField] private float startRange = 0f;
        
        [Header("Spot Angle Settings")]
        [SerializeField] private float targetSpotAngle = 30f;
        [SerializeField] private float startSpotAngle = 180f;
        
        [Header("Enable/Disable Settings")]
        [SerializeField] private bool enableLight = true;
        
        [Header("Multiple Properties")]
        [SerializeField] private bool animateIntensity = false;
        [SerializeField] private bool animateColor = false;
        [SerializeField] private bool animateRange = false;
        [SerializeField] private bool animateSpotAngle = false;
        
        [Header("Flicker Effect")]
        [SerializeField] private bool enableFlicker = false;
        [SerializeField] private float flickerSpeed = 5f;
        [SerializeField] private float flickerAmount = 0.1f;
        [SerializeField] private float flickerDuration = 2f;
        
        private float originalIntensity;
        private Color originalColor;
        private float originalRange;
        private float originalSpotAngle;
        private Coroutine animationCoroutine;
        private Coroutine flickerCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (!SetupLight())
            {
                return;
            }
            
            CacheOriginalValues();
            
            if (duration <= 0f)
            {
                SetPropertiesImmediate();
            }
            else
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(AnimateLight());
            }
            
            if (enableFlicker)
            {
                StartFlicker();
            }
        }
        
        private bool SetupLight()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
                
                if (targetLight == null && createLightIfMissing)
                {
                    targetLight = gameObject.AddComponent<Light>();
                    LogDebug("Created Light component");
                }
                
                if (targetLight == null)
                {
                    LogWarning("No Light found and createLightIfMissing is false");
                    return false;
                }
            }
            
            return true;
        }
        
        private void CacheOriginalValues()
        {
            originalIntensity = targetLight.intensity;
            originalColor = targetLight.color;
            originalRange = targetLight.range;
            originalSpotAngle = targetLight.spotAngle;
        }
        
        private void SetPropertiesImmediate()
        {
            switch (propertyType)
            {
                case LightPropertyType.Intensity:
                    targetLight.intensity = targetIntensity;
                    break;
                case LightPropertyType.Color:
                    SetColor(targetColor);
                    break;
                case LightPropertyType.Range:
                    targetLight.range = targetRange;
                    break;
                case LightPropertyType.SpotAngle:
                    targetLight.spotAngle = targetSpotAngle;
                    break;
                case LightPropertyType.Enable:
                    targetLight.enabled = enableLight;
                    break;
                case LightPropertyType.Multiple:
                    SetMultipleProperties();
                    break;
            }
            
            LogDebug($"Set light property {propertyType} immediately");
        }
        
        private void SetColor(Color color)
        {
            if (animateTemperature)
            {
                targetLight.colorTemperature = targetTemperature;
            }
            else
            {
                targetLight.color = color;
            }
        }
        
        private void SetMultipleProperties()
        {
            if (animateIntensity)
                targetLight.intensity = targetIntensity;
            if (animateColor)
                SetColor(targetColor);
            if (animateRange)
                targetLight.range = targetRange;
            if (animateSpotAngle && targetLight.type == LightType.Spot)
                targetLight.spotAngle = targetSpotAngle;
        }
        
        private IEnumerator AnimateLight()
        {
            // Get start values
            float currentStartIntensity = animateFromCurrent ? originalIntensity : startIntensity;
            Color currentStartColor = animateFromCurrent ? originalColor : startColor;
            float currentStartRange = animateFromCurrent ? originalRange : startRange;
            float currentStartSpotAngle = animateFromCurrent ? originalSpotAngle : startSpotAngle;
            float currentStartTemperature = animateFromCurrent ? targetLight.colorTemperature : startTemperature;
            
            float elapsedTime = 0f;
            
            LogDebug($"Animating light property {propertyType} over {duration}s");
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float easedT = easingSettings.Evaluate(t);
                
                switch (propertyType)
                {
                    case LightPropertyType.Intensity:
                        targetLight.intensity = Mathf.Lerp(currentStartIntensity, targetIntensity, easedT);
                        break;
                        
                    case LightPropertyType.Color:
                        if (animateTemperature)
                        {
                            targetLight.colorTemperature = Mathf.Lerp(currentStartTemperature, targetTemperature, easedT);
                        }
                        else
                        {
                            targetLight.color = Color.Lerp(currentStartColor, targetColor, easedT);
                        }
                        break;
                        
                    case LightPropertyType.Range:
                        targetLight.range = Mathf.Lerp(currentStartRange, targetRange, easedT);
                        break;
                        
                    case LightPropertyType.SpotAngle:
                        if (targetLight.type == LightType.Spot)
                        {
                            targetLight.spotAngle = Mathf.Lerp(currentStartSpotAngle, targetSpotAngle, easedT);
                        }
                        break;
                        
                    case LightPropertyType.Enable:
                        // For enable/disable, switch at halfway point
                        if (easedT >= 0.5f)
                        {
                            targetLight.enabled = enableLight;
                        }
                        break;
                        
                    case LightPropertyType.Multiple:
                        AnimateMultipleProperties(easedT, currentStartIntensity, currentStartColor, 
                                                currentStartRange, currentStartSpotAngle, currentStartTemperature);
                        break;
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Set final values
            SetPropertiesImmediate();
            animationCoroutine = null;
        }
        
        private void AnimateMultipleProperties(float t, float startInt, Color startCol, float startRange, float startSpot, float startTemp)
        {
            if (animateIntensity)
                targetLight.intensity = Mathf.Lerp(startInt, targetIntensity, t);
                
            if (animateColor)
            {
                if (animateTemperature)
                    targetLight.colorTemperature = Mathf.Lerp(startTemp, targetTemperature, t);
                else
                    targetLight.color = Color.Lerp(startCol, targetColor, t);
            }
            
            if (animateRange)
                targetLight.range = Mathf.Lerp(startRange, targetRange, t);
                
            if (animateSpotAngle && targetLight.type == LightType.Spot)
                targetLight.spotAngle = Mathf.Lerp(startSpot, targetSpotAngle, t);
        }
        
        private void StartFlicker()
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
            }
            
            flickerCoroutine = StartCoroutine(FlickerEffect());
        }
        
        private IEnumerator FlickerEffect()
        {
            float originalIntensityForFlicker = targetLight.intensity;
            float elapsedTime = 0f;
            
            while (elapsedTime < flickerDuration)
            {
                float flicker = Mathf.Sin(Time.time * flickerSpeed) * flickerAmount;
                targetLight.intensity = originalIntensityForFlicker + flicker;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            targetLight.intensity = originalIntensityForFlicker;
            flickerCoroutine = null;
            
            LogDebug("Flicker effect completed");
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
        }
        
        #region Public API
        
        public void SetTargetLight(Light light)
        {
            targetLight = light;
        }
        
        public void SetTargetIntensity(float intensity)
        {
            targetIntensity = Mathf.Max(0f, intensity);
            propertyType = LightPropertyType.Intensity;
        }
        
        public void SetTargetColor(Color color)
        {
            targetColor = color;
            propertyType = LightPropertyType.Color;
            animateTemperature = false;
        }
        
        public void SetTargetTemperature(float temperature)
        {
            targetTemperature = Mathf.Clamp(temperature, 1000f, 20000f);
            propertyType = LightPropertyType.Color;
            animateTemperature = true;
        }
        
        public void SetTargetRange(float range)
        {
            targetRange = Mathf.Max(0f, range);
            propertyType = LightPropertyType.Range;
        }
        
        public void SetTargetSpotAngle(float angle)
        {
            targetSpotAngle = Mathf.Clamp(angle, 1f, 179f);
            propertyType = LightPropertyType.SpotAngle;
        }
        
        public void SetEnable(bool enable)
        {
            enableLight = enable;
            propertyType = LightPropertyType.Enable;
        }
        
        public void StartFlickerEffect()
        {
            enableFlicker = true;
            StartFlicker();
        }
        
        public void StopFlickerEffect()
        {
            enableFlicker = false;
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            duration = Mathf.Max(0f, duration);
            targetIntensity = Mathf.Max(0f, targetIntensity);
            startIntensity = Mathf.Max(0f, startIntensity);
            targetRange = Mathf.Max(0f, targetRange);
            startRange = Mathf.Max(0f, startRange);
            targetSpotAngle = Mathf.Clamp(targetSpotAngle, 1f, 179f);
            startSpotAngle = Mathf.Clamp(startSpotAngle, 1f, 179f);
            targetTemperature = Mathf.Clamp(targetTemperature, 1000f, 20000f);
            startTemperature = Mathf.Clamp(startTemperature, 1000f, 20000f);
            flickerSpeed = Mathf.Max(0.1f, flickerSpeed);
            flickerAmount = Mathf.Max(0f, flickerAmount);
            flickerDuration = Mathf.Max(0.1f, flickerDuration);
        }
        #endif
    }
}