using UnityEngine;

namespace Capstone.AI
{
	public class PlayerAwareness : MonoBehaviour
    {
        [SerializeField] float lineOfSightRadius = 5f;

        LayerMask groundMask;
        LayerMask playerMask;

        private void Awake()
        {
            groundMask = LayerMask.GetMask("Ground");
            playerMask = LayerMask.GetMask("Player");
        }

        public Transform Target { get; private set; } = null;

        /// <summary>
        /// Check to see if we found a player, and if we did, check for a wall, because the player might be behind the wall.
        /// </summary>
        private void FixedUpdate()
        {
            var rayHitWall = Physics.Raycast(transform.position, transform.forward, out var wallHit, lineOfSightRadius, groundMask);
            var playerHit = Physics.OverlapSphere(transform.position, lineOfSightRadius, playerMask);

            if (playerHit.Length != 0)
            {
                if (rayHitWall)
                {
                    var playerDistance = Vector3.Distance(transform.position, playerHit[0].transform.position);
                    var wallDistance = Vector3.Distance(transform.position, wallHit.transform.position);

                    if (wallDistance < playerDistance)
                        return;
                }

                Target = playerHit[0].transform;
            }
            else
                Target = null;
        }
    }
}
