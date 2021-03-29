using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone
{
	/// <summary>
	/// Various states of our player
	/// </summary>
	[Flags]
	public enum PlayerState : byte
	{
		None = 0,
		Grounded = 1,
		Moving = 2,
		Fired = 4,
		Damaged = 8
	}

	/// <summary>
	/// Manages player input, movement and actions
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(CharacterRigidbody))]
	[RequireComponent(typeof(Material))]
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] float moveSpeed = 5f;
		[SerializeField] float jumpForce = 1f;
		[SerializeField] float jumpTimeFrame = 0.25f;
		[SerializeField] float gravityMultiplier = -3f;
		[SerializeField] float forceWhenDamaged = 5f;
		[SerializeField] float limitedInvulnerabilityTime = 1f;
		[SerializeField] float groundDistanceFactor = 0.01f;
		[SerializeField] LayerMask groundMask;
		[SerializeField] int maxJumps = 2;

		CharacterController controller = null;
		CharacterRigidbody physicsBody = null;
		Material[] playerMaterials = null;
		Color[] materialColors = null;
		Vector3 velocity = Vector3.zero;
		float movement = 0f;
		float groundDelta = 0f;
		float damagedDelta = 0f;
		int jumpCount = 0;
		PlayerState state = PlayerState.None;

		InputAction jumpAction = null;
		InputAction moveAction = null;

		/// <summary>
		/// Set up our input actions.
		/// </summary>
		private void Awake()
		{
			controller = GetComponent<CharacterController>();
			physicsBody = GetComponent<CharacterRigidbody>();

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

			jumpAction = GameManager.Input.actions.FindAction("Jump");
			moveAction = GameManager.Input.actions.FindAction("Move");
		}

		/// <summary>
		/// Apply any motion specified to the player.
		/// </summary>
		private void Update()
		{
			velocity = controller.velocity;

			if (state.HasFlag(PlayerState.Moving))
				controller.Move(Vector3.forward * movement * moveSpeed * Time.deltaTime);

			// Cast a ray to check if we're on ground, otherwise apply gravity.
			//Debug.DrawRay(transform.position, -Vector3.up * groundDistanceFactor, Color.green);
			if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, groundDistanceFactor, groundMask))
			{
				state |= PlayerState.Grounded;
				jumpCount = 0;
				groundDelta = 0f;
			}
			else
			{
				
				state &= ~PlayerState.Grounded;
				velocity.y += Physics.gravity.y * Time.deltaTime;
			}
			controller.Move(velocity * Time.deltaTime);

			if (!state.HasFlag(PlayerState.Grounded))
				groundDelta += Time.deltaTime;

			if (state.HasFlag(PlayerState.Damaged))
			{
				damagedDelta -= Time.deltaTime;
				if (damagedDelta <= 0f)
					state &= ~PlayerState.Damaged;
			}
		}

		/// <summary>
		/// Enable input actions.
		/// </summary>
		private void OnEnable()
		{
			jumpAction.performed += OnJump;
			moveAction.started += OnMoveStarted;
			moveAction.canceled += OnMoveStopped;
		}

		/// <summary>
		/// Disable input actions.
		/// </summary>
		private void OnDisable()
		{
			jumpAction.performed -= OnJump;
			moveAction.started -= OnMoveStarted;
			moveAction.canceled -= OnMoveStopped;
		}

		/// <summary>
		/// Called when the player touches something
		/// </summary>
		/// <param name="hit">What was hit</param>
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			// give the player some knockback and activate limited invulnerability
			if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("Hazard"))
			{
				if (!state.HasFlag(PlayerState.Damaged))
				{
					physicsBody.AddImpact(Vector3.Normalize(-transform.forward), forceWhenDamaged);
					state |= PlayerState.Damaged;
					damagedDelta = limitedInvulnerabilityTime;
					StartCoroutine(DamageEffect());
				}
			}
		}

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

		/// <summary>
		/// Called when the 'fire' button is pressed
		/// </summary>
		public void OnFire(InputAction.CallbackContext context)
		{
		}

		/// <summary>
		/// Called when the 'jump' button is pressed
		/// </summary>
		public void OnJump(InputAction.CallbackContext context)
		{
			if (!state.HasFlag(PlayerState.Grounded) && jumpCount < maxJumps)
			{
				controller.velocity.Set(controller.velocity.x, 0f, controller.velocity.z); // zero out velocity for double (triple?) jumps
				Jump();
			}

			if (state.HasFlag(PlayerState.Grounded))
				Jump();

			jumpCount++;
		}

		/// <summary>
		/// Apply velocity for a jump.
		/// </summary>
		void Jump()
		{
			velocity.y += Mathf.Sqrt(jumpForce * gravityMultiplier * Physics.gravity.y);
			controller.Move(velocity * Time.deltaTime);
		}

		/// <summary>
		/// Called when movement input is started
		/// </summary>
		public void OnMoveStarted(InputAction.CallbackContext context)
		{
			state |= PlayerState.Moving;

			var input = context.ReadValue<Vector2>();
			movement = input.x;
		}

		public void OnMoveStopped(InputAction.CallbackContext context) => state &= ~PlayerState.Moving;
	}
}