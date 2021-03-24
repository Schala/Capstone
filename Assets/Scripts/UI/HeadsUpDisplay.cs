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

namespace Capstone.UI
{
	/// <summary>
	/// Heads-up display for the UI
	/// </summary>
	public class HeadsUpDisplay : MonoBehaviour
	{
		Camera pov = null;

		/// <summary>
		/// Identify and cache a reference to the main camera.
		/// </summary>
		private void Awake() => pov = Camera.main;

		/// <summary>
		/// Have the HUD always face the camera.
		/// </summary>
		void LateUpdate() => transform.LookAt(transform.position + pov.transform.forward);
	}
}
