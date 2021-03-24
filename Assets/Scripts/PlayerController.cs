using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone
{
	/// <summary>
	/// Various states of player input
	/// </summary>
	[Flags]
	public enum PlayerInputFlags : byte
	{
		None = 0,
		Grounded = 1,
		Moving = 2,
		Jumped = 4,
		Fired = 8
	}

	/// <summary>
	/// Manages player input, movement and actions
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class PlayerController : MonoBehaviour, InputActions.IPlayerActions
	{
		[SerializeField] float moveSpeed = 5f;
		[SerializeField] float jumpForce = 1f;
		[SerializeField] float jumpTimeMax = 0.25f;
		[SerializeField] float gravityMultiplier = -3f;
		[SerializeField] int maxJumps = 2;

		CharacterController controller = null;
		InputActions inputActions = null;
		Vector3 velocity = Vector3.zero;
		float movement = 0f;
		float jumpDelta = 0f;
		int jumpCount = 0;
		PlayerInputFlags flags = PlayerInputFlags.None;

		/// <summary>
		/// Set up our input actions.
		/// </summary>
		private void Awake()
		{
			controller = GetComponent<CharacterController>();

			inputActions = new InputActions();
			inputActions.Player.SetCallbacks(this);
		}

		/// <summary>
		/// Apply any motion specified to the player.
		/// </summary>
		private void Update()
		{
			velocity = controller.velocity;

			if (flags.HasFlag(PlayerInputFlags.Moving))
				controller.Move(Vector3.forward * movement * moveSpeed * Time.deltaTime);

			if (!flags.HasFlag(PlayerInputFlags.Grounded))
				velocity.y += Physics.gravity.y * Time.deltaTime;
			controller.Move(velocity * Time.deltaTime); // apply gravity

			if (flags.HasFlag(PlayerInputFlags.Jumped))
				jumpDelta += Time.deltaTime;
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
			if (hit.gameObject.CompareTag("Ground"))
			{
				flags |= PlayerInputFlags.Grounded;
				flags &= ~PlayerInputFlags.Jumped;
				jumpCount = 0;
				jumpDelta = 0f;
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
				if (flags.HasFlag(PlayerInputFlags.Jumped) && jumpCount < maxJumps)
				{
					controller.velocity.Set(controller.velocity.x, 0f, controller.velocity.z); // zero out velocity for double (triple?) jumps
					Jump();
				}

				if (flags.HasFlag(PlayerInputFlags.Grounded))
				{
					Jump();
					flags |= PlayerInputFlags.Jumped;
					flags &= ~PlayerInputFlags.Grounded;
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
			if (context.started) flags |= PlayerInputFlags.Moving;
			if (context.canceled) flags &= ~PlayerInputFlags.Moving;

			var input = context.ReadValue<Vector2>();
			movement = -input.x; // Negate input or our movement will be inverted.
		}
	}
}