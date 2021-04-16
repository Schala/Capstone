using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// AI for an enemy that simply moves between points
	/// </summary>
	public class Traveler : MonoBehaviour
	{
		[Header("Movement")]
		[SerializeField] float moveSpeed = 5f;
		[SerializeField] float turnSpeed = 100f;
		[SerializeField] bool constrainToZAxis = true;

		[Header("Waypoints")]
		[SerializeField] float waypointEpsilon = 0.1f;
		[SerializeField] Transform[] waypoints = null;

		Rigidbody physicsBody = null;
		int nextWaypoint = 1;
		bool turning = false;

		private void Awake() => physicsBody = GetComponent<Rigidbody>();

		private void FixedUpdate()
		{
			if (Mathf.Abs(transform.position.z - waypoints[nextWaypoint].position.z) > waypointEpsilon)
				Move();
			
			if (Mathf.Abs(transform.position.z - waypoints[nextWaypoint].position.z) < waypointEpsilon)
			{
				Stop();
				nextWaypoint++;
				nextWaypoint %= waypoints.Length;
				turning = true;
			}

			if (turning)
			{
				var distance = waypoints[nextWaypoint].position - transform.position;
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(distance), turnSpeed * Time.deltaTime);
				transform.eulerAngles = Vector3.up * transform.eulerAngles.y;
			}
		}

		/// <summary>
		/// Apply physics velocity.
		/// </summary>
		void Move() => physicsBody.velocity = transform.forward * moveSpeed;

		/// <summary>
		/// Halt velocity.
		/// </summary>
		void Stop() => physicsBody.velocity = Vector3.zero;

		private void OnEnable() => Move();

		private void OnDisable() => Stop();
	}
}
