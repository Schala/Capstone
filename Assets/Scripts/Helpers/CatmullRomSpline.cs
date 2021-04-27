using System;
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

	[Serializable]
	public class SplinePoint
	{
		public Vector3 Position;
		public float Length;

		public SplinePoint(Vector3 pos)
		{
			Position = pos;
			Length = 0f;
		}
	}

	/// <summary>
	/// Implementation of a 3D Catmull-Rom spline
	/// </summary>
	public class CatmullRomSpline : MonoBehaviour
	{
		public bool looped = false;
		public List<SplinePoint> points;
		public Transform startTrigger = null;
		public Transform endTrigger = null;

		/// <summary>
		/// Add a new point to the spline.
		/// </summary>
		public void AddPoint()
		{
			if (points == null)
			{
				points = new List<SplinePoint>
				{
					new SplinePoint(transform.position + Vector3.right),
					new SplinePoint(transform.position + new Vector3(2f, 0f, 0f)),
					new SplinePoint(transform.position + new Vector3(3f, 0f, 0f)),
					new SplinePoint(transform.position + new Vector3(4f, 0f, 0f))
				};
			}
			else
			{
				var lastPosition = points[points.Count - 1].Position;
				points.Add(new SplinePoint(lastPosition + Vector3.right));
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
		/// Get the point along the spline, or alternatively the forward vector, relative to `t`.
		/// </summary>
		public SplinePoint GetPoint(float t, bool getForward = false)
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

			float q0, q1, q2, q3;
			if (getForward)
			{
				q0 = -3f * tt + 4f * t - 1f;
				q1 = 9f * tt - 10f * t;
				q2 = -9f * tt + 8f * t + 1f;
				q3 = 3f * tt - 2f * t;
			}
			else
			{
				q0 = -ttt + 2f * tt - t;
				q1 = 3f * ttt - 5f * tt + 2f;
				q2 = -3f * ttt + 4f * tt + t;
				q3 = ttt - tt;
			}

			float tx = 0.5f * (points[p0].Position.x * q0 + points[p1].Position.x * q1 + points[p2].Position.x * q2 + points[p3].Position.x * q3);
			float ty = 0.5f * (points[p0].Position.y * q0 + points[p1].Position.y * q1 + points[p2].Position.y * q2 + points[p3].Position.y * q3);
			float tz = 0.5f * (points[p0].Position.z * q0 + points[p1].Position.z * q1 + points[p2].Position.z * q2 + points[p3].Position.z * q3);

			return new SplinePoint(new Vector3(tx, ty, tz));
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
				case Axis.X: lowest = points[0].Position.x; break;
				case Axis.Y: lowest = points[0].Position.y; break;
				case Axis.Z: lowest = points[0].Position.z; break;
			}

			// compare to see if subsequent points have a lower value
			for (int i = 0; i < points.Count; i++)
			{
				switch (axis)
				{
					case Axis.X:
						if (points[i].Position.x < lowest) lowest = points[i].Position.x;
						break;
					case Axis.Y:
						if (points[i].Position.y < lowest) lowest = points[i].Position.z;
						break;
					case Axis.Z:
						if (points[i].Position.z < lowest) lowest = points[i].Position.z;
						break;
				}
			}

			// apply that value to every point
			for (int i = 0; i < points.Count; i++)
			{
				var point = points[i];
				switch (axis)
				{
					case Axis.X: point.Position.x = lowest; break;
					case Axis.Y: point.Position.y = lowest; break;
					case Axis.Z: point.Position.z = lowest; break;
				}
				points[i] = point;
			}
		}

		/// <summary>
		/// Reshape the spline into a circle of the specified radius.
		/// </summary>
		public void Circle(float radius)
		{
			if (!looped) return;

			for (int i = 0; i < points.Count; i++)
				points[i] = new SplinePoint(new Vector3
				{
					x = radius * Mathf.Sin((float)i / points.Count * Mathf.PI * 2f),
					y = points[i].Position.y,
					z = radius * Mathf.Cos((float)i / points.Count * Mathf.PI * 2f)
				});
		}

		/// <summary>
		/// Get the length approximation of a segment, given its index in the list.
		/// </summary>
		public float GetLength(int point)
		{
			if (point > points.Count) return 0f;

			float length = 0f;
			var prev = GetPoint(point);
			SplinePoint newPoint;

			for (float t = 0f; t < 1f; t += 0.005f)
			{
				newPoint = GetPoint(point + t);
				length = Mathf.Sqrt(Mathf.Pow(newPoint.Position.x - prev.Position.x, 2) + Mathf.Pow(newPoint.Position.y - prev.Position.y, 2) +
					Mathf.Pow(newPoint.Position.z - prev.Position.z, 2));
				prev = newPoint;
			}

			return length;
		}

		/*public float GetNormalisedOffset(float p)
		{
			int i = 0;

		}*/
	}
}
