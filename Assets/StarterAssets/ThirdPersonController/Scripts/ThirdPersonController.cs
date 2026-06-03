using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using Vastav.Utils.Input;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioSource AudioFootsteps;
        public AudioSource LandingAudio;
        public AudioSource AudioFoley;
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Attack")]
        [SerializeField] private string[] attackAnimations = { "Attack1", "Attack2", "Attack3" };
        [SerializeField] private string idleBlendTreeStateName = "Idle Walk Run Blend";
        [SerializeField] private float attackCrossFadeDuration = 0.05f;
        [SerializeField] private float idleCrossFadeDuration = 0.1f;

        [Header("Attack Debug")]
        [SerializeField] private bool debugAttack = true;

        private bool isAttacking = false;
        private bool isComboWindowOpen = false;
        private bool isNextAttackQueued = false;
        private int attackAnimationIndex = 0;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem_PlayerActionsSO.OnAttackEvent += InputSystem_PlayerActionsSO_OnAttackEvent;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void InputSystem_PlayerActionsSO_OnAttackEvent(InputAction.CallbackContext obj)
        {
            if (!obj.performed)
                return;

            Attack();
        }
#endif

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem_PlayerActionsSO.OnAttackEvent -= InputSystem_PlayerActionsSO_OnAttackEvent;
#endif
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (isAttacking)
                return;

            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private void Move()
        {
            if (isAttacking)
            {
                MoveVerticalOnlyWhileAttacking();
                return;
            }

            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(
                _controller.velocity.x,
                0.0f,
                _controller.velocity.z
            ).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * SpeedChangeRate
            );

            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(
                _input.move.x,
                0.0f,
                _input.move.y
            ).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime
                );

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(
                0.0f,
                _targetRotation,
                0.0f
            ) * Vector3.forward;

            _controller.Move(
                targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void MoveVerticalOnlyWhileAttacking()
        {
            _speed = 0f;
            _animationBlend = 0f;

            _controller.Move(
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );

            // Important:
            // Do not set Animator Speed or MotionSpeed here.
            // If Attack1/Attack2/Attack3 use Speed or MotionSpeed as Speed Multiplier,
            // setting them to 0 freezes the attack animation at the first frame.
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (!isAttacking && _input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void Attack()
        {
            if (!_hasAnimator)
                return;

            if (attackAnimations == null || attackAnimations.Length == 0)
                return;

            if (_input.jump)
                return;

            if (!isAttacking)
            {
                attackAnimationIndex = 0;
                PlayCurrentAttackAnimation();
                return;
            }

            if (!isComboWindowOpen)
                return;

            if (isNextAttackQueued)
                return;

            int nextAttackIndex = attackAnimationIndex + 1;

            if (nextAttackIndex >= attackAnimations.Length)
                return;

            isNextAttackQueued = true;

            if (debugAttack)
            {
                Debug.Log($"<color=yellow>[Attack]</color> Queued next attack: {attackAnimations[nextAttackIndex]}");
            }
        }

        private void PlayCurrentAttackAnimation()
        {
            isAttacking = true;
            isComboWindowOpen = false;
            isNextAttackQueued = false;

            _input.jump = false;
            _speed = 0f;
            _animationBlend = 0f;

            string attackStateName = attackAnimations[attackAnimationIndex];

            if (debugAttack)
            {
                Debug.Log($"<color=orange>[Attack]</color> Playing: {attackStateName}");
            }

            _animator.speed = 1f;

            _animator.CrossFadeInFixedTime(
                attackStateName,
                attackCrossFadeDuration,
                0,
                0f
            );
        }

        public void Attacking()
        {
            isAttacking = true;
        }

        public void AttackFinish()
        {
            if (debugAttack)
            {
                Debug.Log($"<color=green>[Attack]</color> AttackFinish called on index: {attackAnimationIndex}");
            }

            if (isNextAttackQueued)
            {
                attackAnimationIndex++;
                PlayCurrentAttackAnimation();
                return;
            }

            ResetAttack();
        }

        private void ResetAttack()
        {
            if (debugAttack)
            {
                Debug.Log("<color=cyan>[Attack]</color> Combo finished.");
            }

            isAttacking = false;
            isComboWindowOpen = false;
            isNextAttackQueued = false;
            attackAnimationIndex = 0;

            _animator.speed = 1f;

            _animator.CrossFadeInFixedTime(
                idleBlendTreeStateName,
                idleCrossFadeDuration,
                0,
                0f
            );
        }

        public void SetComboWindowOpen()
        {
            if (!isAttacking)
                return;

            isComboWindowOpen = true;

            if (debugAttack)
            {
                Debug.Log($"<color=lime>[Attack]</color> Combo window OPEN for: {attackAnimations[attackAnimationIndex]}");
            }
        }

        public void SetComboWindowClose()
        {
            isComboWindowOpen = false;

            if (debugAttack)
            {
                Debug.Log($"<color=red>[Attack]</color> Combo window CLOSE for: {attackAnimations[attackAnimationIndex]}");
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;

            if (lfAngle > 360f)
                lfAngle -= 360f;

            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (AudioFootsteps != null)
                    AudioFootsteps.Play();

                if (AudioFoley != null)
                    AudioFoley.Play();
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudio != null)
                    LandingAudio.Play();
            }
        }
    }
}