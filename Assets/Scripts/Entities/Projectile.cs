using Capstone.Player;
using UnityEngine;

namespace Capstone.Entities
{
	public class Projectile : MonoBehaviour
	{
		[SerializeField] float lifetime = 3f;
		[SerializeField] float speed = 2f;
		[SerializeField] Color[] possibleColors = null;

		public bool PositiveForward { get; set; } = true;

		float lifeDelta = 0f;

		/// <summary>
		/// Give the projectile a random color.
		/// </summary>
		private void Start() => GetComponentInChildren<Renderer>().material.color = possibleColors[Random.Range(0, possibleColors.Length)];

		/// <summary>
		/// Move the projectile along its Z axis until lifetime expires.
		/// </summary>
		private void Update()
		{
			lifeDelta += Time.deltaTime;

			if (PositiveForward)
				transform.position += transform.forward * speed * Time.deltaTime;
			else
				transform.position -= transform.forward * speed * Time.deltaTime;

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

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag("Player"))
				collision.gameObject.GetComponent<PlayerController>().Damage();
			Recycle();
		}
	}
}
