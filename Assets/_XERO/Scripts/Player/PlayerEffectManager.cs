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

        

        [SerializeField] private bool debugLandingImpact = false;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player landFeedback;
        [SerializeField] private MMF_Player attackFeedback;
        [SerializeField] private MMF_Player attackHitFeedback;

        public void PlayAttackFeedback()
        {
            attackFeedback?.PlayFeedbacks();
        }

        public void PlayLandEmpact(float landingImpact)
        {
            landFeedback?.PlayFeedbacks(transform.position, landingImpact);
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