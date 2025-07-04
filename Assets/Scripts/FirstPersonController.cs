﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerInput))]
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		public float MoveSpeed = 8.0f;
		public float SprintSpeed = 16.0f;
		public float RotationSpeed = 1.0f;
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		public float JumpHeight = 1.2f;
		public float Gravity = -15.0f;
        public float JumpMultiplier = 1f;

        [Space(10)]
		public float JumpTimeout = 0.1f;
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		public bool Grounded = true;
		public float GroundedOffset = -0.14f;
		public float GroundedRadius = 0.5f;
		public LayerMask GroundLayers;

        public float ClimbSpeed = 5f;
        public int ExtraJumpCount = 1;


        [Header("Cinemachine")]
		public GameObject CinemachineCameraTarget;
		public float TopClamp = 90.0f;
		public float BottomClamp = -90.0f;

		[Header("Respawn")]
		public float FallThreshold = -20f;
		public Vector3 RespawnPosition;

		private float _cinemachineTargetPitch;
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

        private bool _onLadder = false;
        private Vector3 _ladderDirection = Vector3.up;

        private CharacterController _controller;
		private StarterAssetsInputs _input;
		private PlayerInput _playerInput;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private int _jumpCount = 0;

		private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

		private void Awake()
		{
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
			CheckFallRespawn();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			if (_input.look.sqrMagnitude >= _threshold)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			if (_onLadder)
			{
				float vertical = _input.move.y; // W/S or up/down
				Vector3 climbMovement = _ladderDirection * vertical * ClimbSpeed * Time.deltaTime;
				_controller.Move(climbMovement);
			}
			else
			{
				float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
				if (_input.move == Vector2.zero) targetSpeed = 0.0f;

				float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
				float speedOffset = 0.1f;
				float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

				if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
				{
					_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
					_speed = Mathf.Round(_speed * 1000f) / 1000f;
				}
				else
				{
					_speed = targetSpeed;
				}

				Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
				if (_input.move != Vector2.zero)
				{
					inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
				}

				_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			}
		}

		private void JumpAndGravity()
		{
            if (_onLadder)
            {
                if (_onLadder)
                {
                    if (_input.jump)
                    {
                        _onLadder = false;
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                        Vector3 pushBack = -transform.forward * 0.05f;
                        _controller.Move(pushBack); // move away from ladder
                        _input.jump = false;
                    }
                    else
                    {
                        _verticalVelocity = 0f;
                    }
                    return;
                }

                // Disable gravity
                _verticalVelocity = 0f;
                return;
            }

            if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
				_jumpCount = 0;
				
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					_verticalVelocity = Mathf.Sqrt(JumpHeight * JumpMultiplier * -2f * Gravity);
					_jumpCount++;
					_input.jump = false;
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
				if (_input.jump && _jumpCount < ExtraJumpCount)
				{
					_verticalVelocity = Mathf.Sqrt(JumpHeight * JumpMultiplier * -2f * Gravity);
					_jumpCount++;
                    _input.jump = false;
				}
			}

			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

        public void EnterLadder(Vector3 climbDirection)
        {
            _onLadder = true;
            _ladderDirection = climbDirection;
            _verticalVelocity = 0f; // stop falling
        }

        public void ExitLadder()
        {
            _onLadder = false;
        }


        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
			Gizmos.color = Grounded ? transparentGreen : transparentRed;
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		private void CheckFallRespawn()
		{
			if (transform.position.y < FallThreshold)
			{
				_verticalVelocity = 0f;
				_controller.enabled = false;
				transform.position = RespawnPosition;
				_controller.enabled = true;
			}
		}
	}
}
