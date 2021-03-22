using UnityEngine;

/// <summary>
/// Implements the core game logic
/// </summary>
public class GameManager : MonoBehaviour
{
	static GameManager instance = null;
	int frameRate = 0;

	/// <summary>
	/// Set up a singleton. We don't want more than one game manager instance.
	/// </summary>
	private void Awake()
	{
		if (instance != null)
			Destroy(gameObject);
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// Update our frame rate value every tick.
	/// </summary>
	private void Update()
	{
		frameRate = (int)(1f / Time.unscaledDeltaTime);
	}

	/// <summary>
	/// Retrieve the current frame rate.
	/// </summary>
	public static int GetFrameRate() => instance.frameRate;
}
