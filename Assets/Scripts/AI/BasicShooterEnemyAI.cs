using UnityEngine;

namespace Capstone.AI
{
    /// <summary>
    /// AI for a basic stationary enemy that fires projectiles
    /// </summary>
    public class BasicShooterEnemyAI : MonoBehaviour
    {
        [SerializeField] float forceWhenDamaged = 5f;

        [Header("Shooting")]
        [SerializeField] Transform projectileSpawn = null;
        [SerializeField] string projectileTag = string.Empty;
        [SerializeField] float shootRate = 2f;
        [SerializeField] float projectileSpeed = 1f;
        [SerializeField] float projectileLifetime = 3f;
        [SerializeField] float lineOfSightRadius = 5f;
        [SerializeField] bool active = false;

        Rigidbody physicsBody = null;
        float shootDelta = 0f;

        private void Awake()
        {
            physicsBody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// If conditions are right, shoot at the player periodically.
        /// </summary>
        private void Update()
        {
            if (!active) return;
            if (!Physics.SphereCast(transform.position, lineOfSightRadius, Vector3.up, out var hit)) return;
            if (!hit.collider.CompareTag("Player")) return;

            transform.LookAt(hit.transform, Vector3.up);

            shootDelta += Time.deltaTime;
            if (shootDelta < shootRate) return;

            shootDelta = 0f;
            var projectile = ObjectPool.Get(projectileTag);
            var behavior = projectile.GetComponent<Projectile>();
            projectile.transform.position = projectileSpawn.position;
            behavior.lifetime = projectileLifetime;
        }
    }
}
