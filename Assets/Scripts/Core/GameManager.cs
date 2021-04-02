using System.IO;
using UnityEngine;

namespace Capstone
{
	/// Implements the core game logic
	public class GameManager : MonoBehaviour
	{
		const int SaveVersion = 1;
		static GameManager instance = null;
		int frameRate = 0;

		/// Set up a singleton. We don't want more than one game manager instance.
		private void Awake()
		{
			if (instance != null)
				Destroy(gameObject);
			instance = this;
			DontDestroyOnLoad(gameObject);

			Save();
		}

		/// Update our frame rate value every tick.
		private void Update() => frameRate = (int)(1f / Time.unscaledDeltaTime);

		public static int FrameRate => instance.frameRate;

		/// Generate a CRC from the contents of the provided array.
		static uint GenerateCRC(byte[] data)
		{
			uint crc = 0xFEEDC0DEu;

			for (int i = 0; i < data.Length; i++)
				crc = ((crc << 1) | (((crc & 0x80000000u) != 0u) ? 1u : 0u)) ^ data[i];
			return crc;
		}

		/// Make or overwrite a saved game.
		public static bool Save()
		{
			using var memoryStream = new MemoryStream();
			using var mwriter = new BinaryWriter(memoryStream);

			mwriter.Write(SaveVersion);

			using var fileStream = new FileStream(Application.persistentDataPath + "/game.sav", FileMode.OpenOrCreate);
			using var fwriter = new BinaryWriter(fileStream);
			var data = memoryStream.ToArray();
			memoryStream.Close();

			fwriter.Write(GenerateCRC(data));
			fwriter.Write(data);
			fwriter.Close();

			return true;
		}
	}
}
