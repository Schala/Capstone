using System.Collections;
using UnityEngine;

namespace Capstone.AI
{
    /// <summary>
    /// AI for an enemy that fires projectiles
    /// </summary>
    [RequireComponent(typeof(PlayerAwareness))]
    public class Shooter : MonoBehaviour
    {
        [SerializeField] Transform projectileSpawnPoint = null;
        [SerializeField] string projectileTag = string.Empty;
        [SerializeField] float shootRate = 2f;
        [SerializeField] float lineOfSightRadius = 5f;
        [SerializeField] float turnSpeed = 100f;

        PlayerAwareness awareness = null;
        bool canFire = true;

        private void Awake() => awareness = GetComponent<PlayerAwareness>();

		/// <summary>
		/// If conditions are right, shoot at the player periodically.
		/// </summary>
		private void Update()
        {
            if (!awareness.enabled || awareness.Target == null) return;

            var distance = awareness.Target.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(distance), turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * transform.eulerAngles.y;

            if (canFire) StartCoroutine(Shoot());
        }

        /// <summary>
        /// Retrieve a projectile from our object pool and set its position and forward vector.
        /// </summary>
        IEnumerator Shoot()
        {
            canFire = false;
            yield return new WaitForSeconds(shootRate);
            var projectile = ObjectPool.Get(projectileTag);
            var behavior = projectile.GetComponent<Projectile>();
            projectile.transform.position = projectileSpawnPoint.position;
            behavior.PositiveForward = projectileSpawnPoint.forward.z > 0f;
            projectile.SetActive(true);
            canFire = true;
        }
    }
}
