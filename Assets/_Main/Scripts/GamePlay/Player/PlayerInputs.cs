using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.DeckSystem;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Player
{
	public class PlayerInputs : MonoBehaviour
	{
		public bool CanInput { get; set; } = true;

		public static event UnityAction<Vector3> OnMouseDown;

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelLose += OnLevelLost;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelLose -= OnLevelLost;
		}

		private void Update()
		{
			if (!CanInput) return;
			if (Input.GetMouseButtonDown(0))
			{
				OnMouseDown?.Invoke(Input.mousePosition);
				OnDown();
			}

			if (Input.GetMouseButton(0))
			{
				OnDrag();
			}

			if (Input.GetMouseButtonUp(0))
			{
				OnUp();
			}
		}

		private void OnDown()
		{
			if (!Deck.Instance.CurrentShape) return;

			GetMovePosition();
		}

		private void OnDrag()
		{
			if (!Deck.Instance.CurrentShape) return;

			GetMovePosition();
		}

		private void OnUp()
		{
			if (!Deck.Instance.CurrentShape) return;

			Deck.Instance.CurrentShape.Place();
		}

		private void GetMovePosition()
		{
			var mousePos = Input.mousePosition;
			var pos = Helper.MainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Helper.MainCamera.transform.position.z));
			Deck.Instance.CurrentShape.Move(pos.x);
		}

		private void OnLevelStarted()
		{
			CanInput = true;
		}

		private void OnLevelLost()
		{
			CanInput = false;
		}

		private void OnLevelWon()
		{
			CanInput = false;
		}
	}
}