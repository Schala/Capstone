using Capstone.Player;
using UnityEngine;

/// <summary>
/// When the player sets foot on the swing's platform, make the platform the parent of the player, so that we stay on it until we jump off.
/// </summary>
public class SwingCollision : MonoBehaviour
{
	[SerializeField] Transform swing = null;

	/// <summary>
	/// Parent the player to the swing so we don't slide off.
	/// </summary>
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		other.transform.SetParent(swing, true);
		other.attachedRigidbody.useGravity = false;
		other.GetComponent<PlayerController>().ShouldStep = false;
	}

	/// <summary>
	/// Keep the player upright.
	/// </summary>
	private void OnTriggerStay(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		other.transform.up = Vector3.up;
	}

	/// <summary>
	/// Unparent the swing from the player since it should no longer influence movement.
	/// </summary>
	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		other.transform.SetParent(null, true);
		other.attachedRigidbody.useGravity = true;
		other.GetComponent<PlayerController>().ShouldStep = true;
	}
}
