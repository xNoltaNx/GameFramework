using UnityEngine;

namespace GameFramework.Core
{
    public class EasingDemo : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float duration = 2f;
        [SerializeField] private bool autoRestart = true;
        [SerializeField] private bool pingPong = true;
        
        [Header("Position Animation")]
        [SerializeField] private EasingSettings positionEasing = new EasingSettings(EasingType.EaseOutQuart);
        [SerializeField] private Vector3 startPosition = Vector3.zero;
        [SerializeField] private Vector3 endPosition = Vector3.right * 5f;
        
        [Header("Scale Animation")]
        [SerializeField] private EasingSettings scaleEasing = new EasingSettings(EasingType.EaseOutElastic);
        [SerializeField] private Vector3 startScale = Vector3.one;
        [SerializeField] private Vector3 endScale = Vector3.one * 2f;
        
        [Header("Color Animation")]
        [SerializeField] private EasingSettings colorEasing = new EasingSettings(EasingType.EaseInOutSine);
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = Color.red;
        
        private float animationTime = 0f;
        private bool isReversed = false;
        private Renderer objectRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        
        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
            
            // Set initial values
            transform.position = startPosition;
            transform.localScale = startScale;
            
            if (objectRenderer != null)
            {
                materialPropertyBlock.SetColor("_Color", startColor);
                objectRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        
        private void Update()
        {
            // Update animation time
            animationTime += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTime / duration);
            
            // Handle ping pong
            float easingProgress = progress;
            if (pingPong)
            {
                if (progress >= 1f && !isReversed)
                {
                    isReversed = true;
                    animationTime = 0f;
                }
                else if (progress >= 1f && isReversed)
                {
                    isReversed = false;
                    animationTime = 0f;
                    if (!autoRestart) return;
                }
                
                easingProgress = isReversed ? 1f - progress : progress;
            }
            else if (progress >= 1f)
            {
                if (autoRestart)
                {
                    animationTime = 0f;
                }
                else
                {
                    easingProgress = 1f;
                }
            }
            
            // Apply animations using the easing system
            AnimatePosition(easingProgress);
            AnimateScale(easingProgress);
            AnimateColor(easingProgress);
        }
        
        private void AnimatePosition(float progress)
        {
            Vector3 currentPos = positionEasing.Lerp(startPosition, endPosition, progress);
            transform.position = currentPos;
        }
        
        private void AnimateScale(float progress)
        {
            Vector3 currentScale = scaleEasing.Lerp(startScale, endScale, progress);
            transform.localScale = currentScale;
        }
        
        private void AnimateColor(float progress)
        {
            if (objectRenderer != null && materialPropertyBlock != null)
            {
                Color currentColor = colorEasing.Lerp(startColor, endColor, progress);
                materialPropertyBlock.SetColor("_Color", currentColor);
                objectRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        
        [ContextMenu("Reset Animation")]
        public void ResetAnimation()
        {
            animationTime = 0f;
            isReversed = false;
            
            transform.position = startPosition;
            transform.localScale = startScale;
            
            if (objectRenderer != null && materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor("_Color", startColor);
                objectRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        
        [ContextMenu("Test All Easing Types")]
        public void TestAllEasingTypes()
        {
            Debug.Log("Testing all easing types:");
            foreach (EasingType easingType in System.Enum.GetValues(typeof(EasingType)))
            {
                if (easingType != EasingType.CustomCurve)
                {
                    float result = Easing.Evaluate(easingType, 0.5f);
                    Debug.Log($"{easingType}: f(0.5) = {result:F3}");
                }
            }
        }
    }
}