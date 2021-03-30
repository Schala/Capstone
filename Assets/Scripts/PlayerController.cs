using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone
{
	/// Various states of our player
	[Flags]
	public enum PlayerState : byte
	{
		None = 0,
		Grounded = 1,
		Moving = 2,
		Fired = 4,
		Damaged = 8
	}

	/// Manages player input, movement and actions
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Material))]
	public class PlayerController : MonoBehaviour
	{
		[Header("Movement")]
		[SerializeField] float moveSpeed = 25f;
		[SerializeField] float airborneMovementDilusion = 0.25f;

		[Header("Stepping")]
		[SerializeField] Transform stepUpper = null;
		[SerializeField] Transform stepLower = null;
		[SerializeField] float stepLowerRayCastDistance = 0.1f;
		[SerializeField] float stepUpperRayCastDistance = 0.2f;
		[SerializeField] float stepSmooth = 0.1f;

		[Header("Jumping")]
		[SerializeField] float jumpForce = 5f;
		[SerializeField] float jumpTimeFrame = 0.25f;
		[SerializeField] int maxJumps = 2;

		[Header("Interaction")]
		[SerializeField] float forceWhenDamaged = 5f;
		[SerializeField] float limitedInvulnerabilityTime = 1f;

		Rigidbody physicsBody = null;
		Material[] playerMaterials = null;
		Color[] materialColors = null;
		float movement = 0f;
		float groundDelta = 0f;
		float damagedDelta = 0f;
		int jumpCount = 0;
		PlayerState state = PlayerState.None;
		PlayerInput playerInput = null;

		InputAction jumpAction = null;
		InputAction moveAction = null;

		/// Set up our input actions and gather material info.
		private void Awake()
		{
			physicsBody = GetComponent<Rigidbody>();

			playerInput = FindObjectOfType<GameManager>().GetComponent<PlayerInput>();
			jumpAction = playerInput.actions.FindAction("Jump");
			moveAction = playerInput.actions.FindAction("Move");

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
		}

		/// Apply any motion specified to the player.
		private void FixedUpdate()
		{
			if (state.HasFlag(PlayerState.Moving))
			{
				if (state.HasFlag(PlayerState.Grounded))
					physicsBody.AddForce(Vector3.forward * movement * moveSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
				else
					physicsBody.AddForce(Vector3.forward * movement * moveSpeed * airborneMovementDilusion * Time.fixedDeltaTime, ForceMode.Impulse);
			}

			Climb();
		}

		/// Check to see if we're on the ground.
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Ground"))
			{
				state |= PlayerState.Grounded;
				jumpCount = 0;
				groundDelta = 0f;
			}
		}

		/// Chewck to see if we're off the ground.
		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Ground"))
				state &= ~PlayerState.Grounded;
		}

		/// Progress our timers.
		private void Update()
		{
			if (!state.HasFlag(PlayerState.Grounded))
				groundDelta += Time.deltaTime;

			if (state.HasFlag(PlayerState.Damaged))
			{
				damagedDelta -= Time.deltaTime;
				if (damagedDelta <= 0f)
					state &= ~PlayerState.Damaged;
			}
		}

		/// Enable input actions.
		private void OnEnable()
		{
			jumpAction.performed += OnJump;
			moveAction.started += OnMoveStarted;
			moveAction.canceled += OnMoveStopped;
		}

		/// Disable input actions.
		private void OnDisable()
		{
			if (jumpAction != null) jumpAction.performed -= OnJump;

			if (moveAction != null)
			{
				moveAction.started -= OnMoveStarted;
				moveAction.canceled -= OnMoveStopped;
			}
		}

		/// Move our rigidbody on various elevated ground depending on raycast validation.
		void Climb()
		{
			// Forward and back
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(Vector3.forward), out _, stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(Vector3.forward), out _, stepUpperRayCastDistance))
				{
					physicsBody.position -= Vector3.up * -stepSmooth;
				}
			}

			// Left
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(1.5f, 0f, 1f), out _, stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(1.5f, 0f, 1f), out _, stepUpperRayCastDistance))
				{
					physicsBody.position -= Vector3.up * -stepSmooth;
				}
			}

			// Right
			if (Physics.Raycast(stepLower.position, transform.TransformDirection(-1.5f, 0, 1f), out _, stepLowerRayCastDistance))
			{
				if (!Physics.Raycast(stepUpper.position, transform.TransformDirection(-1.5f, 0f, 1f), out _, stepUpperRayCastDistance))
				{
					physicsBody.position -= Vector3.up * -stepSmooth;
				}
			}
		}

		/// Called when the player touches something
		private void OnCollisionEnter(Collision collision)
		{
			// give the player some knockback and activate limited invulnerability
			if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hazard"))
			{
				if (!state.HasFlag(PlayerState.Damaged))
				{
					physicsBody.AddForce(Vector3.Normalize(-transform.forward) * forceWhenDamaged, ForceMode.Impulse);
					state |= PlayerState.Damaged;
					damagedDelta = limitedInvulnerabilityTime;
					StartCoroutine(DamageEffect());
				}
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
		}

		/// Called when the 'jump' button is pressed
		public void OnJump(InputAction.CallbackContext context)
		{
			if (!state.HasFlag(PlayerState.Grounded) && jumpCount < maxJumps)
			{
				physicsBody.velocity.Set(physicsBody.velocity.x, 0f, physicsBody.velocity.z); // // zero out velocity for double (triple?) jumps
				Jump();
			}

			if (state.HasFlag(PlayerState.Grounded))
				Jump();

			jumpCount++;
		}

		/// Apply force for a jump.
		void Jump() => physicsBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

		/// Called when movement input is started
		public void OnMoveStarted(InputAction.CallbackContext context)
		{
			state |= PlayerState.Moving;

			var input = context.ReadValue<Vector2>();
			movement = input.x;
		}

		/// Called when move input stops.
		public void OnMoveStopped(InputAction.CallbackContext context) => state &= ~PlayerState.Moving;
	}
}
