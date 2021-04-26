using UnityEditor;
using UnityEngine;

namespace Capstone.Helpers
{
	/// <summary>
	/// Inspector frontend for splines.
	/// </summary>
	[CustomEditor(typeof(Spline))]
	public class SplineEditor : Editor
	{
		/// <summary>
		/// Construct the property view.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var spline = (Spline)target;

			if (GUILayout.Button("Add Anchor"))
			{
				Undo.RecordObject(spline, "Add Anchor");
				spline.AddAnchor();
				spline.SetDirty();
				serializedObject.Update();
			}

			if (GUILayout.Button("Remove Last Anchor"))
			{
				Undo.RecordObject(spline, "Remove Last Anchor");
				spline.RemoveLastAnchor();
				spline.SetDirty();
				serializedObject.Update();
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("normal"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));

			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button("Set All Z = 0"))
			{
				Undo.RecordObject(spline, "Set All Z = 0");
				spline.SetAllZ0();
				spline.SetDirty();
				serializedObject.Update();
			}

			if (GUILayout.Button("Set All Y = 0"))
			{
				Undo.RecordObject(spline, "Set All Y = 0");
				spline.SetAllY0();
				spline.SetDirty();
				serializedObject.Update();
			}
		}

		/// <summary>
		/// Draw the spline editor in the scene view.
		/// </summary>
		public void OnSceneGUI()
		{
			var spline = (Spline)target;
			var transformPosition = spline.transform.position;
			var anchors = spline.Anchors;

			if (anchors == null) return;

			for (int i = 0; i < anchors.Count; i++)
			{
				Handles.color = Color.white;
				Handles.DrawWireCube(transformPosition + anchors[i].position, Vector3.one * 0.5f);
				EditorGUI.BeginChangeCheck();
				var newPosition = Handles.PositionHandle(transformPosition + anchors[i].position, Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, "Change Anchor Position");
					anchors[i].position = newPosition - transformPosition;
					spline.SetDirty();
					serializedObject.Update();
				}

				Handles.color = Color.green;
				Handles.SphereHandleCap(0, transformPosition + anchors[i].handleAPosition, Quaternion.identity, 0.5f, EventType.Repaint);
				EditorGUI.BeginChangeCheck();
				newPosition = Handles.PositionHandle(transformPosition + anchors[i].handleAPosition, Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, "Change Anchor Handle A Position");
					anchors[i].handleAPosition = newPosition - transformPosition;
					spline.SetDirty();
					serializedObject.Update();
				}

				Handles.color = Color.blue;
				Handles.SphereHandleCap(0, transformPosition + anchors[i].handleBPosition, Quaternion.identity, 0.5f, EventType.Repaint);
				EditorGUI.BeginChangeCheck();
				newPosition = Handles.PositionHandle(transformPosition + anchors[i].handleBPosition, Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(spline, "Change Anchor Handle B Position");
					anchors[i].handleBPosition = newPosition - transformPosition;
					spline.SetDirty();
					serializedObject.Update();
				}

				Handles.color = Color.white;
				Handles.DrawLine(transformPosition + anchors[i].position, transformPosition + anchors[i].handleAPosition);
				Handles.DrawLine(transformPosition + anchors[i].position, transformPosition + anchors[i].handleBPosition);
			}

			// draw bezier
			for (int i = 0; i < anchors.Count - 1; i++)
			{
				var anchor = anchors[i];
				var nextAnchor = anchors[i + 1];
				Handles.DrawBezier(transformPosition + anchor.position, transformPosition + nextAnchor.position, transformPosition + anchor.handleBPosition,
					transformPosition + nextAnchor.handleAPosition, Color.grey, null, 3f);
			}

			if (spline.IsLoop)
			{
				var anchor = anchors[anchors.Count - 1];
				var nextAnchor = anchors[0];
				Handles.DrawBezier(transformPosition + anchor.position, transformPosition + nextAnchor.position, transformPosition + anchor.handleBPosition,
					transformPosition + nextAnchor.handleAPosition, Color.grey, null, 3f);
			}
		}
	}
}