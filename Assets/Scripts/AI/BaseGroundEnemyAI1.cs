using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// Abstract base class for ground-dwelling enemies
	/// </summary>
	public abstract class BaseGroundEnemyAI : BaseEnemyAI
	{
		[SerializeField] float collisionRadius = 0.75f;
		
		LayerMask groundMask;

		private void Awake()
		{
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
