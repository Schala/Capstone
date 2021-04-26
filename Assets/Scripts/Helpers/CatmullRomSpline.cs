using System.Collections.Generic;
using UnityEngine;


namespace Capstone.Helpers
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	/// <summary>
	/// Implementation of a 3D Catmull-Rom spline
	/// </summary>
	public class CatmullRomSpline : MonoBehaviour
	{
		public bool looped = false;
		public List<Vector3> points;
		public Transform startTrigger = null;
		public Transform endTrigger = null;

		/// <summary>
		/// Add a new point to the spline.
		/// </summary>
		public void AddPoint()
		{
			if (points == null)
			{
				points = new List<Vector3>
				{
					transform.position + Vector3.right,
					transform.position + new Vector3(2f, 0f, 0f),
					transform.position + new Vector3(3f, 0f, 0f),
					transform.position + new Vector3(4f, 0f, 0f)
				};
			}
			else
			{
				var lastPoint = points[points.Count - 1];
				points.Add(lastPoint + Vector3.right);
			}
		}

		/// <summary>
		/// Remove the last point of the spline.
		/// </summary>
		public void RemoveLastPoint()
		{
			if (points == null || points.Count <= 4) return;
			points.RemoveAt(points.Count - 1);
		}

		/// <summary>
		/// Get the point along the spline, relative to `t`.
		/// </summary>
		public Vector3 GetPoint(float t)
		{
			int p0, p1, p2, p3;

			if (looped)
			{
				p1 = Mathf.FloorToInt(t);
				p2 = (p1 + 1) % points.Count;
				p3 = (p2 + 1) % points.Count;
				p0 = p1 >= 1 ? p1 - 1 : points.Count - 1;
			}
			else
			{
				p1 = Mathf.FloorToInt(t) + 1;
				p2 = p1 + 1;
				p3 = p2 + 1;
				p0 = p1 - 1;
			}

			t -= Mathf.Floor(t); // normalise `t`
			float tt = t * t;
			float ttt = tt * t;

			float q0 = -ttt + 2f * tt - t;
			float q1 = 3f * ttt - 5f * tt + 2f;
			float q2 = -3f * ttt + 4f * tt + t;
			float q3 = ttt - tt;

			float tx = 0.5f * (points[p0].x * q0 + points[p1].x * q1 + points[p2].x * q2 + points[p3].x * q3);
			float ty = 0.5f * (points[p0].y * q0 + points[p1].y * q1 + points[p2].y * q2 + points[p3].y * q3);
			float tz = 0.5f * (points[p0].z * q0 + points[p1].z * q1 + points[p2].z * q2 + points[p3].z * q3);

			return new Vector3(tx, ty, tz);
		}

		/// <summary>
		/// Flatten the spline along an axis
		/// </summary>
		public void Flatten(Axis axis)
		{
			float lowest = 0f;

			// grab the axis of the first point
			switch (axis)
			{
				case Axis.X: lowest = points[0].x; break;
				case Axis.Y: lowest = points[0].y; break;
				case Axis.Z: lowest = points[0].z; break;
			}

			// compare to see if subsequent points have a lower value
			for (int i = 0; i < points.Count; i++)
			{
				switch (axis)
				{
					case Axis.X:
						if (points[i].x < lowest) lowest = points[i].x;
						break;
					case Axis.Y:
						if (points[i].y < lowest) lowest = points[i].z;
						break;
					case Axis.Z:
						if (points[i].z < lowest) lowest = points[i].z;
						break;
				}
			}

			// apply that value to every point
			for (int i = 0; i < points.Count; i++)
			{
				var point = points[i];
				switch (axis)
				{
					case Axis.X: point.x = lowest; break;
					case Axis.Y: point.y = lowest; break;
					case Axis.Z: point.z = lowest; break;
				}
				points[i] = point;
			}
		}
	}
}
