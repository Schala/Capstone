using System.Collections.Generic;
using UnityEngine;

namespace Capstone.Editor
{
	/// <summary>
	/// Editor script to visually set up a spline
	/// </summary>
	[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
	public class EditorSpline : MonoBehaviour
	{
		[SerializeField] GameObject pointPrefab = null;
		[SerializeField] int density = 5;
		[SerializeField] int pointCount = 3;
		[SerializeField] Color color = Color.green;
		[SerializeField] float width = 0.2f;

		List<GameObject> controlPoints = null;
		LineRenderer lineRenderer = null;

		private void Awake()
		{
			if (!Application.isEditor) return;
			controlPoints = new List<GameObject>();
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.useWorldSpace = true;
		}

		/// <summary>
		/// Fetch all points from the spline.
		/// </summary>
		public Vector3[] Points
		{
			get
			{
				var points = new Vector3[lineRenderer.positionCount];
				lineRenderer.GetPositions(points);
				return points;
			}
		}

		private void Update()
		{
			if (!Application.isEditor) return;
			if (!pointPrefab) return;

			lineRenderer.startColor = color;
			lineRenderer.endColor = color;
			lineRenderer.startWidth = width;
			lineRenderer.endWidth = width;

			// Ensure there are at least 3 points
			if (pointCount < 3)
				pointCount = 3;

			// If there are less points than we specified
			if (pointCount > controlPoints.Count)
				for (int i = controlPoints.Count; i <= pointCount; i++)
					controlPoints.Add(Instantiate(pointPrefab, transform));

			// If there are more than specified
			if (pointCount < controlPoints.Count)
			{
				for (int i = controlPoints.Count - 1; i >= pointCount; i--)
				{
					var point = controlPoints[i];
					controlPoints.RemoveAt(i);
					DestroyImmediate(point);
				}
			}

			// Ensure density is at least 2
			if (density < 2) density = 2;
			lineRenderer.positionCount = density * (controlPoints.Count - 2);

			for (int i = 0; i < controlPoints.Count - 2; i++)
			{
				if (!controlPoints[i] || !controlPoints[i + 1] || !controlPoints[i + 2])
					return;

				// determine control points of segment
				var p0 = 0.5f * (controlPoints[i].transform.position + controlPoints[i + 1].transform.position);
				var p1 = controlPoints[i + 1].transform.position;
				var p2 = 0.5f * (controlPoints[i + 1].transform.position + controlPoints[i + 2].transform.position);

				// set spline curve points
				var step = 1f / density;

				// last point should reach p2
				if (i == controlPoints.Count - 3)
					step = 1f / (density - 1f);

				for (int j = 0; j < density; j++)
				{
					var t = j * step;
					var position = (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;
					lineRenderer.SetPosition(j + i * density, position);
				}
			}
		}
	}
}
