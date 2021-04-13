using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// AI for an enemy that simply moves between points
	/// </summary>
	public class Traveler : MonoBehaviour
	{
		[SerializeField] bool active = false;

		[Header("Movement")]
		[SerializeField] float moveSpeed = 5f;
		[SerializeField] float turnSpeed = 100f;

		[Header("Waypoints")]
		[SerializeField] float waypointEpsilon = 0.1f;
		[SerializeField] Transform[] waypoints = null;

		int nextWaypoint = 0;

		private void Update()
		{
			if (!active) return;

			transform.position = Vector3.MoveTowards(transform.position, waypoints[nextWaypoint].position, moveSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, waypoints[nextWaypoint].position) < waypointEpsilon)
			{
				nextWaypoint++;
				nextWaypoint %= waypoints.Length;
				var distance = waypoints[nextWaypoint].position - transform.position;
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(distance), turnSpeed * Time.deltaTime);
				transform.eulerAngles = Vector3.up * transform.eulerAngles.y;
			}
		}
	}
}
