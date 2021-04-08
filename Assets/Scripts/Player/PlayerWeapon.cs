using Capstone.AI;
using UnityEngine;

/// <summary>
/// Player weapon behavior
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
	public float lifetime = 0.5f;

	float lifeDelta = 0f;

	/// <summary>
	/// Deactivate after a specified time
	/// </summary>
	private void Update()
	{
		lifeDelta += Time.deltaTime;

		if (lifeDelta < lifetime) return;
		lifeDelta = 0f;
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Reset the lifetime in case the player attacks again before the end of the lifetime
	/// </summary>
	public void Refresh() => lifeDelta = 0f;

	/// <summary>
	/// and of course, weapons hurt when colliding with enemies
	/// </summary>
	/// <param name="collision">The enemy</param>
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
			collision.gameObject.GetComponent<BaseEnemyAI>().Damage(transform.forward);
	}
}
