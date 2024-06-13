using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.AudioSystem;
using Fiber.LevelSystem;
using Fiber.Utilities.Extensions;
using GamePlay.Obstacles;
using GamePlay.GridSystem;
using GamePlay.DeckSystem;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.Shapes
{
	[SelectionBase]
	public class ShapeCell : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public ColorType ColorType { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }
		[field: SerializeField, ReadOnly] public BaseObstacle CurrentObstacle { get; set; }

		public bool IsBusy { get; private set; } = false;

		[Title("References")]
		[SerializeField] private MeshRenderer[] meshRenderers;
		[SerializeField] private Collider col;
		[Space]
		[SerializeField] private TrailRenderer trail;
		[Space]
		[SerializeField] private FaceController faceController;

		private ShapeCell currentShapeCellUnder;
		private GridCell currentGridCellUnder;

		public static readonly float SIZE = 1;
		public static float FOLD_DURATION = .25f;
		protected const float PLACE_SPEED = 15;
		protected const float DESTROY_DURATION = .25f;
		protected const string SEPARATOR_TAG = "Separator";

		private const float SQUASH_AMOUNT = .15f;
		private const float SQUASH_MOVE_AMOUNT = .2f;
		private const float SQUASH_DURATION = .2f;

		public static event UnityAction<ColorType, int, Vector3> OnFoldComplete; //ColorType colorType, int foldCount, Vector3 foldPosition

		private void OnEnable()
		{
			LevelManager.OnLevelLose += OnLevelLost;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLose -= OnLevelLost;
		}

		private void OnLevelLost()
		{
			StopAllCoroutines();
		}

		public void Place()
		{
			GridCell gridCell = null;

			if (currentShapeCellUnder)
			{
				gridCell = Grid.Instance.GetCell(currentShapeCellUnder.Coordinates);
			}
			else if (currentGridCellUnder)
			{
				gridCell = currentGridCellUnder;
			}

			if (!gridCell) return;

			var yAbove = gridCell.Coordinates.y;
			GridCell cellAbove = null;
			while (cellAbove is null)
			{
				if (yAbove < 0)
				{
					// Fail
					LevelManager.Instance.Lose();

					return;
				}

				cellAbove = Grid.Instance.TryToGetCell(gridCell.Coordinates.x, yAbove);
				if (cellAbove?.CurrentShapeCell)
				{
					cellAbove = null;
				}

				yAbove--;
			}

			if (cellAbove)
				Drop(cellAbove);
		}

		public void Drop(GridCell cellToPlace)
		{
			AudioManager.Instance.PlayAudio(AudioName.Pop3).SetRandomPitch(1.1f, 1.5f);

			trail.emitting = true;

			Coordinates = cellToPlace.Coordinates;
			if (CurrentObstacle)
				CurrentObstacle.Coordinates = Coordinates;

			cellToPlace.CurrentShapeCell = this;
			IsBusy = true;
			transform.DOMove(cellToPlace.transform.position, PLACE_SPEED).SetSpeedBased().SetEase(Ease.InQuint).OnComplete(() =>
			{
				AudioManager.Instance.PlayAudio(AudioName.Place);

				// feedbacks
				var height = Grid.Instance.GridCells.GetLength(1);
				for (int i = Grid.Instance.GridCells.GetLength(1) - 1; i > Coordinates.y; i--)
				{
					// squash
					var shapeCellUnder = Grid.Instance.TryToGetCell(Coordinates.x, i).CurrentShapeCell;
					shapeCellUnder.faceController.Blink(1 / (SQUASH_DURATION * 2f), SQUASH_DURATION * 2);
					if (shapeCellUnder.IsBusy) continue;
					shapeCellUnder.transform.DOComplete();
					shapeCellUnder.transform.DOMoveY(-SQUASH_MOVE_AMOUNT - SQUASH_AMOUNT * (height - i) + shapeCellUnder.transform.position.y, SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
					shapeCellUnder.transform.DOScaleY(1f - SQUASH_AMOUNT, SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
				}

				transform.DOMoveY(-SQUASH_MOVE_AMOUNT - SQUASH_AMOUNT * (height - Coordinates.y) + transform.position.y, SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
				transform.DOPunchScale(SQUASH_AMOUNT * Vector3.one, SQUASH_DURATION, 1).OnComplete(() =>
				{
					IsBusy = false;
					// Check folding
					if (isActiveAndEnabled && !CurrentObstacle && StateManager.Instance.CurrentState == GameState.OnStart)
						CheckFold();
				});

				col.enabled = true;
				trail.emitting = false;
			});
		}

		public void CheckFold()
		{
			StartCoroutine(CheckFoldCoroutine());
		}

		private IEnumerator CheckFoldCoroutine()
		{
			var pos = transform.position;
			var currentCell = Grid.Instance.GetCell(Coordinates);

			var neighbours = Grid.Instance.GetSameNeighbours(currentCell);
			var tempNeighbours = neighbours;
			yield return new WaitUntil(() => !tempNeighbours.Any(x => x.IsBusy));
			IsBusy = true;
			yield return null;
			yield return new WaitUntil(() => !Grid.Instance.IsRearranging);
			yield return null;

			if (!currentCell.CurrentShapeCell || currentCell.CurrentShapeCell != this)
			{
				currentCell = Grid.Instance.GetCell(Coordinates);
				if (!currentCell.CurrentShapeCell)
				{
					IsBusy = false;
					yield break;
				}
			}

			neighbours = Grid.Instance.GetSameNeighbours(currentCell).ToArray();

			if (!neighbours.Any())
			{
				IsBusy = false;
				yield break;
			}

			var obstacles = new List<BaseObstacle>();
			foreach (var neighbourCell in neighbours)
			{
				var neighbourGridCell = Grid.Instance.GetCell(neighbourCell.Coordinates);
				neighbourGridCell.CurrentShapeCell = null;
				neighbourCell.IsBusy = true;

				var neighbourObstacles = Grid.Instance.GetObstacleNeighbours(neighbourGridCell);
				foreach (var obstacle in neighbourObstacles)
				{
					obstacles.AddIfNotContains(obstacle);
				}
			}

			for (var i = 0; i < obstacles.Count; i++)
			{
				obstacles[i].OnFold();
			}

			var count = neighbours.Count();
			yield return Fold((ShapeCell[])neighbours, count);

			OnFoldComplete?.Invoke(ColorType, count + 1, pos);

			// destroy neighbour cells and this cell
			foreach (var shapeCell in neighbours)
			{
				currentCell.CurrentShapeCell = null;
				shapeCell.transform.DOScale(0, DESTROY_DURATION).SetEase(Ease.OutBack);
				transform.DOScale(0, DESTROY_DURATION).SetEase(Ease.OutBack).OnComplete(() =>
				{
					Destroy(shapeCell.gameObject);
					Destroy(gameObject);
				});
			}
		}

		public IEnumerator Fold(ShapeCell[] neighbours, int count, bool feedback = true)
		{
			for (int i = 0; i < count; i++)
			{
				yield return neighbours[i].FoldTo(transform.position, i, feedback).WaitForCompletion();
			}
		}

		private Tween FoldTo(Vector3 position, int index, bool feedback = true)
		{
			var middlePoint = (position + transform.position) / 2f;
			var separator = ObjectPooler.Instance.Spawn(SEPARATOR_TAG, middlePoint, Quaternion.identity);
			transform.SetParent(separator.transform);

			var dir = (position - transform.position).normalized;
			var dirCrossed = Vector3.Cross(dir, Vector3.forward);

			return separator.transform.DORotate(180 * dirCrossed, FOLD_DURATION).SetDelay(0.1f).SetEase(Ease.Linear).OnComplete(() =>
			{
				if (feedback)
				{
					AudioManager.Instance.PlayAudio(AudioName.Fold).SetPitch(1 + index * 0.2f);
					HapticManager.Instance.PlayHaptic(0.3f, .4f, FOLD_DURATION);
				}

				ObjectPooler.Instance.Release(separator, SEPARATOR_TAG);
				IsBusy = false;
			});
		}

		public ShapeCell GetShapeCellUnder()
		{
			if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100, Deck.ShapeCellLayerMask))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out ShapeCell shapeCell))
				{
					currentShapeCellUnder = shapeCell;
					currentGridCellUnder = null;
					return shapeCell;
				}
			}

			return null;
		}

		public GridCell GetGridCellUnder()
		{
			if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100, Deck.GridCellLayerMask))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out GridCell gridCell))
				{
					currentGridCellUnder = Grid.Instance.GetCell(gridCell.X, Grid.Instance.GridCells.GetLength(1) - 1);
					currentShapeCellUnder = null;
					return gridCell;
				}
			}

			return null;
		}

		#region Setup

		public void SetupGrid(Vector2Int coordinates, ColorType colorType)
		{
			ColorType = colorType;
			Coordinates = coordinates;

			col.enabled = true;

			SetupMaterials(GameManager.Instance.ColorDataSO.ColorDatas[ColorType].Material);
		}

		public void SetupShape(ColorType colorType, Vector2Int coordinates)
		{
			ColorType = colorType;
			ShapeCoordinates = coordinates;

			col.enabled = false;

			SetupMaterials(GameManager.Instance.ColorDataSO.ColorDatas[ColorType].Material);
		}

		public void SetupMaterials(Material material)
		{
			foreach (var meshRenderer in meshRenderers)
				meshRenderer.material = material;
		}

		#endregion
	}
}