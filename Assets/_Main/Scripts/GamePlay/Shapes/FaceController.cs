using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace GamePlay.Shapes
{
	public class FaceController : MonoBehaviour
	{
		[SerializeField] private Animator eyesAnimator;
		[SerializeField] private List<Animator> eyelidAnimators;
		[SerializeField] private FaceDataSO faceData;

		private readonly string baseAnimationId = "base";
		private readonly string[] eyeAnimationTriggers = { "base", "top", "topRight", "topLeft", "bottom", "bottomRight", "bottomLeft" };
		private const string BLINK_ANIMATION_TRIGGER = "blink";

		private void Start()
		{
			StartCoroutine(PlayRandomEyesAnimation());
			StartCoroutine(PlayBlinkAnimation());
		}

		private IEnumerator PlayRandomEyesAnimation()
		{
			while (isActiveAndEnabled)
			{
				var randomIndex = Random.Range(0, eyeAnimationTriggers.Length);
				var randomWaitDuration = Random.Range(faceData.MinEyeWaitDuration, faceData.MaxEyeWaitDuration);
				var animationTrigger = eyeAnimationTriggers[randomIndex];
				var animationSpeed = Random.Range(faceData.MinEyeAnimationSpeed, faceData.MaxEyeAnimationSpeed);

				eyesAnimator.speed = 1f / animationSpeed;
				eyesAnimator.SetTrigger(animationTrigger);

				yield return new WaitForSeconds(faceData.TransitionDuration + randomWaitDuration);
			}
		}

		private IEnumerator PlayBlinkAnimation()
		{
			while (isActiveAndEnabled)
			{
				var blinkDuration = Random.Range(faceData.MinBlinkWaitDuration, faceData.MaxBlinkWaitDuration);
				var blinkSpeed = Random.Range(faceData.MinBlinkSpeed, faceData.MaxBlinkSpeed);
				yield return new WaitForSeconds(blinkDuration);
				for (int i = 0; i < eyelidAnimators.Count; i++)
				{
					eyelidAnimators[i].speed = 1f / blinkSpeed;
					eyelidAnimators[i].SetTrigger(BLINK_ANIMATION_TRIGGER);
				}
			}
		}
	}
}