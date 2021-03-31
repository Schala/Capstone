using UnityEngine;
using UnityEngine.AI;

namespace Capstone.AI
{
	/// Wraps and manages nav mesh agent behavior
	[RequireComponent(typeof(NavMeshAgent))]
	public class AIMovement : MonoBehaviour
	{
		public float moveSpeed = 1f;
		public float turnSpeed = 10f;
		public float epsilon = 0.1f;
		public float groundDistanceFactor = 0.1f;
		[SerializeField] LayerMask groundMask;

		NavMeshAgent navMeshAgent = null;

		/// Sync our settings to the nav mesh agent's.
		private void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
			navMeshAgent.speed = moveSpeed;
			navMeshAgent.angularSpeed = turnSpeed;
		}

		/// Is the underlying nav mesh agent active?
		public bool Active
		{
			get => navMeshAgent.enabled;
			set => navMeshAgent.enabled = value;
		}

		/// Move to the specified position.
		public void Move(Vector3 position)
		{
			if (!Active) return;

			Continue();
			navMeshAgent.SetDestination(position);
		}

		/// Has the AI arrived at its destination?
		public bool HasArrived()
		{
			if (!Active) return false;
			return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending;
		}

		/// Stop moving.
		public void Stop()
		{
			if (!Active) return;
			navMeshAgent.isStopped = true;
		}

		/// Continue moving.
		public void Continue()
		{
			if (!Active) return;
			navMeshAgent.isStopped = false;
		}

		/// Is the AI in range of the target position?
		public bool IsInRange(Vector3 target) => Vector3.Distance(transform.position, target) < epsilon;
	}
}
