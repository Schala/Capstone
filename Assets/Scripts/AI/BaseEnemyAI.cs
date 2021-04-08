using Capstone.Player;
using UnityEngine;

namespace Capstone.AI
{
	/// <summary>
	/// Abstract base class for all enemy behavior
	/// </summary>
	public abstract class BaseEnemyAI : MonoBehaviour
	{
		[SerializeField] float forceWhenDamaged = 5f;

		protected Rigidbody physicsBody = null;

		private void Awake()
		{
			physicsBody = GetComponent<Rigidbody>();
		}

		/// <summary>
		/// Enemy damage and knockback
		/// </summary>
		/// <param name="direction">Where the attack came from</param>
		public void Damage(Vector3 direction) => physicsBody.AddForce(direction.normalized * forceWhenDamaged, ForceMode.Impulse);

		/// Called when the enemy touches the player
		private void OnCollisionEnter(Collision collision)
		{
			// give the player some knockback and activate limited invulnerability
			if (collision.gameObject.CompareTag("Player"))
				collision.gameObject.GetComponent<PlayerController>().Damage();
		}

		/// If the player touches us but we're using a trigger volume.
		private void OnTriggerEnter(Collider other)
		{
			// give the player some knockback and activate limited invulnerability
			if (other.CompareTag("Player"))
				other.GetComponent<PlayerController>().Damage();

			if (other.CompareTag("Player Attack"))
				Damage(other.transform.forward);
		}

		/// Is the player being naughty and standing in the fire?
		private void OnTriggerStay(Collider other)
		{
			// give the player some knockback and activate limited invulnerability
			if (other.CompareTag("Player"))
				other.GetComponent<PlayerController>().Damage();
		}
	}
}
