using System.Collections;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Shapes;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace UI
{
	public class GoalUI : MonoBehaviour
	{
		[SerializeField] private GoalIconUI goalIcon;
		[SerializeField] private TextMeshProUGUI txtGoal;

		private Goal goal;
		private int currentAmount;

		private const float ICON_MOVE_DURATION = .75F;
		private const float DELAY = .05F;
		private const string ICON_TAG = "GoalIcon";

		public event UnityAction<GoalUI> OnComplete;

		private void OnEnable()
		{
			ShapeCell.OnFoldComplete += OnFoldCompleted;
		}

		private void OnDisable()
		{
			ShapeCell.OnFoldComplete -= OnFoldCompleted;
		}

		private void OnDestroy()
		{
			txtGoal.transform.DOKill();
		}

		private void OnFoldCompleted(ColorType colorType, int count, Vector3 pos)
		{
			if (goal.ColorType != ColorType.None)
				if (goal.ColorType != colorType)
					return;

			if (isActiveAndEnabled)
				StartCoroutine(WaitGoalUpdate());
			return;

			IEnumerator WaitGoalUpdate()
			{
				yield return null;
				
				var amount = goal.Amount - goal.CurrentAmount;

				pos = Helper.MainCamera.WorldToScreenPoint(pos);

				for (int i = 0; i < count; i++)
				{
					var icon = ObjectPooler.Instance.Spawn(ICON_TAG, pos).GetComponent<GoalIconUI>();
					icon.Setup(colorType);
					icon.transform.SetParent(UIManager.Instance.transform);
					icon.transform.DOMove(transform.position, ICON_MOVE_DURATION).SetDelay(i * DELAY).SetEase(Ease.InBack).OnComplete(() =>
					{
						txtGoal.transform.DOComplete();
						txtGoal.transform.DOPunchScale(0.9f * Vector3.one, 0.2f, 2, 0.5f);
						ChangeGoalText(amount);

						ObjectPooler.Instance.Release(icon.gameObject, ICON_TAG);
						HapticManager.Instance.PlayHaptic(.5f, 0);

						if (amount <= 0)
						{
							ShapeCell.OnFoldComplete -= OnFoldCompleted;
							OnComplete?.Invoke(this);
							gameObject.SetActive(false);
						}
					});
				}
			}
		}

		private void ChangeGoalText(int goalAmount)
		{
			currentAmount = Mathf.Clamp(goalAmount, 0, int.MaxValue);
			txtGoal.SetText(goalAmount.ToString());
		}

		public void Setup(Goal _goal)
		{
			goal = _goal;
			goalIcon.Setup(goal.ColorType);
			ChangeGoalText(goal.Amount);
		}
	}
}