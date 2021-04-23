using UnityEditor;

namespace Capstone.Helpers
{
	[CustomEditor(typeof(Spline))]
	public class SplineEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var spline = (Spline)target;

			serializedObject.ApplyModifiedProperties();
		}
	}
}