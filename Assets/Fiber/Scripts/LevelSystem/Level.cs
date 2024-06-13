using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Fiber.Managers;
using GamePlay.Player;
using GamePlay.DeckSystem;
using GamePlay.Shapes;
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
		private readonly WaitForSeconds waitForSecond = new WaitForSeconds(1);
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
			StartCoroutine(OnFoldCompleteCoroutine());
		}

		private IEnumerator OnFoldCompleteCoroutine()
		{
			yield return null;

			if (currentMoveCount <= 0)
			{
				LevelManager.Instance.Lose();
			}
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
				yield return waitForSecond;

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