using UnityEngine;
using UnityEngine.Playables;

#if UNITY_TIMELINE
using UnityEngine.Timeline;
#endif

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that controls Animator and Timeline components.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Animation Action")]
    [ActionDefinition("animation-control", "🎬", "Animation Action", "Controls Animator and Timeline components with play, parameters, and speed modifications", "Animation", 190)]
    public class AnimationAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum AnimationActionType
        {
            PlayAnimation,
            StopAnimation,
            PauseAnimation,
            ResumeAnimation,
            SetAnimatorBool,
            SetAnimatorInt,
            SetAnimatorFloat,
            SetAnimatorTrigger,
            PlayTimeline,
            StopTimeline,
            PauseTimeline,
            ResumeTimeline,
            SetAnimatorSpeed
        }
        
        [System.Serializable]
        public enum ParameterType
        {
            Bool,
            Int,
            Float,
            Trigger
        }
        
        [Header("Animation Settings")]
        [SerializeField] private AnimationActionType actionType = AnimationActionType.PlayAnimation;
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private bool useContext = false;
        [SerializeField] private bool createAnimatorIfMissing = false;
        
        [Header("Animation Clip Settings")]
        [SerializeField] private string animationStateName = "";
        [SerializeField] private int layerIndex = 0;
        [SerializeField] private float normalizedTime = 0f;
        [SerializeField] private float crossFadeDuration = 0.1f;
        [SerializeField] private bool useCrossFade = true;
        
        [Header("Animator Parameters")]
        [SerializeField] private string parameterName = "";
        [SerializeField] private bool boolValue = true;
        [SerializeField] private int intValue = 0;
        [SerializeField] private float floatValue = 1f;
        [SerializeField] private bool randomizeValue = false;
        [SerializeField] private Vector2 floatRange = new Vector2(0f, 1f);
        [SerializeField] private Vector2Int intRange = new Vector2Int(0, 10);
        
        [Header("Timeline Settings")]
        #if UNITY_TIMELINE
        [SerializeField] private TimelineAsset timelineAsset;
        #else
        [SerializeField] private ScriptableObject timelineAsset; // Fallback for when Timeline package is not installed
        #endif
        [SerializeField] private double timelineStartTime = 0.0;
        [SerializeField] private bool pauseOnStart = false;
        
        [Header("Speed Settings")]
        [SerializeField] private float animatorSpeed = 1f;
        [SerializeField] private bool animateSpeed = false;
        [SerializeField] private float speedDuration = 1f;
        [SerializeField] private float startSpeed = 0f;
        
        protected override void PerformAction(GameObject context)
        {
            switch (actionType)
            {
                case AnimationActionType.PlayAnimation:
                case AnimationActionType.StopAnimation:
                case AnimationActionType.PauseAnimation:
                case AnimationActionType.ResumeAnimation:
                case AnimationActionType.SetAnimatorBool:
                case AnimationActionType.SetAnimatorInt:
                case AnimationActionType.SetAnimatorFloat:
                case AnimationActionType.SetAnimatorTrigger:
                case AnimationActionType.SetAnimatorSpeed:
                    HandleAnimatorAction(context);
                    break;
                    
                case AnimationActionType.PlayTimeline:
                case AnimationActionType.StopTimeline:
                case AnimationActionType.PauseTimeline:
                case AnimationActionType.ResumeTimeline:
                    HandleTimelineAction(context);
                    break;
            }
        }
        
        private void HandleAnimatorAction(GameObject context)
        {
            Animator animator = GetTargetAnimator(context);
            
            if (animator == null)
            {
                LogWarning("No Animator found for animation action");
                return;
            }
            
            switch (actionType)
            {
                case AnimationActionType.PlayAnimation:
                    PlayAnimation(animator);
                    break;
                case AnimationActionType.StopAnimation:
                    StopAnimation(animator);
                    break;
                case AnimationActionType.PauseAnimation:
                    PauseAnimation(animator);
                    break;
                case AnimationActionType.ResumeAnimation:
                    ResumeAnimation(animator);
                    break;
                case AnimationActionType.SetAnimatorBool:
                    SetAnimatorBool(animator);
                    break;
                case AnimationActionType.SetAnimatorInt:
                    SetAnimatorInt(animator);
                    break;
                case AnimationActionType.SetAnimatorFloat:
                    SetAnimatorFloat(animator);
                    break;
                case AnimationActionType.SetAnimatorTrigger:
                    SetAnimatorTrigger(animator);
                    break;
                case AnimationActionType.SetAnimatorSpeed:
                    SetAnimatorSpeed(animator);
                    break;
            }
        }
        
        private void HandleTimelineAction(GameObject context)
        {
            #if UNITY_TIMELINE
            PlayableDirector director = GetTargetPlayableDirector(context);
            
            if (director == null)
            {
                LogWarning("No PlayableDirector found for timeline action");
                return;
            }
            
            switch (actionType)
            {
                case AnimationActionType.PlayTimeline:
                    PlayTimeline(director);
                    break;
                case AnimationActionType.StopTimeline:
                    StopTimeline(director);
                    break;
                case AnimationActionType.PauseTimeline:
                    PauseTimeline(director);
                    break;
                case AnimationActionType.ResumeTimeline:
                    ResumeTimeline(director);
                    break;
            }
            #else
            LogWarning("Timeline actions require the Timeline package to be installed. Please install via Package Manager.");
            #endif
        }
        
        private Animator GetTargetAnimator(GameObject context)
        {
            Animator animator = null;
            
            if (useContext && context != null)
            {
                animator = context.GetComponent<Animator>();
            }
            else if (targetAnimator != null)
            {
                animator = targetAnimator;
            }
            else
            {
                animator = GetComponent<Animator>();
            }
            
            if (animator == null && createAnimatorIfMissing)
            {
                GameObject targetObj = useContext && context != null ? context : gameObject;
                animator = targetObj.AddComponent<Animator>();
                LogDebug($"Created Animator on {targetObj.name}");
            }
            
            return animator;
        }
        
        private PlayableDirector GetTargetPlayableDirector(GameObject context)
        {
            #if UNITY_TIMELINE
            if (useContext && context != null)
            {
                return context.GetComponent<PlayableDirector>();
            }
            else if (playableDirector != null)
            {
                return playableDirector;
            }
            else
            {
                return GetComponent<PlayableDirector>();
            }
            #else
            return null;
            #endif
        }
        
        private void PlayAnimation(Animator animator)
        {
            if (string.IsNullOrEmpty(animationStateName))
            {
                LogWarning("No animation state name specified");
                return;
            }
            
            if (useCrossFade)
            {
                animator.CrossFade(animationStateName, crossFadeDuration, layerIndex, normalizedTime);
                LogDebug($"Cross-fading to animation: {animationStateName}");
            }
            else
            {
                animator.Play(animationStateName, layerIndex, normalizedTime);
                LogDebug($"Playing animation: {animationStateName}");
            }
        }
        
        private void StopAnimation(Animator animator)
        {
            animator.StopPlayback();
            LogDebug("Stopped animation playback");
        }
        
        private void PauseAnimation(Animator animator)
        {
            animator.speed = 0f;
            LogDebug("Paused animation");
        }
        
        private void ResumeAnimation(Animator animator)
        {
            animator.speed = animatorSpeed;
            LogDebug($"Resumed animation with speed: {animatorSpeed}");
        }
        
        private void SetAnimatorBool(Animator animator)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                LogWarning("No parameter name specified for bool parameter");
                return;
            }
            
            animator.SetBool(parameterName, boolValue);
            LogDebug($"Set bool parameter {parameterName} to {boolValue}");
        }
        
        private void SetAnimatorInt(Animator animator)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                LogWarning("No parameter name specified for int parameter");
                return;
            }
            
            int finalValue = randomizeValue ? Random.Range(intRange.x, intRange.y + 1) : intValue;
            animator.SetInteger(parameterName, finalValue);
            LogDebug($"Set int parameter {parameterName} to {finalValue}");
        }
        
        private void SetAnimatorFloat(Animator animator)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                LogWarning("No parameter name specified for float parameter");
                return;
            }
            
            float finalValue = randomizeValue ? Random.Range(floatRange.x, floatRange.y) : floatValue;
            animator.SetFloat(parameterName, finalValue);
            LogDebug($"Set float parameter {parameterName} to {finalValue}");
        }
        
        private void SetAnimatorTrigger(Animator animator)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                LogWarning("No parameter name specified for trigger parameter");
                return;
            }
            
            animator.SetTrigger(parameterName);
            LogDebug($"Triggered parameter: {parameterName}");
        }
        
        private void SetAnimatorSpeed(Animator animator)
        {
            if (animateSpeed && speedDuration > 0f)
            {
                StartCoroutine(AnimateAnimatorSpeed(animator));
            }
            else
            {
                animator.speed = animatorSpeed;
                LogDebug($"Set animator speed to: {animatorSpeed}");
            }
        }
        
        private System.Collections.IEnumerator AnimateAnimatorSpeed(Animator animator)
        {
            float elapsedTime = 0f;
            float currentStartSpeed = animator.speed;
            
            if (!animateSpeed)
            {
                currentStartSpeed = startSpeed;
            }
            
            while (elapsedTime < speedDuration)
            {
                float t = elapsedTime / speedDuration;
                float currentSpeed = Mathf.Lerp(currentStartSpeed, animatorSpeed, t);
                animator.speed = currentSpeed;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            animator.speed = animatorSpeed;
            LogDebug($"Animated animator speed from {currentStartSpeed} to {animatorSpeed}");
        }
        
        #if UNITY_TIMELINE
        private void PlayTimeline(PlayableDirector director)
        {
            if (timelineAsset != null)
            {
                director.playableAsset = timelineAsset as TimelineAsset;
            }
            
            if (timelineStartTime > 0.0)
            {
                director.time = timelineStartTime;
            }
            
            director.Play();
            
            if (pauseOnStart)
            {
                director.Pause();
            }
            
            LogDebug($"Playing timeline: {(timelineAsset ? timelineAsset.name : "current")}");
        }
        
        private void StopTimeline(PlayableDirector director)
        {
            director.Stop();
            LogDebug("Stopped timeline");
        }
        
        private void PauseTimeline(PlayableDirector director)
        {
            director.Pause();
            LogDebug("Paused timeline");
        }
        
        private void ResumeTimeline(PlayableDirector director)
        {
            director.Resume();
            LogDebug("Resumed timeline");
        }
        #endif
        
        #region Public API
        
        public void SetTargetAnimator(Animator animator)
        {
            targetAnimator = animator;
            useContext = false;
        }
        
        public void SetTargetPlayableDirector(PlayableDirector director)
        {
            playableDirector = director;
            useContext = false;
        }
        
        public void SetAnimationState(string stateName, int layer = 0)
        {
            animationStateName = stateName;
            layerIndex = layer;
        }
        
        public void SetAnimatorParameter(string paramName, bool value)
        {
            parameterName = paramName;
            boolValue = value;
            actionType = AnimationActionType.SetAnimatorBool;
        }
        
        public void SetAnimatorParameter(string paramName, int value)
        {
            parameterName = paramName;
            intValue = value;
            actionType = AnimationActionType.SetAnimatorInt;
        }
        
        public void SetAnimatorParameter(string paramName, float value)
        {
            parameterName = paramName;
            floatValue = value;
            actionType = AnimationActionType.SetAnimatorFloat;
        }
        
        public void TriggerAnimatorParameter(string paramName)
        {
            parameterName = paramName;
            actionType = AnimationActionType.SetAnimatorTrigger;
        }
        
        #if UNITY_TIMELINE
        public void SetTimelineAsset(TimelineAsset asset)
        {
            timelineAsset = asset;
        }
        #else
        public void SetTimelineAsset(ScriptableObject asset)
        {
            timelineAsset = asset;
            LogWarning("Timeline functionality requires the Timeline package to be installed.");
        }
        #endif
        
        public void SetAnimatorSpeed(float speed, bool animate = false, float duration = 1f)
        {
            animatorSpeed = speed;
            animateSpeed = animate;
            speedDuration = duration;
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            layerIndex = Mathf.Max(0, layerIndex);
            normalizedTime = Mathf.Clamp01(normalizedTime);
            crossFadeDuration = Mathf.Max(0f, crossFadeDuration);
            speedDuration = Mathf.Max(0.1f, speedDuration);
            animatorSpeed = Mathf.Max(0f, animatorSpeed);
            startSpeed = Mathf.Max(0f, startSpeed);
            timelineStartTime = System.Math.Max(0.0, timelineStartTime);
            
            // Ensure ranges are valid
            if (floatRange.x > floatRange.y)
            {
                floatRange.y = floatRange.x;
            }
            
            if (intRange.x > intRange.y)
            {
                intRange.y = intRange.x;
            }
        }
        #endif
    }
}