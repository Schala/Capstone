using UnityEngine;

/// <summary>
/// Manage the game camera.
/// </summary>
public class CameraController : MonoBehaviour
{
	static CameraController instance = null;

	[SerializeField] float offset = -10f;
	[SerializeField] float followSpeed = 0.15f;
	[SerializeField] Transform target = null;
	Vector3 velocity = Vector3.zero;

	/// <summary>
	/// Set up our singleton instance and variables.
	/// </summary>
	private void Awake()
	{
		if (instance != null)
			Destroy(gameObject);
		instance = this;
	}

	/// <summary>
	/// Update the camera to follow the player consistently.
	/// </summary>
	void LateUpdate()
	{
		if (target == null) return;
		var targetPos = target.position;
		targetPos.x += offset;
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, Time.deltaTime * followSpeed);
	}
}
