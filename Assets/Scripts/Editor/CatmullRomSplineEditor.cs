using UnityEditor;
using UnityEngine;

namespace Capstone.Helpers
{
	/// <summary>
	/// Unity editor frontend setup for Catmull-Rom spline
	/// </summary>
	[CustomEditor(typeof(CatmullRomSpline))]
	public class CatmullRomSplineEditor : Editor
	{
		GUIStyle style = new GUIStyle();

		/// <summary>
		/// Construct the property view.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var spline = (CatmullRomSpline)target;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("looped"));

			// Add/remove point buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Point", EditorStyles.miniButtonLeft))
			{
				Undo.RecordObject(spline, "Add Point");
				spline.AddPoint();
				serializedObject.Update();
			}
			if (GUILayout.Button("Remove Last Point", EditorStyles.miniButtonRight))
			{
				Undo.RecordObject(spline, "Remove Last Point");
				spline.RemoveLastPoint();
				serializedObject.Update();
			}
			EditorGUILayout.EndHorizontal();

			// flatten X/Y/Z buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Flatten X", EditorStyles.miniButtonLeft))
			{
				Undo.RecordObject(spline, "Flatten X");
				spline.Flatten(Axis.X);
				serializedObject.Update();
			}
			if (GUILayout.Button("Flatten Y", EditorStyles.miniButtonMid))
			{
				Undo.RecordObject(spline, "Flatten Y");
				spline.Flatten(Axis.Y);
				serializedObject.Update();
			}
			if (GUILayout.Button("Flatten Z", EditorStyles.miniButtonRight))
			{
				Undo.RecordObject(spline, "Flatten Z");
				spline.Flatten(Axis.Z);
				serializedObject.Update();
			}
			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Draw spline elements in the 3D view.
		/// </summary>
		public void OnSceneGUI()
		{
			style.normal.textColor = Color.green;
			style.fontSize = 24;

			Tools.current = Tool.None;

			var spline = (CatmullRomSpline)target;
			var points = spline.points;

			if (points == null) return;

			// draw a handle for the spline root
			Handles.color = Color.white;
			Handles.DrawWireCube(spline.transform.position, Vector3.one * 0.5f);
			EditorGUI.BeginChangeCheck();
			var newSplinePos = Handles.PositionHandle(spline.transform.position, Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(spline, "Change Root Position");
				var deltaPos = newSplinePos - spline.transform.position;
				spline.transform.position = newSplinePos;
				for (int i = 0; i < points.Count; i++)
					points[i] += deltaPos;
				serializedObject.Update();
			}

			// and for each point
			for (int i = 0; i < points.Count; i++)
			{
				Handles.color = Color.blue;
				Handles.SphereHandleCap(0, points[i], Quaternion.identity, 0.5f, EventType.Repaint);

				// label our points with a number
				Handles.BeginGUI();
				var pos2D = HandleUtility.WorldToGUIPoint(points[i]);
				GUI.Label(new Rect(pos2D.x, pos2D.y, 50f, 50f), i.ToString(), style);
				Handles.EndGUI();

				// handle movement of a point
				EditorGUI.BeginChangeCheck();
				var newPosition = Handles.PositionHandle(points[i], Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, "Change Point Position");
					points[i] = newPosition;
					serializedObject.Update();
				}
			}

			Handles.color = Color.grey;
			if (spline.looped)
			{
				// draw circle
				for (float t = 0.005f; t < points.Count; t += 0.005f)
				{
					var prev = spline.GetPoint(t - 0.005f);
					var pos = spline.GetPoint(t);
					Handles.DrawLine(prev, pos);
				}
			}
			else
			{
				// draw curve
				for (float t = 0.005f; t < points.Count - 3f; t += 0.005f)
				{
					var prev = spline.GetPoint(t - 0.005f);
					var pos = spline.GetPoint(t);
					Handles.DrawLine(prev, pos);
				}

				// draw a dotted line for the anchors
				Handles.color = Color.green;
				Handles.DrawDottedLine(points[0], points[1], 3f);
				Handles.DrawDottedLine(points[points.Count - 1], points[points.Count - 2], 3f);
			}
		}
	}
}
