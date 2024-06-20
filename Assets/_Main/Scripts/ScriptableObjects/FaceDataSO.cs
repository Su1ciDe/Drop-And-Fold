using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "FaceData", menuName = "Drop N Fold/Face Data", order = 1)]
	public class FaceDataSO : ScriptableObject
	{
		[SerializeField] private float transitionDuration = 1.0f;

		[SerializeField] private float minEyeWaitDuration = 1f;
		[SerializeField] private float maxEyeWaitDuration = 2.5f;

		[SerializeField] private float minEyeAnimationSpeed = 2f;
		[SerializeField] private float maxEyeAnimationSpeed = 2.5f;

		[SerializeField] private float minBlinkWaitDuration = 2f;
		[SerializeField] private float maxBlinkWaitDuration = 4f;
		[SerializeField] private float minBlinkSpeed = 0.1f;
		[SerializeField] private float maxBlinkSpeed = 0.3f;

		public float TransitionDuration => transitionDuration;

		public float MinEyeWaitDuration => minEyeWaitDuration;
		public float MaxEyeWaitDuration => maxEyeWaitDuration;

		public float MinEyeAnimationSpeed => minEyeAnimationSpeed;
		public float MaxEyeAnimationSpeed => maxEyeAnimationSpeed;

		public float MinBlinkWaitDuration => minBlinkWaitDuration;
		public float MaxBlinkWaitDuration => maxBlinkWaitDuration;

		public float MinBlinkSpeed => minBlinkSpeed;
		public float MaxBlinkSpeed => maxBlinkSpeed;
	}
}