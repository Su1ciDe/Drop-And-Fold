using System.Collections;
using System.Collections.Generic;
using Fiber.Utilities.Extensions;
using GamePlay.Player;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations;

namespace GamePlay.Shapes
{
	public class FaceController : MonoBehaviour
	{
		[SerializeField] private FaceDataSO faceData;
		[SerializeField] private Animator eyesAnimator;
		[SerializeField] private List<Animator> eyelidAnimators;
		[SerializeField] private RuntimeAnimatorController[] animatorControllers;

		[Space]
		[SerializeField] private LookAtConstraint[] lookAtConstraints;

		private bool isBlinkPaused = false;

		private const string BASE_ANIMATION_ID = "base";
		private readonly string[] eyeAnimationTriggers = { "base", "top", "topRight", "topLeft", "bottom", "bottomRight", "bottomLeft" };
		private static readonly int blink = Animator.StringToHash("blink");

		private void Awake()
		{
			eyesAnimator.runtimeAnimatorController = animatorControllers.RandomItem();
			foreach (var lookAtConstraint in lookAtConstraints)
			{
				var source = new ConstraintSource { sourceTransform = Player.Player.Instance.PlayerInputs.InputEyeTarget, weight = 0 };
				lookAtConstraint.AddSource(source);
			}
		}

		private void Start()
		{
			StartCoroutine(PlayRandomEyesAnimation());
			StartCoroutine(PlayBlinkAnimation());
		}

		private void OnEnable()
		{
			PlayerInputs.OnMouseDown += OnMouseDown;
			PlayerInputs.OnMouseUp += OnMouseUp;
		}

		private void OnDisable()
		{
			PlayerInputs.OnMouseDown -= OnMouseDown;
			PlayerInputs.OnMouseUp -= OnMouseUp;
		}

		private void OnMouseDown(Vector3 pos)
		{
			for (var i = 0; i < lookAtConstraints.Length; i++)
			{
				var eyeTarget_CS = lookAtConstraints[i].GetSource(0);
				eyeTarget_CS.weight = 0;
				lookAtConstraints[i].SetSource(0, eyeTarget_CS);

				var inputEyeTarget_CS = lookAtConstraints[i].GetSource(1);
				inputEyeTarget_CS.weight = 1;
				lookAtConstraints[i].SetSource(1, inputEyeTarget_CS);
			}
		}

		private void OnMouseUp(Vector3 pos)
		{
			for (var i = 0; i < lookAtConstraints.Length; i++)
			{
				var eyeTarget_CS = lookAtConstraints[i].GetSource(0);
				eyeTarget_CS.weight = 1;
				lookAtConstraints[i].SetSource(0, eyeTarget_CS);

				var inputEyeTarget_CS = lookAtConstraints[i].GetSource(1);
				inputEyeTarget_CS.weight = 0;
				lookAtConstraints[i].SetSource(1, inputEyeTarget_CS);
			}
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
				yield return new WaitUntil(() => !isBlinkPaused);

				var blinkDuration = Random.Range(faceData.MinBlinkWaitDuration, faceData.MaxBlinkWaitDuration);
				var blinkSpeed = Random.Range(faceData.MinBlinkSpeed, faceData.MaxBlinkSpeed);

				yield return new WaitForSeconds(blinkDuration);

				for (int i = 0; i < eyelidAnimators.Count; i++)
				{
					eyelidAnimators[i].speed = 1f / blinkSpeed;
					eyelidAnimators[i].SetTrigger(blink);
				}
			}
		}

		public void Blink(float speed, float duration)
		{
			StartCoroutine(BlinkOnce(speed, duration));
		}

		private IEnumerator BlinkOnce(float speed, float duration)
		{
			isBlinkPaused = true;
			for (int i = 0; i < eyelidAnimators.Count; i++)
			{
				eyelidAnimators[i].speed = speed;
				eyelidAnimators[i].SetTrigger(blink);
			}

			yield return new WaitForSeconds(duration);

			isBlinkPaused = false;
		}
	}
}