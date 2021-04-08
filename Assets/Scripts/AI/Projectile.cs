using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float lifetime = 3f;

	float lifeDelta = 0f;

	private void Update()
	{
		if (lifeDelta < lifetime) return;
		Recycle();
	}

	/// <summary>
	/// Recycles this projectile for future re-use.
	/// </summary>
	void Recycle()
	{
		lifeDelta = 0f;
		gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		Recycle();
	}
}
