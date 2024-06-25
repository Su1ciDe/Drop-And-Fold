using System.Collections;
using Fiber.UI;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Player;
using GamePlay.DeckSystem;
using GamePlay.GridSystem.GridBoosters;
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
			Bomb.OnSpawn -= BombTutorial;

			Unsub();
		}

		private void OnLevelStarted()
		{
			if (LevelManager.Instance.LevelNo.Equals(1))
			{
				Level1Tutorial();
			}

			if (LevelManager.Instance.LevelNo.Equals(9))
			{
				Level9Tutorial();
			}
			
			if (PlayerPrefs.GetInt(PlayerPrefsNames.TUTORIAL_BOMB, 0) == 0)
			{
				Bomb.OnSpawn += BombTutorial;
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

			if (TutorialUI.Instance)
			{
				tutorialUI.HideHand();
				tutorialUI.HideText();
				tutorialUI.HideFocus();
				tutorialUI.HideDragToMove();
			}
		}

		#region Level1 Tutorial

		private bool isLeft = true;
		private Coroutine waitForNewDeckCoroutine;

		private void Level1Tutorial()
		{
			isLeft = true;

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

		#region Level 9 Tutorial

		private void Level9Tutorial()
		{
			
		}

		#endregion
		
		#region Bomb Tutorial

		private void BombTutorial(Bomb bomb)
		{
			tutorialUI.ShowFocus(bomb.transform.position, Helper.MainCamera);
			tutorialUI.ShowText("A Bomb Will Appear When You Fold 4!");

			StartCoroutine(WaitPause());
			return;

			IEnumerator WaitPause()
			{
				yield return new WaitForSeconds(0.4f);
				Time.timeScale = 0;

				yield return new WaitForSecondsRealtime(2);

				Time.timeScale = 1;

				tutorialUI.HideFocus();
				tutorialUI.HideText();

				PlayerPrefs.SetInt(PlayerPrefsNames.TUTORIAL_BOMB, 1);
				Bomb.OnSpawn -= BombTutorial;
			}
		}

		#endregion
	}
}