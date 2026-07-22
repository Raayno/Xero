using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    [RequireComponent(typeof(PlayerEffectManager))]
    [RequireComponent(typeof(PlayerCombatActivationManager))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
//         public bool BlockMovement = false;

        

//         [Space(10)]
//         [Tooltip("The height the player can jump")]
//         public float JumpHeight = 1.2f;


//         [Space(10)]
//         [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
//         public float JumpTimeout = 0.50f;


//         [Header("Player Grounded")]
//         [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
//         public bool Grounded = true;

//         // [Tooltip("Useful for rough ground")]
//         // public float GroundedOffset = -0.14f;

//         // [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
//         // public float GroundedRadius = 0.28f;

//         // [Tooltip("What layers the character uses as ground")]
//         // public LayerMask GroundLayers;

//         [Header("Landing Impact Runtime")]
//         [SerializeField] private bool debugLandingImpact = false;

//         [SerializeField] private PlayerAnimationManager _animationManager;


//         private bool isFalling = false;

//         private bool _wasGroundedLastFrame = true;
//         private bool _isTrackingFallDistance = false;
//         private float _fallStartHeight;
//         private float _lastFallDistance;



//         private float _jumpTimeoutDelta;
//         private float _fallTimeoutDelta;

// #if ENABLE_INPUT_SYSTEM
//         private PlayerInput _playerInput;
// #endif

//         private CharacterController _controller;
//         private StarterAssetsInputs _input;
//         private PlayerCombatActivationManager _combatActivationManager;
//         private GameObject _mainCamera;

//         public float LastFallDistance => _lastFallDistance;

//         public StarterAssetsInputs Input => _input;
//         public PlayerAnimationManager AnimationManager => _animationManager;

//         private bool IsCurrentDeviceMouse
//         {
//             get
//             {
// #if ENABLE_INPUT_SYSTEM
//                 return _playerInput.currentControlScheme == "KeyboardMouse";
// #else
//                 return false;
// #endif
//             }
//         }


//         private void Update()
//         {
//             JumpAndGravity();
//             GroundedCheck();
//             _combatActivationManager.UpdateAttackAnimationEndWait();
            
//             if (!isFalling && BlockMovement) return;
//             Move();
//         }

//         private void GroundedCheck()
//         {
//             // Vector3 spherePosition = new(
//             //     transform.position.x,
//             //     transform.position.y - GroundedOffset,
//             //     transform.position.z
//             // );

//             // Grounded = Physics.CheckSphere(
//             //     spherePosition,
//             //     GroundedRadius,
//             //     GroundLayers,
//             //     QueryTriggerInteraction.Ignore
//             // );

//             if (Grounded && !_wasGroundedLastFrame)
//             {
//                 CompleteFallDistanceTracking();
//             }

//             _wasGroundedLastFrame = Grounded;

//             _animationManager.SetGrounded(Grounded);
//         }

        

        



//         private void MoveVerticalOnlyWhileAttacking()
//         {

//             _controller.Move(
//                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
//             );

//             // Important:
//             // Do not set Animator Speed or MotionSpeed here.
//             // If Attack1/Attack2/Attack3 use Speed or MotionSpeed as Speed Multiplier,
//             // setting them to 0 freezes the attack animation at the first frame.
//         }

//         public void FallComplete()
//         {
//             isFalling = false;
//             _animationManager.SetJumpEnd(true);
//         }
    }
}