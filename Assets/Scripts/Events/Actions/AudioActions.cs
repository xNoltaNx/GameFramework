using System.Collections;
using UnityEngine;

namespace GameFramework.Events.Actions
{
    /// <summary>
    /// Action that plays audio clips with various configuration options.
    /// </summary>
    [AddComponentMenu("GameFramework/Events/Actions/Audio Action")]
    [ActionDefinition("audio-action", "🔊", "Audio Action", "Plays audio clips with configurable volume, pitch, and 3D settings", "Audio", 10)]
    public class AudioAction : BaseTriggerAction
    {
        [System.Serializable]
        public enum AudioActionType
        {
            PlayOneShot,
            Play,
            Stop,
            Pause,
            Resume,
            FadeIn,
            FadeOut
        }
        
        [Header("Audio Settings")]
        [SerializeField] private AudioActionType actionType = AudioActionType.PlayOneShot;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private bool createAudioSourceIfMissing = true;
        
        [Header("Playback Settings")]
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private bool randomizePitch = false;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private bool randomizeVolume = false;
        [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1f);
        
        [Header("3D Audio Settings")]
        [SerializeField] private bool use3D = false;
        [SerializeField] private Transform audioSourcePosition;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 500f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        
        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private bool stopAfterFadeOut = true;
        
        private float originalVolume;
        private Coroutine fadeCoroutine;
        
        protected override void PerformAction(GameObject context)
        {
            if (!SetupAudioSource())
            {
                return;
            }
            
            SetupAudioProperties();
            
            switch (actionType)
            {
                case AudioActionType.PlayOneShot:
                    PlayOneShot();
                    break;
                case AudioActionType.Play:
                    PlayAudio();
                    break;
                case AudioActionType.Stop:
                    StopAudio();
                    break;
                case AudioActionType.Pause:
                    PauseAudio();
                    break;
                case AudioActionType.Resume:
                    ResumeAudio();
                    break;
                case AudioActionType.FadeIn:
                    FadeIn();
                    break;
                case AudioActionType.FadeOut:
                    FadeOut();
                    break;
            }
        }
        
        private bool SetupAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                
                if (audioSource == null && createAudioSourceIfMissing)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    LogDebug("Created AudioSource component");
                }
                
                if (audioSource == null)
                {
                    LogWarning("No AudioSource found and createAudioSourceIfMissing is false");
                    return false;
                }
            }
            
            return true;
        }
        
        private void SetupAudioProperties()
        {
            if (audioSource == null) return;
            
            // Set basic properties
            float finalVolume = randomizeVolume ? Random.Range(volumeRange.x, volumeRange.y) : volume;
            float finalPitch = randomizePitch ? Random.Range(pitchRange.x, pitchRange.y) : pitch;
            
            audioSource.volume = finalVolume;
            audioSource.pitch = finalPitch;
            
            // Set 3D properties
            if (use3D)
            {
                audioSource.spatialBlend = 1f; // 3D
                audioSource.rolloffMode = rolloffMode;
                audioSource.minDistance = minDistance;
                audioSource.maxDistance = maxDistance;
                
                if (audioSourcePosition != null)
                {
                    audioSource.transform.position = audioSourcePosition.position;
                }
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D
            }
            
            // Set clip if specified
            if (audioClip != null && actionType != AudioActionType.PlayOneShot)
            {
                audioSource.clip = audioClip;
            }
            
            originalVolume = finalVolume;
        }
        
        private void PlayOneShot()
        {
            if (audioClip == null)
            {
                LogWarning("No audio clip specified for PlayOneShot");
                return;
            }
            
            audioSource.PlayOneShot(audioClip, audioSource.volume);
            LogDebug($"Played one shot: {audioClip.name}");
        }
        
        private void PlayAudio()
        {
            if (audioSource.clip == null)
            {
                LogWarning("No audio clip assigned to AudioSource");
                return;
            }
            
            audioSource.Play();
            LogDebug($"Started playing: {audioSource.clip.name}");
        }
        
        private void StopAudio()
        {
            audioSource.Stop();
            LogDebug("Stopped audio");
        }
        
        private void PauseAudio()
        {
            audioSource.Pause();
            LogDebug("Paused audio");
        }
        
        private void ResumeAudio()
        {
            audioSource.UnPause();
            LogDebug("Resumed audio");
        }
        
        private void FadeIn()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeCoroutine(0f, originalVolume, true));
        }
        
        private void FadeOut()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeCoroutine(audioSource.volume, 0f, false));
        }
        
        private IEnumerator FadeCoroutine(float startVolume, float endVolume, bool playAtStart)
        {
            audioSource.volume = startVolume;
            
            if (playAtStart && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                float t = elapsedTime / fadeDuration;
                audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            audioSource.volume = endVolume;
            
            if (!playAtStart && stopAfterFadeOut && endVolume <= 0f)
            {
                audioSource.Stop();
            }
            
            fadeCoroutine = null;
            
            LogDebug($"Fade completed: {startVolume:F2} -> {endVolume:F2}");
        }
        
        public override void Stop()
        {
            base.Stop();
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
        
        #region Public API
        
        public void SetAudioSource(AudioSource source)
        {
            audioSource = source;
        }
        
        public void SetAudioClip(AudioClip clip)
        {
            audioClip = clip;
        }
        
        public void SetVolume(float vol)
        {
            volume = Mathf.Clamp01(vol);
        }
        
        public void SetPitch(float p)
        {
            pitch = Mathf.Clamp(p, -3f, 3f);
        }
        
        public void SetActionType(AudioActionType type)
        {
            actionType = type;
        }
        
        public void SetFadeDuration(float duration)
        {
            fadeDuration = Mathf.Max(0.1f, duration);
        }
        
        #endregion
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            volume = Mathf.Clamp01(volume);
            pitch = Mathf.Clamp(pitch, -3f, 3f);
            fadeDuration = Mathf.Max(0.1f, fadeDuration);
            minDistance = Mathf.Max(0f, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
            
            // Ensure volume range is valid
            volumeRange.x = Mathf.Clamp01(volumeRange.x);
            volumeRange.y = Mathf.Clamp01(volumeRange.y);
            if (volumeRange.x > volumeRange.y)
            {
                volumeRange.y = volumeRange.x;
            }
            
            // Ensure pitch range is valid
            pitchRange.x = Mathf.Clamp(pitchRange.x, -3f, 3f);
            pitchRange.y = Mathf.Clamp(pitchRange.y, -3f, 3f);
            if (pitchRange.x > pitchRange.y)
            {
                pitchRange.y = pitchRange.x;
            }
        }
        #endif
    }
}