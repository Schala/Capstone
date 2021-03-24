using UnityEngine;
using UnityEngine.AI;

namespace Capstone.AI
{
	/// <summary>
	/// Wraps and manages nav mesh agent behavior
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	public class AIMovement : MonoBehaviour
	{
		public float moveSpeed = 1f;
		public float turnSpeed = 10f;
		public float epsilon = 0.1f;

		NavMeshAgent navMeshAgent = null;

		private void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
			navMeshAgent.speed = moveSpeed;
			navMeshAgent.angularSpeed = turnSpeed;
		}

		/// <summary>
		/// Is the underlying nav mesh agent active?
		/// </summary>
		public bool Active
		{
			get
			{
				return navMeshAgent.enabled;
			}
			set
			{
				navMeshAgent.enabled = value;
			}
		}

		/// <summary>
		/// Move to the specified position.
		/// </summary>
		public void Move(Vector3 position)
		{
			Continue();
			navMeshAgent.SetDestination(position);
		}

		/// <summary>
		/// Has the AI arrived at its destination?
		/// </summary>
		public bool HasArrived() => navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending;

		/// <summary>
		/// Stop moving.
		/// </summary>
		public void Stop() => navMeshAgent.isStopped = true;

		/// <summary>
		/// Continue moving.
		/// </summary>
		public void Continue() => navMeshAgent.isStopped = false;

		/// <summary>
		/// Is the AI in range of the target position?
		/// </summary>
		public bool IsInRange(Vector3 target)
		{
			if (target == null) return true; // crash mitigation
			return Vector3.Distance(transform.position, target) < epsilon;
		}
	}
}
