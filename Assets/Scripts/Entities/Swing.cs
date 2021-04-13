using UnityEngine;

public class Swing : MonoBehaviour
{
	[SerializeField] float maxRotation = 0.5f;
	[SerializeField] float speed = 2f;
	[SerializeField] float direction = 1f;

	Quaternion start = Quaternion.identity;

	private void Start() => start = transform.rotation;

	/// <summary>
	/// Swing the platform back and forth. This is probably not the best way of doing this, but it works.
	/// </summary>
	private void Update()
	{
		var current = start;
		current.x = direction * (maxRotation * Mathf.Sin(Time.time * speed));
		transform.rotation = current;
	}
}
