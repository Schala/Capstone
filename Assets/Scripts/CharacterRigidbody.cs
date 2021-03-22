using UnityEngine;

/// <summary>
/// Pseudo-rigidbody for Unity's CharacterController component
/// </summary>
public class CharacterRigidbody : MonoBehaviour
{
	[SerializeField] float mass = 3f;
	[SerializeField] float minMoveMagnitude = 0.2f;
	[SerializeField] float impactConsumptionFactor = 5f;
	CharacterController controller = null;
	Vector3 impact = Vector3.zero;

	/// <summary>
	/// Cache a reference to the associated CharacterController.
	/// </summary>
	private void Awake() => controller = GetComponent<CharacterController>();

	/// <summary>
	/// Simulate a physics system tick.
	/// </summary>
	private void FixedUpdate()
	{
		if (controller == null) return;

		// apply the impact force
		if (impact.magnitude > minMoveMagnitude)
			controller.Move(impact * Time.fixedDeltaTime);

		// consume the impact energy each cycle
		impact = Vector3.Lerp(impact, Vector3.zero, impactConsumptionFactor * Time.fixedDeltaTime);
	}

	/// <summary>
	/// Adds an impacting force to the game object
	/// </summary>
	/// <param name="direction">Direction of the impact</param>
	/// <param name="force">Amount to apply</param>
	public void AddImpact(Vector3 direction, float force)
	{
		direction.Normalize();

		// reflect down force on the ground
		if (direction.y < 0f) direction.y = -direction.y;

		impact += direction.normalized * force / mass;
	}

	/// <summary>
	/// Zero out the velocity.
	/// </summary>
	public void Stop()
	{
		controller.velocity.Set(0f, 0f, 0f);
		impact = Vector3.zero;
	}
}

