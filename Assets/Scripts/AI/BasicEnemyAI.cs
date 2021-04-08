using UnityEngine;

public class BasicEnemyAI : MonoBehaviour
{
	[SerializeField] float moveSpeed = 0.25f;
	[SerializeField] float forceWhenDamaged = 5f;

	[Header("Waypoints")]
	[SerializeField] float waypointEpsilon = 0.1f;
	[SerializeField] Transform[] waypoints = null;

	Rigidbody physicsBody = null;
	int nextWaypoint = 0;

	private void Awake()
	{
		physicsBody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		transform.position = Vector3.MoveTowards(transform.position, waypoints[nextWaypoint].position, moveSpeed * Time.fixedDeltaTime);

		if (Vector3.Distance(transform.position, waypoints[nextWaypoint].position) < waypointEpsilon)
		{
			nextWaypoint++;
			nextWaypoint %= waypoints.Length;
			transform.LookAt(waypoints[nextWaypoint], Vector3.up);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player Attack"))
			physicsBody.AddForce(Vector3.Normalize(-transform.forward) * forceWhenDamaged, ForceMode.Impulse);
	}
}
