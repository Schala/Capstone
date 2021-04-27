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
		const float demoLineWidth = 2.5f;

		GUIStyle style1 = new GUIStyle();
		GUIStyle style2 = new GUIStyle();
		float demoT = 0f;
		float radius = 10f;

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

			// circle
			if (spline.looped)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Radius: ");
				radius = float.Parse(GUILayout.TextField(radius.ToString()));
				if (GUILayout.Button("Convert to Circle", EditorStyles.miniButtonRight))
				{
					Undo.RecordObject(spline, "Convert to Circle");
					spline.Circle(radius);
					serializedObject.Update();
				}
				EditorGUILayout.EndHorizontal();
			}

			// This is to just demo the forward vector of points in the scene view of the editor for debug purposes.
			if (spline.looped)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Preview Agent: ");
				demoT = GUILayout.HorizontalSlider(demoT, 0f, spline.points.Count - 0.001f);
				EditorGUILayout.EndHorizontal();
			}

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Draw spline elements in the 3D view.
		/// </summary>
		public void OnSceneGUI()
		{
			style1.normal.textColor = Color.green;
			style1.fontSize = 24;

			style2.normal.textColor = Color.blue;
			style2.fontSize = 24;

			Tools.current = Tool.None; // disable default handles

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
				{
					var p = points[i];
					p.Position += deltaPos;
					points[i] = p;
				}
				serializedObject.Update();
			}

			// and for each point
			for (int i = 0; i < points.Count; i++)
			{
				var p = points[i];
				/*if (spline.looped) */p.Length = spline.GetLength(i);

				Handles.color = Color.blue;
				Handles.SphereHandleCap(0, p.Position, Quaternion.identity, 0.5f, EventType.Repaint);

				// label our points with a number, length, and a location hint
				Handles.Label(p.Position, $"X: {p.Position.x}, Y: {p.Position.y}, Z: {p.Position.z}");
				Handles.Label(p.Position + Vector3.down * 0.5f, $"Len: {p.Length}");
				Handles.BeginGUI();
				var pos2D = HandleUtility.WorldToGUIPoint(p.Position);
				GUI.Label(new Rect(pos2D.x, pos2D.y, 50f, 50f), i.ToString(), style1);
				Handles.EndGUI();

				// handle movement of a point
				EditorGUI.BeginChangeCheck();
				var newPosition = Handles.PositionHandle(p.Position, Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, "Change Point Position");
					p.Position = newPosition;
					serializedObject.Update();
				}
			}

			Handles.color = Color.grey;
			if (spline.looped)
			{
				// draw circle
				for (float t = 0.005f; t < points.Count; t += 0.005f)
				{
					var prev = spline.GetPoint(t - 0.005f).Position;
					var pos = spline.GetPoint(t).Position;
					Handles.DrawLine(prev, pos);
				}
			}
			else
			{
				demoT /= spline.points.Count; // normalise agent to avoid out of range

				// draw curve
				for (float t = 0.005f; t < points.Count - 3f; t += 0.005f)
				{
					var prev = spline.GetPoint(t - 0.005f).Position;
					var pos = spline.GetPoint(t).Position;
					Handles.DrawLine(prev, pos);
				}

				// draw a dotted line for the anchors
				Handles.color = Color.green;
				Handles.DrawDottedLine(points[0].Position, points[1].Position, 3f);
				Handles.DrawDottedLine(points[points.Count - 1].Position, points[points.Count - 2].Position, 3f);
			}

			// draw our demo agent for forward previewing
			Handles.color = Color.red;
			var demoPoint = spline.GetPoint(demoT);
			var demoForward = spline.GetPoint(demoT, true);
			float r = Mathf.Atan2(-demoForward.Position.z, demoForward.Position.x);
			var demoP1 = new Vector3
			{
				x = demoLineWidth * Mathf.Sin(r) + demoPoint.Position.x,
				y = demoPoint.Position.y,
				z = demoLineWidth * Mathf.Cos(r) + demoPoint.Position.z,
			};
			var demoP2 = new Vector3
			{
				x = -demoLineWidth * Mathf.Sin(r) + demoPoint.Position.x,
				y = demoPoint.Position.y,
				z = -demoLineWidth * Mathf.Cos(r) + demoPoint.Position.z,
			};
			Handles.DrawLine(demoP1, demoP2);
		}
	}
}
