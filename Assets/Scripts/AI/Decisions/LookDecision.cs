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

namespace Capstone.AI.Decisions
{
	/// <summary>
	/// AI look-for-target logic
	/// </summary>
	[CreateAssetMenu(menuName = "AI/Decisions/Look")]
	public class LookDecision : Decision
	{
		public override bool Decide(StateController controller)
		{
			var hits = Physics.OverlapSphere(controller.transform.position, controller.lookRadius);

			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].CompareTag("Player"))
				{
					controller.Target = hits[i].transform;
					return true;
				}
			}

			return false;
		}
	}
}
