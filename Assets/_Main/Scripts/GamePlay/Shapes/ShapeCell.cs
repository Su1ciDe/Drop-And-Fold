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
using GamePlay.DeckSystem;
using GamePlay.GridSystem;
using GamePlay.GridSystem.GridBoosters;
using Models;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.Shapes
{
	[SelectionBase]
	public class ShapeCell : Tile
	{
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }
		[field: SerializeField, ReadOnly] public ColorType ColorType { get; private set; }
		[field: SerializeField, ReadOnly] public BaseObstacle CurrentObstacle { get; set; }

		[Title("References")]
		[SerializeField] private MeshRenderer[] meshRenderers;
		[SerializeField] private Collider col;
		[Space]
		[SerializeField] private TrailRenderer trail;
		[Space]
		[SerializeField] private FaceController faceController;
		public FaceController FaceController => faceController;

		private ShapeCell currentShapeCellUnder;
		public ShapeCell CurrentShapeCellUnder => currentShapeCellUnder;
		private GridCell currentGridCellUnder;

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

			currentGridCellUnder = null;
			currentShapeCellUnder = null;
		}

		public override void Drop(GridCell cellToPlace)
		{
			trail.emitting = true;

			cellToPlace.CurrentShapeCell = this;

			base.Drop(cellToPlace);

			if (CurrentObstacle)
				CurrentObstacle.Coordinates = Coordinates;
		}

		protected override void AfterDropping()
		{
			col.enabled = true;
			trail.emitting = false;
		}

		protected override void AfterSquashing()
		{
			if (isActiveAndEnabled && !CurrentObstacle && StateManager.Instance.CurrentState == GameState.OnStart)
				CheckFold();
		}

		public void CheckFold()
		{
			StartCoroutine(CheckFoldCoroutine());
		}

		private IEnumerator CheckFoldCoroutine()
		{
			if (CurrentObstacle)
				yield return new WaitUntil(() => !CurrentObstacle);

			var pos = transform.position;
			var currentCell = Grid.Instance.GetCell(Coordinates);

			var neighbours = Grid.Instance.GetSameNeighbours(currentCell);
			var tempNeighbours = new List<ShapeCell>(neighbours);
			if (tempNeighbours.Count.Equals(0))
			{
				IsBusy = false;
				yield break;
			}

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
			var neighbourObstacles = Grid.Instance.GetObstacleNeighbours(Grid.Instance.GetCell(Coordinates));
			foreach (var obstacle in neighbourObstacles)
			{
				obstacles.AddIfNotContains(obstacle);
			}

			foreach (var neighbourCell in neighbours)
			{
				var neighbourGridCell = Grid.Instance.GetCell(neighbourCell.Coordinates);
				neighbourGridCell.CurrentShapeCell = null;
				neighbourGridCell.CurrentTile = null;
				neighbourCell.IsBusy = true;

				neighbourObstacles = Grid.Instance.GetObstacleNeighbours(neighbourGridCell);
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
			yield return null;

			OnFoldComplete?.Invoke(ColorType, count + 1, pos);

			// destroy neighbour cells and this cell
			foreach (var shapeCell in neighbours)
				shapeCell.transform.DOScale(0, DESTROY_DURATION).SetEase(Ease.InBack).OnComplete(() => Destroy(shapeCell.gameObject));

			transform.DOScale(0, DESTROY_DURATION).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
			currentCell.CurrentShapeCell = null;
			currentCell.CurrentTile = null;

			// Check if plant a bomb
			if (count + 1 >= 4)
			{
				var bomb = ObjectPooler.Instance.Spawn("Bomb", currentCell.transform.position).GetComponent<Bomb>();
				bomb.Place(currentCell.Coordinates);
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

		public void Blast()
		{
			IsBusy = false;
			Grid.Instance.GetCell(Coordinates).CurrentTile = null;
			Grid.Instance.GetCell(Coordinates).CurrentShapeCell = null;

			OnFoldComplete?.Invoke(ColorType, 1, transform.position);

			Destroy(gameObject);
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