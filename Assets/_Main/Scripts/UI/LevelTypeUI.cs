using DG.Tweening;
using Fiber.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
	public class LevelTypeUI : MonoBehaviour
	{
		[SerializeField] private Image imgLevelType;
		[SerializeField] private TMP_Text txtTimer_MoveCount;
		[Space]
		[SerializeField] private Sprite timerSprite;
		[SerializeField] private Sprite moveCountSprite;

		private const float TIMER_ANIM_DURATION = .25f;

		private void Awake()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;
		}

		private void OnLevelLoaded()
		{
			txtTimer_MoveCount.SetText(LevelManager.Instance.CurrentLevel.LevelTypeArgument.ToString());
			if (LevelManager.Instance.CurrentLevel.LevelType == LevelType.Timer)
			{
				SetupTimer();
			}
			else if (LevelManager.Instance.CurrentLevel.LevelType == LevelType.MoveCount)
			{
				SetupMoveCount();
			}
		}

		private void OnLevelUnloaded()
		{
			LevelManager.Instance.CurrentLevel.OnTimerTick -= OnTimerTicked;
			LevelManager.Instance.CurrentLevel.OnMoveCountChange -= OnMoveCountUpdated;
		}

		private void SetupTimer()
		{
			LevelManager.Instance.CurrentLevel.OnTimerTick += OnTimerTicked;
		}

		private void SetupMoveCount()
		{
			LevelManager.Instance.CurrentLevel.OnMoveCountChange += OnMoveCountUpdated;
		}

		private void OnMoveCountUpdated(int moveCount)
		{
			txtTimer_MoveCount.SetText(moveCount.ToString());
		}

		private void OnTimerTicked(int time)
		{
			txtTimer_MoveCount.SetText(time.ToString());

			if (time < 10)
			{
				var sign = time % 2 == 0 ? 1 : -1;
				imgLevelType.transform.DOShakeRotation(TIMER_ANIM_DURATION, sign * 30 * Vector3.forward, 50, 1, true, ShakeRandomnessMode.Harmonic).SetLoops(2, LoopType.Restart);
				txtTimer_MoveCount.transform.DOScale(1.5f, TIMER_ANIM_DURATION).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);
				txtTimer_MoveCount.DOColor(Color.red, TIMER_ANIM_DURATION).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);

				HapticManager.Instance.PlayHaptic(HapticManager.AdvancedHapticType.Heartbeats);
			}
		}
	}
}