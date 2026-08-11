using MoreMountains.Feedbacks;
using System;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// Plays a Wwise Event.
    /// If TargetObject is left empty, the MMF_Player's GameObject is used.
    /// </summary>
    [Serializable]
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Wwise Event")]
    [FeedbackHelp("Plays a Wwise Event on a target GameObject. If no target is specified, the MMF_Player's GameObject will be used.")]
    public class MMF_WwiseEvent : MMF_Feedback
    {
        /// <summary>
        /// Used to enable/disable all feedbacks of this type at once.
        /// </summary>
        public static bool FeedbackTypeAuthorized = true;

#if UNITY_EDITOR
        /// <summary>
        /// Inspector color.
        /// </summary>
        public override Color FeedbackColor => MMFeedbacksInspectorColors.SoundsColor;

        public override bool HasCustomInspectors => true;

        public override bool EvaluateRequiresSetup()
        {
            return Event == null;
        }

        public override string RequiredTargetText
        {
            get
            {
                return Event != null ? Event.Name : string.Empty;
            }
        }

        public override string RequiresSetupText
        {
            get
            {
                return "Please assign a Wwise Event.";
            }
        }
#endif

        /// <summary>
        /// Returns the duration reported to MMFeedbacks.
        /// Set this manually for voice/dialogue events.
        /// </summary>
        public override float FeedbackDuration => EventDuration;

        [MMFInspectorGroup("Wwise", true, 10)]

        [Tooltip("The Wwise Event to post.")]
        public AK.Wwise.Event Event;

        [Tooltip("Optional. If left empty, the MMF_Player's GameObject will be used.")]
        public GameObject TargetObject;

        [Tooltip("Duration of this event in seconds. Used by MMFeedbacks.")]
        [Min(0f)]
        public float EventDuration = 0f;

        [Tooltip("Stops the event when the feedback stops.")]
        public bool StopEventOnFeedbackStop = false;

        public MMF_Button TestPlayButton;

        protected GameObject _resolvedTarget;

        public override void InitializeCustomAttributes()
        {
            base.InitializeCustomAttributes();
            TestPlayButton = new MMF_Button("Debug Play Event", TestPlayEvent);
        }

        /// <summary>
        /// Cache the target once.
        /// </summary>
        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);

            _resolvedTarget = TargetObject != null
                ? TargetObject
                : owner.gameObject;
        }

        /// <summary>
        /// Plays the Wwise event.
        /// </summary>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            if (Event == null)
            {
                return;
            }

            Event.Post(_resolvedTarget);
        }

        /// <summary>
        /// Stops the Wwise event if requested.
        /// </summary>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            if (!StopEventOnFeedbackStop || Event == null)
            {
                return;
            }

            Event.Stop(_resolvedTarget);
        }

        /// <summary>
        /// Plays the event from the inspector while in Play Mode.
        /// </summary>
        protected virtual void TestPlayEvent()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Wwise Events can only be tested while the game is running.");
                return;
            }

            if (Event == null)
            {
                Debug.LogWarning("No Wwise Event assigned.");
                return;
            }

            Event.Post(_resolvedTarget);
#endif
        }
    }
}
