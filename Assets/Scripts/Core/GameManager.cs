using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone
{
	/// Implements the core game logic
	public class GameManager : MonoBehaviour
	{
		static GameManager instance = null;
		int frameRate = 0;

		/// Set up a singleton. We don't want more than one game manager instance.
		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		/// Update our frame rate value every tick.
		private void Update() => frameRate = (int)(1f / Time.unscaledDeltaTime);

		public static int FrameRate => instance.frameRate;
	}
}
