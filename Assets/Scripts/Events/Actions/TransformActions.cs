using System.Collections;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that moves a transform to a target position over time.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Move Action")]
    [ActionDefinition("move", "📐", "Move Action", "Smoothly moves transforms to target positions with easing", "Transform", 60)]
    public class MoveAction : BaseTriggerAction
    {
        [Header("Move Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private bool useLocalPosition = false;
        [SerializeField] private bool useTargetTransform = true;
        [SerializeField] private float duration = 1f;
        [SerializeField] private EasingSettings easingSettings = new EasingSettings();
        [SerializeField] private bool moveRelative = false;
        
        private Vector3 startPosition;
        private Vector3 endPosition;
        private Coroutine moveCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
            
            if (duration <= 0f)
            {
                // Instant move
                SetPosition(GetTargetPosition());
                return;
            }
            
            // Animated move
            moveCoroutine = StartCoroutine(MoveCoroutine());
        }
        
        private IEnumerator MoveCoroutine()
        {
            startPosition = GetCurrentPosition();
            endPosition = GetTargetPosition();
            
            if (moveRelative)
            {
                endPosition = startPosition + endPosition;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float easedT = easingSettings.Evaluate(t);
                
                Vector3 currentPos = Vector3.Lerp(startPosition, endPosition, easedT);
                SetPosition(currentPos);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            SetPosition(endPosition);
            moveCoroutine = null;
        }
        
        private Vector3 GetCurrentPosition()
        {
            return useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        }
        
        private Vector3 GetTargetPosition()
        {
            if (useTargetTransform && targetTransform != null)
            {
                return useLocalPosition ? targetTransform.localPosition : targetTransform.position;
            }
            return targetPosition;
        }
        
        private void SetPosition(Vector3 position)
        {
            if (useLocalPosition)
            {
                targetTransform.localPosition = position;
            }
            else
            {
                targetTransform.position = position;
            }
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
        }
        
        public void SetTarget(Vector3 position)
        {
            targetPosition = position;
            useTargetTransform = false;
        }
        
        public void SetTarget(Transform transform)
        {
            targetTransform = transform;
            useTargetTransform = true;
        }
    }
    
    /// <summary>
    /// Action that rotates a transform to a target rotation over time.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Rotate Action")]
    [ActionDefinition("rotate", "🔄", "Rotate Action", "Smoothly rotates transforms to target rotations with easing", "Transform", 70)]
    public class RotateAction : BaseTriggerAction
    {
        [Header("Rotate Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private bool useLocalRotation = false;
        [SerializeField] private float duration = 1f;
        [SerializeField] private EasingSettings easingSettings = new EasingSettings();
        [SerializeField] private bool rotateRelative = false;
        
        private Quaternion startRotation;
        private Quaternion endRotation;
        private Coroutine rotateCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
            
            if (duration <= 0f)
            {
                // Instant rotation
                SetRotation(GetTargetRotation());
                return;
            }
            
            // Animated rotation
            rotateCoroutine = StartCoroutine(RotateCoroutine());
        }
        
        private IEnumerator RotateCoroutine()
        {
            startRotation = GetCurrentRotation();
            endRotation = GetTargetRotation();
            
            if (rotateRelative)
            {
                endRotation = startRotation * endRotation;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float easedT = easingSettings.Evaluate(t);
                
                Quaternion currentRot = Quaternion.Lerp(startRotation, endRotation, easedT);
                SetRotation(currentRot);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            SetRotation(endRotation);
            rotateCoroutine = null;
        }
        
        private Quaternion GetCurrentRotation()
        {
            return useLocalRotation ? targetTransform.localRotation : targetTransform.rotation;
        }
        
        private Quaternion GetTargetRotation()
        {
            return Quaternion.Euler(targetRotation);
        }
        
        private void SetRotation(Quaternion rotation)
        {
            if (useLocalRotation)
            {
                targetTransform.localRotation = rotation;
            }
            else
            {
                targetTransform.rotation = rotation;
            }
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
                rotateCoroutine = null;
            }
        }
        
        public void SetTargetRotation(Vector3 rotation)
        {
            targetRotation = rotation;
        }
    }
    
    /// <summary>
    /// Action that scales a transform to a target scale over time.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Scale Action")]
    [ActionDefinition("scale", "📏", "Scale Action", "Smoothly scales transforms to target sizes with easing", "Transform", 80)]
    public class ScaleAction : BaseTriggerAction
    {
        [Header("Scale Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 targetScale = Vector3.one;
        [SerializeField] private float duration = 1f;
        [SerializeField] private EasingSettings easingSettings = new EasingSettings();
        [SerializeField] private bool scaleRelative = false;
        [SerializeField] private bool uniformScale = false;
        [SerializeField] private float uniformScaleValue = 1f;
        
        private Vector3 startScale;
        private Vector3 endScale;
        private Coroutine scaleCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }
            
            if (duration <= 0f)
            {
                // Instant scale
                SetScale(GetTargetScale());
                return;
            }
            
            // Animated scale
            scaleCoroutine = StartCoroutine(ScaleCoroutine());
        }
        
        private IEnumerator ScaleCoroutine()
        {
            startScale = targetTransform.localScale;
            endScale = GetTargetScale();
            
            if (scaleRelative)
            {
                endScale = Vector3.Scale(startScale, endScale);
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float easedT = easingSettings.Evaluate(t);
                
                Vector3 currentScale = Vector3.Lerp(startScale, endScale, easedT);
                SetScale(currentScale);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            SetScale(endScale);
            scaleCoroutine = null;
        }
        
        private Vector3 GetTargetScale()
        {
            if (uniformScale)
            {
                return Vector3.one * uniformScaleValue;
            }
            return targetScale;
        }
        
        private void SetScale(Vector3 scale)
        {
            targetTransform.localScale = scale;
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
        }
        
        public void SetTargetScale(Vector3 scale)
        {
            targetScale = scale;
            uniformScale = false;
        }
        
        public void SetUniformScale(float scale)
        {
            uniformScaleValue = scale;
            uniformScale = true;
        }
    }
}