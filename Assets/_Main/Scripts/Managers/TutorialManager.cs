using System.Collections;
using Fiber.Managers;
using Fiber.UI;
using Fiber.Utilities;
using GamePlay.DeckSystem;
using GamePlay.Player;
using UnityEngine;
using Grid = GamePlay.GridSystem.Grid;

namespace Managers
{
	public class TutorialManager : Singleton<TutorialManager>
	{
		public bool Predicate { get; private set; } = true;

		[SerializeField] private GameObject moveHereIndicator;

		private TutorialUI tutorialUI => TutorialUI.Instance;

		private void Awake()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;

			Unsub();
		}

		private void OnLevelStarted()
		{
			if (LevelManager.Instance.LevelNo.Equals(1))
			{
				Level1Tutorial();
			}
		}

		private void OnLevelWon()
		{
			OnLevelUnloaded();
		}

		private void OnLevelUnloaded()
		{
			Unsub();
		}

		private void Unsub()
		{
			if (waitForNewDeckCoroutine is not null)
				StopCoroutine(waitForNewDeckCoroutine);

			Predicate = true;

			PlayerInputs.OnDrag -= Level1OnDrag;
			PlayerInputs.OnMouseUp -= Level1OnMouseUp;

			tutorialUI.HideHand();
			tutorialUI.HideText();
			tutorialUI.HideFocus();
			tutorialUI.HideDragToMove();
		}

		#region Level1 Tutorial

		private bool isLeft = true;
		private Coroutine waitForNewDeckCoroutine;

		private void Level1Tutorial()
		{
			tutorialUI.ShowDragToMove();
			Predicate = false;

			ShowMoveHere(GetLeftOrRight());

			PlayerInputs.OnDrag += Level1OnDrag;
			PlayerInputs.OnMouseUp += Level1OnMouseUp;
		}

		private void Level1OnMouseUp(Vector3 mousePos)
		{
			PlayerInputs.OnDrag -= Level1OnDrag;
			PlayerInputs.OnMouseUp -= Level1OnMouseUp;

			//
			moveHereIndicator.SetActive(false);
			isLeft = !isLeft;

			waitForNewDeckCoroutine = StartCoroutine(WaitForNewDeck());
		}

		private IEnumerator WaitForNewDeck()
		{
			yield return null;
			yield return new WaitUntil(() => Deck.Instance.CurrentShape);
			yield return null;

			Level1Tutorial2();
		}

		private void Level1Tutorial2()
		{
			ShowMoveHere(GetLeftOrRight());

			PlayerInputs.OnDrag += Level1OnDrag;
			PlayerInputs.OnMouseUp += Level1OnMouseUp;
		}

		private void ShowMoveHere(float xPos)
		{
			moveHereIndicator.SetActive(true);
			moveHereIndicator.transform.position = new Vector3(xPos, Deck.Instance.transform.position.y);
		}

		private float GetLeftOrRight()
		{
			return isLeft ? Grid.Instance.GridCells[0, 0].transform.position.x : Grid.Instance.GridCells[Grid.Instance.GridCells.GetLength(0) - 1, 0].transform.position.x;
		}

		private void Level1OnDrag(Vector3 mousePos)
		{
			Predicate = Deck.Instance.CurrentShape?.ShapeCells[0].ColorType == Deck.Instance.CurrentShape?.ShapeCells[0].CurrentShapeCellUnder?.ColorType;
		}

		#endregion
	}
}