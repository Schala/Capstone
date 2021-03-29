using UnityEngine;
using UnityEngine.InputSystem;

namespace Capstone
{
	/// <summary>
	/// Implements the core game logic
	/// </summary>
	[RequireComponent(typeof(PlayerInput))]
	public class GameManager : MonoBehaviour
	{
		static GameManager instance = null;
		int frameRate = 0;
		PlayerInput playerInput = null;

		/// <summary>
		/// Set up a singleton. We don't want more than one game manager instance.
		/// </summary>
		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);
			instance = this;
			DontDestroyOnLoad(gameObject);

			playerInput = GetComponent<PlayerInput>();
		}

		/// <summary>
		/// Update our frame rate value every tick.
		/// </summary>
		private void Update() => frameRate = (int)(1f / Time.unscaledDeltaTime);

		public static int FrameRate => instance.frameRate;
		public static PlayerInput Input => instance.playerInput;
	}
}
