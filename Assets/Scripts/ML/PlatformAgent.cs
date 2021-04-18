using Capstone.Player;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Capstone.ML
{
	/// <summary>
	/// Machine-learning functionality for platforming
	/// </summary>
	[RequireComponent(typeof(PlayerController))]
	public class PlatformAgent : Agent
	{
		float height = 0f;
		PlayerController controller = null;
		Transform startTransform = null;

		private void Awake()
		{
			controller = GetComponent<PlayerController>();
		}

		private void Start()
		{
			startTransform = transform;
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			sensor.AddObservation(transform.position);
		}

		public override void OnActionReceived(ActionBuffers actions)
		{
			controller.IsMoving = true;
			controller.Movement = Vector3.forward * actions.ContinuousActions[0];
			if (controller.Movement != Vector3.zero) transform.rotation = Quaternion.LookRotation(controller.Movement);
		}

		public override void OnEpisodeBegin()
		{
			transform.position = startTransform.position;
			transform.forward = startTransform.forward;
		}
	}
}
