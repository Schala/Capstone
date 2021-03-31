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
	/// A state of AI behavior
	[CreateAssetMenu(menuName = "AI/State")]
	public class State : ScriptableObject
	{
		[SerializeField] Action[] actions;
		[SerializeField] Transition[] transitions;
		public Color sceneGizmoColor = Color.gray;

		/// Called by the state controller to update the associated states
		public void UpdateState(StateController controller)
		{
			DoActions(controller);
			CheckTransitions(controller);
		}

		/// Execute the state's actions.
		void DoActions(StateController controller)
		{
			for (int i = 0; i < actions.Length; i++)
				actions[i].Act(controller);
		}

		/// Check to see if the state should transition or not.
		void CheckTransitions(StateController controller)
		{
			for (int i = 0; i < transitions.Length; i++)
			{
				bool decisionSuccess = transitions[i].decision.Decide(controller);
				controller.TransitionToState(decisionSuccess ? transitions[i].trueState : transitions[i].falseState);
			}
		}
	}
}
