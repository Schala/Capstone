using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone.Player
{
	/// Various states of our player
	[Flags]
	public enum PlayerState : byte
	{
		None = 0,
		Grounded = 1,
		Moving = 2,
		Fired = 4,
		Damaged = 8,
		HitWall = 16
	}

	[Flags]
	public enum PlayerFlags : byte
	{
		None = 0,
		ShouldStep = 1,
		InSpline = 2
	}

	/// Manages player input, movement and actions
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Material))]
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] PlayerFlags flags = PlayerFlags.ShouldStep;

		[Header("Movement")]
		[SerializeField] float moveSpeed = 25f;
		[SerializeField] float airborneMovementDilusion = 0.25f;
		[SerializeField] float fallDepth = -2f;
		[SerializeField] Transform arenaCenter = null;

		[Header("Stepping")]
		[SerializeField] Transform stepUpper = null;
		[SerializeField] Transform stepLower = null;
		[SerializeField] float stepLowerRayCastDistance = 0.1f;
		[SerializeField] float stepUpperRayCastDistance = 0.2f;
		[SerializeField] float stepSmooth = 2f;

		[Header("Jumping")]
		[SerializeField] float jumpForce = 5f;
		[SerializeField] int maxJumps = 2;
		[SerializeField] float collisionRadiusPadding = 0.1f;
		[SerializeField] Transform center = null;
		[SerializeField] float wallRayCheckPadding = 0.25f;

		[Header("Attacking")]
		[SerializeField] GameObject weapon = null;
		[SerializeField] float weaponActiveTime = 0.5f;

		[Header("Interaction")]
		[SerializeField] float forceWhenDamaged = 5f;
		[SerializeField] float limitedInvulnerabilityTime = 1f;

		public float Movement { get; set; } = 0f;

		Rigidbody physicsBody = null;
		Material[] playerMaterials = null;
		Color[] materialColors = null;
		Vector3 lastGroundPosition = Vector3.zero;
		Vector3 splinePoint = Vector3.zero;
		float damagedDelta = 0f;
		int jumpCount = 0;
		PlayerState state = PlayerState.None;
		PlayerInput playerInput = null;
		CapsuleCollider playerCollider = null;
		PlayerWeapon weaponBehavior = null;
		LayerMask groundMask;

		InputAction jumpAction = null;
		InputAction moveAction = null;
		InputAction fireAction = null;

		public Vector3 SplinePoint
		{
			get => IsInSpline ? splinePoint : Vector3.zero;
			set => splinePoint = value;
		}

		/// Set up our input actions and gather material info.
		private void Awake()
		{
			physicsBody = GetComponent<Rigidbody>();
			playerCollider = GetComponent<CapsuleCollider>();

			playerInput = FindObjectOfType<GameManager>().GetComponent<PlayerInput>();
			groundMask = LayerMask.GetMask("Ground");
			jumpAction = playerInput.actions.FindAction("Jump");
			moveAction = playerInput.actions.FindAction("Move");
			fireAction = playerInput.actions.FindAction("Fire");

			var renderers = GetComponentsInChildren<Renderer>();
			var playerMaterialsList = new List<Material>();
			var materialColorsList = new List<Color>();
			for (int i = 0; i < renderers.Length; i++)
			{
				for (int j = 0; j < renderers[i].materials.Length; j++)
				{
					playerMaterialsList.Add(renderers[i].materials[j]);
					materialColorsList.Add(renderers[i].materials[j].color);
				}
			}
			playerMaterials = playerMaterialsList.ToArray();
			materialColors = materialColorsList.ToArray();

			weaponBehavior = weapon.GetComponent<PlayerWeapon>();
		}

		/// Apply any motion specified to the player.
		private void FixedUpdate()
		{
			if (!state.HasFlag(PlayerState.Moving)) return;
			if (state.HasFlag(PlayerState.HitWall)) return;

			if (ShouldStep) Climb();

			if (state.HasFlag(PlayerState.Grounded))
				physicsBody.AddForce(Movement * transform.forward * moveSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
			else
				physicsBody.AddForce(Movement * transform.forward * moveSpeed * airborneMovementDilusion * Time.fixedDeltaTime, ForceMode.Impulse);
		}

		/// Run the damage logic for our player if not already doing so.
		public void Damage()
		{
			if (state.HasFlag(PlayerState.Damaged)) return;
			Injured();
		}

		/// <summary>
		/// Injury logic
		/// </summary>
		void Injured()
		{
			state |= PlayerState.Damaged;
			damagedDelta = limitedInvulnerabilityTime;
			StartCoroutine(DamageEffect());
		}

		/// Progress our timers, check if we're on the ground, and respawn us at the last grounded position if we fall.
		private void Update()
		{
			// if we're grounded
			if (Physics.CheckSphere(transform.position, playerCollider.radius - collisionRadiusPadding, groundMask))
			{
				state |= PlayerState.Grounded;
				jumpCount = 0;
				lastGroundPosition = transform.position - transform.forward;
			}
			else
				state &= ~PlayerState.Grounded;

			// if we fell
			if (transform.position.y <= fallDepth)
			{
				transform.position = lastGroundPosition;
				physicsBody.velocity = Vector3.zero;
				Injured();
			}

			// if we hit a wall
			if (Physics.Raycast(center.position, center.forward, playerCollider.radius / 2f + wallRayCheckPadding, groundMask))
				state |= PlayerState.HitWall;
			else
				state &= ~PlayerState.HitWall;

			// if we're injured
			if (state.HasFlag(PlayerState.Damaged))
			{
				damagedDelta -= Time.deltaTime;
				if (damagedDelta <= 0f)
					state &= ~PlayerState.Damaged;
			}
		}

		/*private void OnDrawGizmos()
		{
			if (playerCollider == null) return;

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, playerCollider.radius - collisionRadiusPadding);
		}*/

		/// Enable input actions.
		private void OnEnable()
		{
			jumpAction.performed += OnJump;
			moveAction.started += OnMoveStarted;
			moveAction.canceled += OnMoveStopped;
			fireAction.performed += OnFire;
		}

		/// Disable input actions.
		private void OnDisable()
		{
			if (jumpAction != null) jumpAction.performed -= OnJump;
			if (fireAction != null) fireAction.performed -= OnFire;

			if (moveAction != null)
			{
				moveAction.started -= OnMoveStarted;
				moveAction.canceled -= OnMoveStopped;
			}
		}

		/// Move our rigidbody on various elevated ground depending on raycast validation.
		/// First check our lower step. If it hits something, check the upper one.
		/// If the upper one does NOT hit something, that's something the player can climb.
		void Climb()
		{
			// Forward and back
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(Vector3.forward), stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(Vector3.forward), stepUpperRayCastDistance))
					physicsBody.position -= Vector3.up * -stepSmooth * Time.deltaTime;
			}

			// Left by 45 degrees
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(1.5f, 0f, 1f), stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(1.5f, 0f, 1f), stepUpperRayCastDistance))
					physicsBody.position -= Vector3.up * -stepSmooth * Time.deltaTime;
			}

			// Right by 45 degrees
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(-1.5f, 0, 1f), stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(-1.5f, 0f, 1f), stepUpperRayCastDistance))
					physicsBody.position -= Vector3.up * -stepSmooth * Time.deltaTime;
			}
		}

		/// Play the visual feedback for damage.
		IEnumerator DamageEffect()
		{
			while (damagedDelta > 0f)
			{
				for (int i = 0; i < playerMaterials.Length; i++)
				{
					materialColors[i].a = 0f;
					playerMaterials[i].color = materialColors[i];
				}
				yield return new WaitForSeconds(0.1f);

				for (int i = 0; i < playerMaterials.Length; i++)
				{
					materialColors[i].a = 1f;
					playerMaterials[i].color = materialColors[i];
				}
				yield return new WaitForSeconds(0.1f);
			}
		}

		/// Called when the 'fire' button is pressed
		public void OnFire(InputAction.CallbackContext context)
		{
			if (weapon.activeInHierarchy)
				weaponBehavior.Refresh();
			else
				weapon.SetActive(true);
		}

		/// Called when the 'jump' button is pressed
		public void OnJump(InputAction.CallbackContext context)
		{
			if (++jumpCount >= maxJumps) return;

			if (IsGrounded)
				Jump();
			else
			{
				physicsBody.velocity.Set(physicsBody.velocity.x, 0f, physicsBody.velocity.z); // zero out velocity for double (triple?) jumps
				Jump();
			}
		}

		/// Apply force for a jump. This is public so we can access it from machine learning code.
		public void Jump() => physicsBody.velocity += Vector3.up * jumpForce;

		/// Called when movement input is started
		public void OnMoveStarted(InputAction.CallbackContext context)
		{
			IsMoving = true;

			var input = context.ReadValue<Vector2>();
			Movement = input.x;

			if (Movement != 0f) transform.rotation = Quaternion.LookRotation(Movement * Vector3.forward);
		}

		/// Called when move input stops.
		public void OnMoveStopped(InputAction.CallbackContext context) => IsMoving = false;

		/// <summary>
		/// Should the player step up stairs and over small obstacles? This should be turned off when on inclines and swinging platforms.
		/// </summary>
		public bool ShouldStep
		{
			get => flags.HasFlag(PlayerFlags.ShouldStep);
			set
			{
				if (value)
					flags |= PlayerFlags.ShouldStep;
				else
					flags &= ~PlayerFlags.ShouldStep;
			}
		}

		/// <summary>
		/// Set if we're moving or not.
		/// </summary>
		public bool IsMoving
		{
			get => state.HasFlag(PlayerState.Moving);
			set
			{
				if (value)
					state |= PlayerState.Moving;
				else
					state &= ~PlayerState.Moving;
			}
		}

		/// <summary>
		/// Set if we're on the ground or not.
		/// </summary>
		public bool IsGrounded
		{
			get => state.HasFlag(PlayerState.Grounded);
			set
			{
				if (value)
					state |= PlayerState.Grounded;
				else
					state &= ~PlayerState.Grounded;
			}
		}

		public bool IsInSpline
		{
			get => flags.HasFlag(PlayerFlags.InSpline);
			set
			{
				if (value)
					flags |= PlayerFlags.InSpline;
				else
					flags &= ~PlayerFlags.InSpline;
			}
		}
	}
}
