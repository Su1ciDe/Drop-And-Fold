using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.DeckSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GamePlay.Player
{
	public class PlayerInputs : MonoBehaviour
	{
		public bool CanInput { get; set; } = true;

		[SerializeField] private Transform inputEyeTarget;
		public Transform InputEyeTarget => inputEyeTarget;

		private bool isDown = false;

		private const float BACK_POS = 6;
		private const float DAMPING = 6;

		public static event UnityAction<Vector3> OnMouseDown;
		public static event UnityAction<Vector3> OnMouseUp;

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
			if (EventSystem.current.IsPointerOverGameObject()) return;
			if (!CanInput) return;
			if (Input.GetMouseButtonDown(0))
			{
				isDown = true;
				OnMouseDown?.Invoke(Input.mousePosition);
				OnDown();
			}

			if (Input.GetMouseButton(0))
			{
				OnDrag();
			}

			if (Input.GetMouseButtonUp(0))
			{
				isDown = false;
				OnMouseUp?.Invoke(Input.mousePosition);
				OnUp();
			}
		}

		private void OnDown()
		{
			if (!Deck.Instance.CurrentShape) return;

			var pos = GetMovePosition();

			Deck.Instance.CurrentShape.Move(pos.x);

			inputEyeTarget.transform.position = Vector3.Lerp(inputEyeTarget.transform.position, pos + BACK_POS * Vector3.back, Time.deltaTime * DAMPING);
		}

		private void OnDrag()
		{
			if (!Deck.Instance.CurrentShape) return;

			var pos = GetMovePosition();

			Deck.Instance.CurrentShape.Move(pos.x);

			inputEyeTarget.transform.position = Vector3.Lerp(inputEyeTarget.transform.position, pos + BACK_POS * Vector3.back, Time.deltaTime * DAMPING);
		}

		private void OnUp()
		{
			if (!Deck.Instance.CurrentShape) return;

			Deck.Instance.CurrentShape.Place();
		}

		private Vector3 GetMovePosition()
		{
			var mousePos = Input.mousePosition;
			var pos = Helper.MainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Helper.MainCamera.transform.position.z));

			return pos;
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