using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Fiber.Managers;
using GamePlay.Player;
using GamePlay.Shapes;
using GamePlay.DeckSystem;
using Managers;
using Utilities;
using TriInspector;
using Grid = GamePlay.GridSystem.Grid;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField] public LevelType LevelType { get; private set; }
		[field: SerializeField] public int LevelTypeArgument { get; private set; }

		[Title("References")]
		[SerializeField] private Grid grid;
		public Grid Grid => grid;

		[SerializeField] private Deck deck;
		public Deck Deck => deck;

		[SerializeField] private GoalManager goalManager;
		public GoalManager GoalManager => goalManager;

		public int CurrentMoveCount => currentMoveCount;
		private int currentMoveCount;

		private int currentTime;
		private readonly WaitForSeconds waitTimer = new WaitForSeconds(0.5f);
		private Coroutine timerCoroutine;

		public event UnityAction<int> OnTimerTick;
		public event UnityAction<int> OnMoveCountChange;

		private void OnEnable()
		{
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelLose += OnLevelLost;
			ShapeCell.OnFoldComplete += OnFoldComplete;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelLose -= OnLevelLost;
			ShapeCell.OnFoldComplete -= OnFoldComplete;
		}

		private void OnDestroy()
		{
			PlayerInputs.OnMouseDown -= OnFirstTouch;
		}

		private void OnLevelWon()
		{
			if (timerCoroutine is not null)
				StopCoroutine(timerCoroutine);
		}

		private void OnLevelLost()
		{
			if (timerCoroutine is not null)
				StopCoroutine(timerCoroutine);
		}

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
			if (LevelType == LevelType.Timer)
			{
				SetupTimer();
			}
			else if (LevelType == LevelType.MoveCount)
			{
				SetupMoveCount();
			}
		}

		#region MoveCount

		private void SetupMoveCount()
		{
			currentMoveCount = LevelTypeArgument;
			Shape.OnPlace += OnShapePlaced;

			OnMoveCountChange?.Invoke(currentMoveCount);
		}

		private void OnShapePlaced(Shape shape)
		{
			currentMoveCount--;
			OnMoveCountChange?.Invoke(currentMoveCount);
		}

		private void OnFoldComplete(ColorType colorType, int amount, Vector3 position)
		{
			if (LevelType == LevelType.MoveCount)
			{
				if (foldCompleteCoroutine is not null)
				{
					StopCoroutine(foldCompleteCoroutine);
					foldCompleteCoroutine = null;
				}

				foldCompleteCoroutine = StartCoroutine(OnFoldCompleteCoroutine());
			}
		}

		private Coroutine foldCompleteCoroutine;

		private IEnumerator OnFoldCompleteCoroutine()
		{
			yield return null;

			if (currentMoveCount > 0) yield break;

			do
			{
				yield return waitTimer;
			} while (Grid.Instance.IsAnyCellBusy());

			yield return null;
			yield return new WaitUntil(() => !Grid.Instance.IsAnyCellBusy());

			LevelManager.Instance.Lose();
		}

		#endregion

		#region Timer

		private void SetupTimer()
		{
			currentTime = LevelTypeArgument;

			PlayerInputs.OnMouseDown += OnFirstTouch;
		}

		private void OnFirstTouch(Vector3 mousePosition)
		{
			PlayerInputs.OnMouseDown -= OnFirstTouch;

			StartTimer();
		}

		private void StartTimer()
		{
			timerCoroutine = StartCoroutine(TimerCoroutine());
		}

		private IEnumerator TimerCoroutine()
		{
			OnTimerTick?.Invoke(currentTime);

			while (currentTime > 0)
			{
				yield return waitTimer;

				currentTime--;
				OnTimerTick?.Invoke(currentTime);
			}

			if (currentTime <= 0)
			{
				LevelManager.Instance.Lose();
			}
		}

		#endregion

		public void Setup(LevelType levelType, int levelTypeArgument)
		{
			LevelType = levelType;
			LevelTypeArgument = levelTypeArgument;
		}
	}
}