using MoreMountains.Feedbacks;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerEffectManager : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource AudioFootsteps;
        public AudioSource LandingAudio;
        public AudioSource AudioFoley;
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Tooltip("X = minimum pitch randomization, Y = maximum pitch randomization.")]
        [SerializeField] private Vector2 footstepPitchRandomRange = new Vector2(0.95f, 1.05f);

        [Header("Landing Impact")]
        [Tooltip("X = fall distance for minimum impact, Y = fall distance for maximum impact.")]
        [SerializeField] private Vector2 fallDistanceImpactRange = new Vector2(1f, 10f);

        [Tooltip("X = minimum feedback impact, Y = maximum feedback impact.")]
        [SerializeField] private Vector2 feedbackImpactRange = new Vector2(0.25f, 1f);

        [SerializeField] private bool debugLandingImpact = false;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player jumpLandFeedback;
        [SerializeField] private MMF_Player attackFeedback;
        [SerializeField] private MMF_Player attackHitFeedback;

        private ThirdPersonController _thirdPersonController;

        private void Awake()
        {
            _thirdPersonController = GetComponent<ThirdPersonController>();
        }

        public void PlayAttackFeedback()
        {
            attackFeedback?.PlayFeedbacks();
        }

        public void PlayLandEmpact()
        {
            float landingImpact = CalculateLandingImpact();

            if (debugLandingImpact)
            {
                Debug.Log($"<color=cyan>[Landing Impact]</color> Fall Distance: {_thirdPersonController.LastFallDistance}, Feedback Impact: {landingImpact}");
            }

            jumpLandFeedback?.PlayFeedbacks(transform.position, landingImpact);
            AkSoundEngine.PostEvent("Play_Plyr_Land", gameObject);
        }

        private float CalculateLandingImpact()
        {
            if (_thirdPersonController == null)
                return feedbackImpactRange.x;

            float minimumDistance = Mathf.Min(fallDistanceImpactRange.x, fallDistanceImpactRange.y);
            float maximumDistance = Mathf.Max(fallDistanceImpactRange.x, fallDistanceImpactRange.y);

            float minimumImpact = Mathf.Min(feedbackImpactRange.x, feedbackImpactRange.y);
            float maximumImpact = Mathf.Max(feedbackImpactRange.x, feedbackImpactRange.y);

            if (Mathf.Approximately(minimumDistance, maximumDistance))
                return maximumImpact;

            float normalizedImpact = Mathf.InverseLerp(
                minimumDistance,
                maximumDistance,
                _thirdPersonController.LastFallDistance
            );

            return Mathf.Lerp(
                minimumImpact,
                maximumImpact,
                normalizedImpact
            );
        }

        public void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f)
                return;

            PlayRandomFootstepSound();
        }

        private void PlayRandomFootstepSound()
        {
            AkSoundEngine.PostEvent("Play_Plyr_Footsteps", gameObject);
            //if (AudioFootsteps == null)
            //    return;

            //float minimumPitch = Mathf.Min(footstepPitchRandomRange.x, footstepPitchRandomRange.y);
            //float maximumPitch = Mathf.Max(footstepPitchRandomRange.x, footstepPitchRandomRange.y);

            //AudioFootsteps.pitch = Random.Range(minimumPitch, maximumPitch);

            //if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
            //{
            //    int randomFootstepIndex = Random.Range(0, FootstepAudioClips.Length);
            //    AudioClip randomFootstepClip = FootstepAudioClips[randomFootstepIndex];

            //    if (randomFootstepClip != null)
            //    {
            //        AudioFootsteps.PlayOneShot(randomFootstepClip, FootstepAudioVolume);
            //    }

            //    return;
            //}

            //AudioFootsteps.Play();
        }

        public void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudio != null)
                    LandingAudio.Play();
            }
        }

        public void PlayAttackHitFeedback()
        {
            attackHitFeedback?.PlayFeedbacks();
        }
    }
}