using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.AudioSystem;
using Fiber.Managers;
using Fiber.Utilities;
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

		public bool IsBusy { get; set; } = false;

		[Title("References")]
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshRenderer[] eyeLidMeshRenderers;
		[SerializeField] private Collider col;
		[Space]
		[SerializeField] private TrailRenderer trail;

		private ShapeCell currentShapeCellUnder;
		private GridCell currentGridCellUnder;

		public static readonly float SIZE = 1;
		private const float PLACE_SPEED = 15;
		public static float FOLD_DURATION = .25f;
		private const float DESTROY_DURATION = .25f;
		private const string SEPARATOR_TAG = "Separator";

		public static event UnityAction<ColorType, int, Vector3> OnFoldComplete; //ColorType colorType, int foldCount, Vector3 foldPosition

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
			cellToPlace.CurrentShapeCell = this;
			IsBusy = true;
			transform.DOMove(cellToPlace.transform.position, PLACE_SPEED).SetSpeedBased().SetEase(Ease.InQuint).OnComplete(() =>
			{
				AudioManager.Instance.PlayAudio(AudioName.Place);

				transform.DOPunchScale(0.25f * Vector3.one, .2f, 1);
				// Check folding
				if (isActiveAndEnabled)
					StartCoroutine(CheckFold());

				col.enabled = true;
				IsBusy = false;
				trail.emitting = false;
			});
		}

		private IEnumerator CheckFold()
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

			foreach (var neighbourCell in neighbours)
			{
				var neighbourGridCell = Grid.Instance.GetCell(neighbourCell.Coordinates);
				neighbourGridCell.CurrentShapeCell = null;
				neighbourCell.IsBusy = true;
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

		private IEnumerator Fold(ShapeCell[] neighbours, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return neighbours[i].FoldTo(transform.position, i).WaitForCompletion();
			}
		}

		private Tween FoldTo(Vector3 position, int index)
		{
			var middlePoint = (position + transform.position) / 2f;
			var separator = ObjectPooler.Instance.Spawn(SEPARATOR_TAG, middlePoint, Quaternion.identity);
			transform.SetParent(separator.transform);

			var dir = (position - transform.position).normalized;
			var dirCrossed = Vector3.Cross(dir, Vector3.forward);

			return separator.transform.DORotate(180 * dirCrossed, FOLD_DURATION).SetDelay(0.1f).SetEase(Ease.Linear).OnComplete(() =>
			{
				AudioManager.Instance.PlayAudio(AudioName.Fold).SetPitch(1 + index * 0.2f);
				HapticManager.Instance.PlayHaptic(0.3f, .4f, FOLD_DURATION);

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
			Coordinates = coordinates;
			ColorType = colorType;

			col.enabled = true;

			var mat = GameManager.Instance.ColorDataSO.ColorDatas[ColorType].Material;
			meshRenderer.material = mat;
			foreach (var lidMeshRenderer in eyeLidMeshRenderers)
				lidMeshRenderer.material = mat;
		}

		public void SetupShape(ColorType colorType, Vector2Int coordinates)
		{
			ColorType = colorType;

			ShapeCoordinates = coordinates;
			col.enabled = false;

			var mat = GameManager.Instance.ColorDataSO.ColorDatas[ColorType].Material;
			meshRenderer.material = mat;
			foreach (var lidMeshRenderer in eyeLidMeshRenderers)
				lidMeshRenderer.material = mat;
		}

		#endregion
	}
}