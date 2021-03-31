/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street - Fifth Floor, Boston, MA 02110-1301, USA.
 */

using UnityEngine;

namespace Capstone.AI
{
    /// Manages AI state flow
    [RequireComponent(typeof(AIMovement))]
    public class StateController : MonoBehaviour
    {
        public Transform[] waypoints;
        public bool active;
        public float lookRadius = 1f;
        public float searchDuration = 4f;

        [Header("States")]
        public State currentState;
        public State remainState;

        public int NextWaypoint { get; set; } = 0;
        public float StateTimeElapsed { get; private set; } = 0f;
        public AIMovement Movement { get; private set; } = null;
        public Transform Target { get; set; } = null;

        private void Awake() => Movement = GetComponent<AIMovement>();

        void Update()
        {
            if (!active) return;
            currentState.UpdateState(this);
        }

        private void Start() => Movement.Active = active;

        /// Draw some helpful debug visuals.
        private void OnDrawGizmos()
        {
            if (currentState == null) return;
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position, lookRadius);
        }

        /// Transition to the specified state.
        public void TransitionToState(State newState)
        {
            if (newState == remainState) return;
            currentState = newState;
            StateTimeElapsed = 0f;
        }

        /// Has the state elapsed the specified duration?
        public bool CheckIfCountDownElapsed(float duration)
        {
            StateTimeElapsed += Time.deltaTime;
            return StateTimeElapsed >= duration;
        }
    }
}
