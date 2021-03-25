using System;
using System.Collections;
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
		Jumped = 4,
		Fired = 8,
		Damaged = 16
	}

	/// <summary>
	/// Manages player input, movement and actions
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(CharacterRigidbody))]
	[RequireComponent(typeof(Material))]
	public class PlayerController : MonoBehaviour, InputActions.IPlayerActions
	{
		[SerializeField] float moveSpeed = 5f;
		[SerializeField] float jumpForce = 1f;
		[SerializeField] float jumpTimeFrame = 0.25f;
		[SerializeField] float gravityMultiplier = -3f;
		[SerializeField] float forceWhenDamaged = 5f;
		[SerializeField] float limitedInvulnerabilityTime = 1f;
		[SerializeField] int maxJumps = 2;

		CharacterController controller = null;
		CharacterRigidbody physicsBody = null;
		Material playerMaterial = null;
		InputActions inputActions = null;
		Color materialColor;
		Vector3 velocity = Vector3.zero;
		float movement = 0f;
		float groundDelta = 0f;
		float damagedDelta = 0f;
		int jumpCount = 0;
		PlayerState state = PlayerState.None;

		/// <summary>
		/// Set up our input actions.
		/// </summary>
		private void Awake()
		{
			controller = GetComponent<CharacterController>();
			physicsBody = GetComponent<CharacterRigidbody>();
			playerMaterial = GetComponentInChildren<Renderer>().material;
			materialColor = playerMaterial.color;

			inputActions = new InputActions();
			inputActions.Player.SetCallbacks(this);
		}

		/// <summary>
		/// Apply any motion specified to the player.
		/// </summary>
		private void Update()
		{
			velocity = controller.velocity;

			if (state.HasFlag(PlayerState.Moving))
				controller.Move(Vector3.forward * movement * moveSpeed * Time.deltaTime);

			if (!state.HasFlag(PlayerState.Grounded))
				velocity.y += Physics.gravity.y * Time.deltaTime;
			controller.Move(velocity * Time.deltaTime); // apply gravity

			if (state.HasFlag(PlayerState.Jumped))
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
		private void OnEnable() => inputActions.Enable();

		/// <summary>
		/// Disable input actions.
		/// </summary>
		private void OnDisable() => inputActions.Disable();

		/// <summary>
		/// Called when the player touches something
		/// </summary>
		/// <param name="hit">What was hit</param>
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (hit.gameObject.CompareTag("Ground")) // reset jump conditions
			{
				state |= PlayerState.Grounded;
				state &= ~PlayerState.Jumped;
				jumpCount = 0;
				groundDelta = 0f;
			}
			else
				state &= ~PlayerState.Grounded;

			// give the player some knockback and activate limited invulnerability
			if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("Hazard"))
			{
				if (!state.HasFlag(PlayerState.Damaged))
				{
					physicsBody.AddImpact(Vector3.Normalize(-transform.forward), forceWhenDamaged);
					state |= PlayerState.Damaged;
					state &= ~PlayerState.Grounded;
					damagedDelta = limitedInvulnerabilityTime;
					StartCoroutine(DamageEffect());
				}
			}
		}

		IEnumerator DamageEffect()
		{
			while (damagedDelta > 0f)
			{
				materialColor.a = 0f;
				playerMaterial.color = materialColor;
				yield return new WaitForSeconds(0.1f);

				materialColor.a = 1f;
				playerMaterial.color = materialColor;
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
			if (context.started)
			{
				if (state.HasFlag(PlayerState.Jumped) && jumpCount < maxJumps)
				{
					controller.velocity.Set(controller.velocity.x, 0f, controller.velocity.z); // zero out velocity for double (triple?) jumps
					Jump();
				}

				if (state.HasFlag(PlayerState.Grounded))
				{
					Jump();
					state |= PlayerState.Jumped;
					state &= ~PlayerState.Grounded;
				}

				jumpCount++;
			}
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
		/// Called when movement input is given
		/// </summary>
		public void OnMove(InputAction.CallbackContext context)
		{
			if (context.started) state |= PlayerState.Moving;
			if (context.canceled) state &= ~PlayerState.Moving;

			var input = context.ReadValue<Vector2>();
			movement = input.x;
		}
	}
}