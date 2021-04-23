using System;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone.Helpers
{
	[Serializable]
	public class SplineAnchor
	{
		public Vector3 position;
		public Vector3 handleAPosition;
		public Vector3 handleBPosition;
	}

	[Serializable]
	public class SplinePoint
	{
		public float t;
		public Vector3 position;
		public Vector3 forward;
		public Vector3 normal;
	}

	public class Spline : MonoBehaviour
	{
		static readonly Vector3 anchorIncrement = new Vector3(1f, 1f, 0f);

		[SerializeField] bool loop = false;
		[SerializeField] Vector3 normal = new Vector3(0f, 0f, -1f);
		[SerializeField] List<SplineAnchor> anchors = null;

		List<SplinePoint> points = null;
		float splineLength = 0f;
		float pointAmountInCurve = 0f;
		float pointAmountPerUnitInCurve = 2f;

		public List<SplineAnchor> Anchors => anchors;
		public List<SplinePoint> Points => points;
		public bool IsLoop => loop;

		private void Awake()
		{
			splineLength = GetSplineLength();

			points = new List<SplinePoint>();
			pointAmountInCurve = pointAmountPerUnitInCurve * splineLength;

			for (float t = 0f; t < 1f; t += 1f / pointAmountInCurve)
				points.Add(new SplinePoint
				{
					t = t,
					position = GetPositionAt(t),
					normal = normal
				});

			points.Add(new SplinePoint
			{
				t = 1f,
				position = GetPositionAt(1f)
			});

			UpdateForwardVectors();
		}

		/// <summary>
		/// Lerp through the points A and B, B and C, then lerp the results of both pairs.
		/// </summary>
		Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			var ab = Vector3.Lerp(a, b, t);
			var bc = Vector3.Lerp(b, c, t);

			return Vector3.Lerp(ab, bc, t);
		}

		/// <summary>
		/// Quadratically lerp through the points A, B, and C, then B, C, and D, then lerp the results of both groups.
		/// </summary>
		Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			var abc = QuadraticLerp(a, b, c, t);
			var bcd = QuadraticLerp(b, c, d, t);

			return Vector3.Lerp(abc, bcd, t);
		}

		/// <summary>
		/// Retrieve the position at `t` in the spline.
		/// </summary>
		public Vector3 GetPositionAt(float t)
		{
			if (t >= 1f) // full position
			{
				SplineAnchor a, b;
				if (loop)
				{
					a = anchors[anchors.Count - 1];
					b = anchors[0];
				}
				else
				{
					a = anchors[anchors.Count - 2];
					b = anchors[anchors.Count - 1];
				}
				return transform.position + CubicLerp(a.position, a.handleBPosition, b.handleAPosition, b.position, t);
			}
			else
			{
				int addLoop = loop ? 1 : 0;
				var tFull = t * (anchors.Count - 1 + addLoop);
				var anchorIndex = Mathf.FloorToInt(tFull);
				var tAnchor = tFull - anchorIndex;

				SplineAnchor a, b;

				if (anchorIndex < anchors.Count - 1)
				{
					a = anchors[anchorIndex];
					b = anchors[anchorIndex + 1];
				}
				else
				{
					// last index, don't link to next one or loop back
					if (loop)
					{
						a = anchors[anchors.Count - 1];
						b = anchors[0];
					}
					else
					{
						a = anchors[anchorIndex - 1];
						b = anchors[anchorIndex];
						tAnchor = 1f;
					}
				}

				return transform.position + CubicLerp(a.position, a.handleBPosition, b.handleAPosition, b.position, tAnchor);
			}
		}

		/// <summary>
		/// Retrieve the forward vector relative to `t` on the spline.
		/// </summary>
		public Vector3 GetForwardAt(float t)
		{
			var a = GetPreviousPoint(t);
			var bIndex = (points.IndexOf(a) + 1) % points.Count;
			var b = points[bIndex];

			return Vector3.Lerp(a.forward, b.forward, (t - a.t) / Mathf.Abs(a.t - b.t));
		}

		/// <summary>
		/// Retrieve the previous point on the spline, relative to `t`.
		/// </summary>
		public SplinePoint GetPreviousPoint(float t)
		{
			int prevIndex = 0;

			for (int i = 1; i < points.Count; i++)
			{
				var point = points[i];
				if (t < point.t)
					return points[prevIndex];
				else
					prevIndex++;
			}

			return points[prevIndex];
		}

		/// <summary>
		/// Get the closest point on the spline, relative to `t`.
		/// </summary>
		public SplinePoint GetClosestPoint(float t)
		{
			var closest = points[0];

			for (int i = 0; i < points.Count; i++)
				if (Mathf.Abs(t - points[i].t) < Mathf.Abs(t - closest.t))
					closest = points[i];
			return closest;
		}

		/// <summary>
		/// Retrieve the spline length, given the step size.
		/// </summary>
		public float GetSplineLength(float step = 0.01f)
		{
			var length = 0f;
			var lastPosition = GetPositionAt(0f);

			for (float t = 0f; t < 1f; t += step)
			{
				length += Vector3.Distance(lastPosition, GetPositionAt(t));
				lastPosition = GetPositionAt(t);
			}
			length += Vector3.Distance(lastPosition, GetForwardAt(1f));
			return length;
		}

		public Vector3 GetPositionAtUnits(float distance, float step = 0.01f)
		{
			var unitDistance = 0f;
			var lastPosition = GetPositionAt(0f);

			for (float t = 0f; t < 1f; t += step)
			{
				unitDistance += Vector3.Distance(lastPosition, GetPositionAt(t));
				lastPosition = GetPositionAt(t);

				if (unitDistance >= distance)
				{
					var direction = (GetPositionAt(t) - GetPositionAt(t - step)).normalized;
					return GetPositionAt(t) + direction * (distance - unitDistance);
				}
			}

			var a = anchors[0];
			var b = anchors[1];
			return CubicLerp(a.position, a.handleBPosition, b.handleAPosition, b.position, distance / splineLength);
		}

		public Vector3 GetForwardAtUnits(float distance, float step = 0.01f)
		{
			var unitDistance = 0f;
			var lastPosition = GetPositionAt(0f);

			for (float t = 0f; t < 1f; t += step)
			{
				var lastDistance = Vector3.Distance(lastPosition, GetPositionAt(t));
				unitDistance += lastDistance;
				lastPosition = GetPositionAt(t);

				if (unitDistance >= distance)
				{
					var remainingDistance = unitDistance - distance;
					return GetForwardAt(t - ((remainingDistance / lastDistance) * step));
				}
			}

			var a = anchors[0];
			var b = anchors[1];
			return CubicLerp(a.position, a.handleBPosition, b.handleAPosition, b.position, distance / splineLength);
		}

		/// <summary>
		/// Update all forward vectors.
		/// </summary>
		void UpdateForwardVectors()
		{
			// set forward vectors
			for (int i = 0; i < points.Count - 1; i++)
				points[i].forward = (points[i + 1].position - points[i].position).normalized;

			// and the final one
			if (loop)
				points[points.Count - 1].forward = points[0].forward;
			else
				points[points.Count - 1].forward = points[points.Count - 2].forward;
		}

		/// <summary>
		/// Update all point positions.
		/// </summary>
		void UpdatePoints()
		{
			if (points == null) return;

			for (int i = 0; i < points.Count; i++)
				points[i].position = GetPositionAt(points[i].t);
			UpdateForwardVectors();
		}

		/// <summary>
		/// Adds a new anchor to the spline.
		/// </summary>
		public void AddAnchor()
		{
			if (anchors == null) anchors = new List<SplineAnchor>();

			var lastAnchor = anchors[anchors.Count - 1];
			anchors.Add(new SplineAnchor
			{
				position = lastAnchor.position + anchorIncrement,
				handleAPosition = lastAnchor.handleAPosition + anchorIncrement,
				handleBPosition = lastAnchor.handleBPosition + anchorIncrement
			});
		}

		/// <summary>
		/// Remove the last anchor of the spline.
		/// </summary>
		public void RemoveLastAnchor()
		{
			if (anchors == null) anchors = new List<SplineAnchor>();
			anchors.RemoveAt(anchors.Count - 1);
		}
	}
}
