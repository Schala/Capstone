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

		Rigidbody physicsBody = null;
		LayerMask groundMask;

		private void Awake()
		{
			physicsBody = GetComponent<Rigidbody>();
			groundMask = LayerMask.GetMask("Ground");
		}

		/// <summary>
		/// Basic gravity check
		/// </summary>
		private void FixedUpdate()
		{
			if (Physics.CheckSphere(transform.position, collisionRadius, groundMask))
				physicsBody.useGravity = false;
			else
				physicsBody.useGravity = true;
		}
	}
}
