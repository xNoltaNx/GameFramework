using System.Collections;
using UnityEngine;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that controls particle systems with various configuration options.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Particle Action")]
    [ActionDefinition("particle-control", "✨", "Particle Action", "Controls particle systems with play, stop, emission rate, burst, and property modifications", "Visual", 170)]
    public class ParticleAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum ParticleActionType
        {
            Play,
            Stop,
            Pause,
            Resume,
            Clear,
            Restart,
            SetEmissionRate,
            Burst,
            SetSpeed,
            SetLifetime
        }
        
        [Header("Particle Settings")]
        [SerializeField] private ParticleActionType actionType = ParticleActionType.Play;
        [SerializeField] private ParticleSystem targetParticleSystem;
        [SerializeField] private bool affectChildren = true;
        [SerializeField] private bool createParticleSystemIfMissing = false;
        
        [Header("Playback Settings")]
        [SerializeField] private bool withChildren = true;
        [SerializeField] private bool stopAndClear = false;
        [SerializeField] private float delayBeforeAction = 0f;
        
        [Header("Emission Settings")]
        [SerializeField] private float emissionRate = 10f;
        [SerializeField] private bool animateEmissionRate = false;
        [SerializeField] private float emissionDuration = 1f;
        [SerializeField] private float startEmissionRate = 0f;
        
        [Header("Burst Settings")]
        [SerializeField] private int burstCount = 10;
        [SerializeField] private float burstTime = 0f;
        
        [Header("Speed Settings")]
        [SerializeField] private float startSpeed = 5f;
        [SerializeField] private bool randomizeSpeed = false;
        [SerializeField] private Vector2 speedRange = new Vector2(3f, 7f);
        
        [Header("Lifetime Settings")]
        [SerializeField] private float startLifetime = 2f;
        [SerializeField] private bool randomizeLifetime = false;
        [SerializeField] private Vector2 lifetimeRange = new Vector2(1f, 3f);
        
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.MainModule mainModule;
        private Coroutine animationCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (!SetupParticleSystem())
            {
                return;
            }
            
            if (delayBeforeAction > 0f)
            {
                StartCoroutine(DelayedAction());
            }
            else
            {
                ExecuteParticleAction();
            }
        }
        
        private bool SetupParticleSystem()
        {
            if (targetParticleSystem == null)
            {
                targetParticleSystem = GetComponent<ParticleSystem>();
                
                if (targetParticleSystem == null && createParticleSystemIfMissing)
                {
                    GameObject particleObj = new GameObject("Particle System");
                    particleObj.transform.SetParent(transform);
                    particleObj.transform.localPosition = Vector3.zero;
                    targetParticleSystem = particleObj.AddComponent<ParticleSystem>();
                    LogDebug("Created ParticleSystem");
                }
                
                if (targetParticleSystem == null)
                {
                    LogWarning("No ParticleSystem found and createParticleSystemIfMissing is false");
                    return false;
                }
            }
            
            emissionModule = targetParticleSystem.emission;
            mainModule = targetParticleSystem.main;
            
            return true;
        }
        
        private IEnumerator DelayedAction()
        {
            yield return new WaitForSeconds(delayBeforeAction);
            ExecuteParticleAction();
        }
        
        private void ExecuteParticleAction()
        {
            switch (actionType)
            {
                case ParticleActionType.Play:
                    PlayParticles();
                    break;
                case ParticleActionType.Stop:
                    StopParticles();
                    break;
                case ParticleActionType.Pause:
                    PauseParticles();
                    break;
                case ParticleActionType.Resume:
                    ResumeParticles();
                    break;
                case ParticleActionType.Clear:
                    ClearParticles();
                    break;
                case ParticleActionType.Restart:
                    RestartParticles();
                    break;
                case ParticleActionType.SetEmissionRate:
                    SetEmissionRate();
                    break;
                case ParticleActionType.Burst:
                    CreateBurst();
                    break;
                case ParticleActionType.SetSpeed:
                    SetStartSpeed();
                    break;
                case ParticleActionType.SetLifetime:
                    SetStartLifetime();
                    break;
            }
        }
        
        private void PlayParticles()
        {
            if (withChildren)
            {
                targetParticleSystem.Play(true);
            }
            else
            {
                targetParticleSystem.Play(false);
            }
            
            LogDebug($"Started particle system: {targetParticleSystem.name}");
        }
        
        private void StopParticles()
        {
            if (stopAndClear)
            {
                if (withChildren)
                {
                    targetParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                else
                {
                    targetParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            else
            {
                if (withChildren)
                {
                    targetParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                else
                {
                    targetParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            
            LogDebug($"Stopped particle system: {targetParticleSystem.name}");
        }
        
        private void PauseParticles()
        {
            if (withChildren)
            {
                targetParticleSystem.Pause(true);
            }
            else
            {
                targetParticleSystem.Pause(false);
            }
            
            LogDebug($"Paused particle system: {targetParticleSystem.name}");
        }
        
        private void ResumeParticles()
        {
            if (withChildren)
            {
                targetParticleSystem.Play(true);
            }
            else
            {
                targetParticleSystem.Play(false);
            }
            
            LogDebug($"Resumed particle system: {targetParticleSystem.name}");
        }
        
        private void ClearParticles()
        {
            if (withChildren)
            {
                targetParticleSystem.Clear(true);
            }
            else
            {
                targetParticleSystem.Clear(false);
            }
            
            LogDebug($"Cleared particle system: {targetParticleSystem.name}");
        }
        
        private void RestartParticles()
        {
            StopParticles();
            ClearParticles();
            PlayParticles();
            
            LogDebug($"Restarted particle system: {targetParticleSystem.name}");
        }
        
        private void SetEmissionRate()
        {
            if (animateEmissionRate)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(AnimateEmissionRate());
            }
            else
            {
                emissionModule.rateOverTime = emissionRate;
                LogDebug($"Set emission rate to: {emissionRate}");
            }
        }
        
        private IEnumerator AnimateEmissionRate()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < emissionDuration)
            {
                float t = elapsedTime / emissionDuration;
                float currentRate = Mathf.Lerp(startEmissionRate, emissionRate, t);
                emissionModule.rateOverTime = currentRate;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            emissionModule.rateOverTime = emissionRate;
            animationCoroutine = null;
            
            LogDebug($"Animated emission rate from {startEmissionRate} to {emissionRate}");
        }
        
        private void CreateBurst()
        {
            ParticleSystem.Burst burst = new ParticleSystem.Burst(burstTime, burstCount);
            
            // Get existing bursts and add new one
            var bursts = new ParticleSystem.Burst[emissionModule.burstCount + 1];
            emissionModule.GetBursts(bursts);
            bursts[bursts.Length - 1] = burst;
            emissionModule.SetBursts(bursts);
            
            LogDebug($"Created burst: {burstCount} particles at time {burstTime}");
        }
        
        private void SetStartSpeed()
        {
            if (randomizeSpeed)
            {
                mainModule.startSpeed = new ParticleSystem.MinMaxCurve(speedRange.x, speedRange.y);
                LogDebug($"Set random start speed: {speedRange.x} - {speedRange.y}");
            }
            else
            {
                mainModule.startSpeed = startSpeed;
                LogDebug($"Set start speed to: {startSpeed}");
            }
        }
        
        private void SetStartLifetime()
        {
            if (randomizeLifetime)
            {
                mainModule.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
                LogDebug($"Set random start lifetime: {lifetimeRange.x} - {lifetimeRange.y}");
            }
            else
            {
                mainModule.startLifetime = startLifetime;
                LogDebug($"Set start lifetime to: {startLifetime}");
            }
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
        }
        
        #region Public API
        
        public void SetParticleSystem(ParticleSystem ps)
        {
            targetParticleSystem = ps;
            if (ps != null)
            {
                emissionModule = ps.emission;
                mainModule = ps.main;
            }
        }
        
        public void SetActionType(ParticleActionType type)
        {
            actionType = type;
        }
        
        public void SetEmissionRate(float rate)
        {
            emissionRate = Mathf.Max(0f, rate);
        }
        
        public void SetBurstCount(int count)
        {
            burstCount = Mathf.Max(0, count);
        }
        
        public void SetStartSpeed(float speed)
        {
            startSpeed = Mathf.Max(0f, speed);
        }
        
        public void SetStartLifetime(float lifetime)
        {
            startLifetime = Mathf.Max(0f, lifetime);
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            emissionRate = Mathf.Max(0f, emissionRate);
            emissionDuration = Mathf.Max(0.1f, emissionDuration);
            startEmissionRate = Mathf.Max(0f, startEmissionRate);
            burstCount = Mathf.Max(0, burstCount);
            burstTime = Mathf.Max(0f, burstTime);
            startSpeed = Mathf.Max(0f, startSpeed);
            startLifetime = Mathf.Max(0f, startLifetime);
            delayBeforeAction = Mathf.Max(0f, delayBeforeAction);
            
            // Ensure speed range is valid
            speedRange.x = Mathf.Max(0f, speedRange.x);
            speedRange.y = Mathf.Max(speedRange.x, speedRange.y);
            
            // Ensure lifetime range is valid
            lifetimeRange.x = Mathf.Max(0f, lifetimeRange.x);
            lifetimeRange.y = Mathf.Max(lifetimeRange.x, lifetimeRange.y);
        }
        #endif
    }
}