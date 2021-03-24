using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
	/// <summary>
	/// Allows for multiple tags on an object
	/// </summary>
	public class TagCollection : MonoBehaviour
	{
		public List<string> tags = null;

		/// <summary>
		/// Initiate the collection, adding any existing tag as the first tag in it.
		/// </summary>
		private void Awake()
		{
			if (CompareTag("Untagged"))
			{
				tags = new List<string>(0);
				return;
			}

			if (tags == null) tags = new List<string>(1) { tag };
		}
	}
}
