using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Capstone.Entities
{
	/// <summary>
	/// The actual movement is calculated in a worker thread.
	/// </summary>
	[BurstCompile]
	public struct SwingJob : IJobParallelForTransform
	{
		public float maxRotation;
		public float speed;
		public float time;

		public void Execute(int index, TransformAccess transform)
		{
			var current = transform.rotation;
			current.x = (index % 2 == 0 ? 1 : -1) * (maxRotation * math.sin(time * speed));
			transform.rotation = current;
		}
	}

	public class SwingSystem : MonoBehaviour
	{
		[SerializeField] float maxRotation = 0.5f;
		[SerializeField] float speed = 2f;
		[SerializeField] Transform[] swings = null;
		[SerializeField] int batchCount = -1;

		TransformAccessArray transforms;
		JobHandle swingJobs;

		private void Awake() => transforms = new TransformAccessArray(swings, batchCount);

		/// <summary>
		/// Schedule our swing movements.
		/// </summary>
		private void Update()
		{
			swingJobs = new SwingJob
			{
				maxRotation = maxRotation,
				speed = speed,
				time = Time.time
			}.Schedule(transforms);
		}

		/// <summary>
		/// Do the swing movements in a worker thread.
		/// </summary>
		private void LateUpdate() => swingJobs.Complete();

		/// <summary>
		/// Clean up any lingering memory before ending.
		/// </summary>
		private void OnDestroy() => transforms.Dispose();
	}
}
