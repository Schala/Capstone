using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// AI for a basic enemy that simply moves between points
	/// </summary>
	public class BasicMovingGroundEnemyAI : BaseGroundEnemyAI
	{
		[SerializeField] float moveSpeed = 0.25f;

		[Header("Waypoints")]
		[SerializeField] float waypointEpsilon = 0.1f;
		[SerializeField] Transform[] waypoints = null;

		int nextWaypoint = 0;

		private void Update()
		{
			transform.position = Vector3.MoveTowards(transform.position, waypoints[nextWaypoint].position, moveSpeed * Time.deltaTime);

			if (Vector3.Distance(transform.position, waypoints[nextWaypoint].position) < waypointEpsilon)
			{
				nextWaypoint++;
				nextWaypoint %= waypoints.Length;
				transform.LookAt(waypoints[nextWaypoint], Vector3.up);
			}
		}
	}
}
