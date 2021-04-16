using Capstone.Entities;
using System;
using System.Collections;
using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// Flags for jumper functionality
	/// </summary>
	[Flags]
	public enum JumperFlags : byte
	{
		None = 0,
		Twirl = 1,
		FaceCamera = 2,
		RandomFrequency = 4,
		MoveWhileJumping = 8
	}

	/// <summary>
	/// AI for enemy jumping capabilities
	/// </summary>
	[RequireComponent(typeof(GroundBehavior))]
	public class Jumper : MonoBehaviour
	{
		[SerializeField] float maxFrequency = 1f;
		[SerializeField] float minFrequency = 1f; // ignored if not random
		[SerializeField] float force = 10f;
		[SerializeField] float waitToMoveAfterJump = 0.5f;
		[SerializeField] JumperFlags flags = JumperFlags.None;

		GroundBehavior groundBehavior = null;
		Rigidbody physicsBody = null;
		Traveler movement = null;
		bool canJump = true;

		private void Awake()
		{
			groundBehavior = GetComponent<GroundBehavior>();
			groundBehavior.alwaysUseGravity = true;
			physicsBody = GetComponent<Rigidbody>();
			movement = GetComponent<Traveler>();
		}

		private void FixedUpdate()
		{
			if (canJump) StartCoroutine(Jump());
		}

		/// <summary>
		/// Check our settings apply jump velocity, then wait for the next fixed update, then check if we're grounded, wait a bit to move, then finally wait for the specified frequency.
		/// </summary>
		IEnumerator Jump()
		{
			canJump = false;
			if (!flags.HasFlag(JumperFlags.MoveWhileJumping) && movement)
				movement.enabled = false;
			physicsBody.velocity += Vector3.up * force;
			yield return new WaitForFixedUpdate();
			if (!flags.HasFlag(JumperFlags.MoveWhileJumping) && movement)
			{
				yield return new WaitForSeconds(waitToMoveAfterJump);
				yield return new WaitUntil(() => groundBehavior.IsGrounded);
				movement.enabled = true;
			}
			else
				yield return new WaitUntil(() => groundBehavior.IsGrounded);
			yield return new WaitForSeconds(flags.HasFlag(JumperFlags.RandomFrequency) ? UnityEngine.Random.Range(minFrequency, maxFrequency) : maxFrequency);
			canJump = true;
		}
	}
}
