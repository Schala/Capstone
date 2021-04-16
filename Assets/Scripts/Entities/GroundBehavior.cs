using UnityEngine;

namespace Capstone.Entities
{
	/// <summary>
	/// Keeps gravity factored in to an entity
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
    public class GroundBehavior : MonoBehaviour
    {
		[SerializeField] float collisionRadius = 0.75f;
		public bool alwaysUseGravity = false;

		Rigidbody physicsBody = null;
		LayerMask groundMask;

		private void Awake()
		{
			physicsBody = GetComponent<Rigidbody>();
			groundMask = LayerMask.GetMask("Ground");
		}

		/// <summary>
		/// Is the entity on the ground?
		/// </summary>
		public bool IsGrounded => Physics.CheckSphere(transform.position, collisionRadius, groundMask);

		/// <summary>
		/// Basic gravity check
		/// </summary>
		private void FixedUpdate()
		{
			if (alwaysUseGravity) return;

			if (IsGrounded)
				physicsBody.useGravity = false;
			else
				physicsBody.useGravity = true;
		}
	}
}
