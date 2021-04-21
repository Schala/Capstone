using Capstone.Entities;
using System;
using System.Collections;
using UnityEngine;

namespace Capstone.AI.Test
{
    /// <summary>
    /// Taisamba 3 cycles through these phases during the fight.
    /// </summary>
    public enum Taisamba3State
    {
        Standby = 0,
        Idle,
        Shoot,
        Beam,
        Submerge
    }

    /// <summary>
    /// Proof of concept recreation of the Taisamba 3 boss fight AI from Ganbare Goemon
    /// </summary>
	public class Taisamba3 : MonoBehaviour
    {
        [SerializeField] Transform target = null;
        [SerializeField] Transform mouth = null;
        [SerializeField] Water water = null;
        [SerializeField] float turnSpeed = 15f;

        [Header("Shoot Phase")]
        [SerializeField] int maxShots = 3;
        [SerializeField] float shotCooldown = 2f;
        [SerializeField] float shotWarmUp = 1f;
        [SerializeField] string projectileTag = string.Empty;

        [Header("Beam Phase")]
        [SerializeField] float rotationDegrees = 45f;

        [Header("Submerge Phase")]
        [SerializeField] float submergeDuration = 5f;

        Taisamba3State[] cycleOrder = null;
        Taisamba3State cycleIndex = Taisamba3State.Standby;
        Taisamba3State state = Taisamba3State.Standby;

        private void Awake() => cycleOrder = new Taisamba3State[3];

        /// <summary>
        /// Randomise the attack order each cycle.
        /// </summary>
        void RandomiseCycleOrder(Taisamba3State min, Taisamba3State max)
        {
            for (int i = 0; i < 3; i++)
            {
                var j = (Taisamba3State)UnityEngine.Random.Range((int)min, (int)max + 1);
                while (Array.Exists(cycleOrder, k => j == k))
                    j = (Taisamba3State)UnityEngine.Random.Range((int)min, (int)max + 1);
                cycleOrder[i] = j;
            }
        }

		private void Update()
		{
            if (state != Taisamba3State.Standby) return;

            switch (cycleIndex)
            {
                case Taisamba3State.Shoot: StartCoroutine(ShootPhase()); break;
                //case Taisamba3State.Beam: StartCoroutine(BeamPhase()); break;
                default: state = Taisamba3State.Standby; break;
            }

            if (++cycleIndex > Taisamba3State.Submerge)
            {
                cycleIndex = 0;
                RandomiseCycleOrder(Taisamba3State.Shoot, Taisamba3State.Submerge);
            }
		}

		/// <summary>
		/// Execute the shoot phase.
		/// </summary>
		IEnumerator ShootPhase()
        {
            if (state == Taisamba3State.Shoot) yield return null;
            state = Taisamba3State.Shoot;

            for (int i = 0; i < maxShots; i++)
            {
                // Face the player
                var distance = target.position - transform.position;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(distance), turnSpeed * Time.deltaTime);
                transform.eulerAngles = Vector3.up * transform.eulerAngles.y;

                // Fire the shot
                yield return new WaitForSeconds(shotWarmUp);
                var projectile = ObjectPool.Get(projectileTag);
                var behavior = projectile.GetComponent<Projectile>();
                projectile.transform.position = mouth.position;
                behavior.PositiveForward = mouth.forward.z > 0f;
                projectile.SetActive(true);

                // Wait for cooldown
                yield return new WaitForSeconds(shotCooldown);
            }

            state = Taisamba3State.Standby;
        }

        /// <summary>
        /// Execute the beam phase.
        /// </summary>
        /*IEnumerable BeamPhase()
        {
            if (state == Taisamba3State.Beam) yield return null;
            state = Taisamba3State.Beam;

            var distance = target.position - transform.position;
            var targetAngles
        }*/
    }
}
