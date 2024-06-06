using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.GridSystem;
using GamePlay.DeckSystem;
using TriInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.Shapes
{
	public class ShapeCell : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public ColorType ColorType { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }

		public bool IsBusy { get; set; } = false;

		[Title("References")]
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private Collider col;

		private ShapeCell currentShapeCellUnder;

		public static readonly float SIZE = 1;
		private const float PLACE_SPEED = 15;
		private const float FOLD_DURATION = .5f;

		public static event UnityAction<int> OnFoldComplete; // int foldCount 

		public void Place()
		{
			var gridCell = Grid.Instance.GetCell(currentShapeCellUnder.Coordinates);

			var yAbove = gridCell.Coordinates.y;
			GridCell cellAbove = null;
			while (cellAbove is null)
			{
				yAbove--;
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
					continue;
				}
			}

			if (cellAbove)
				Drop(cellAbove);
		}

		public void Drop(GridCell cellToPlace)
		{
			Coordinates = cellToPlace.Coordinates;
			cellToPlace.CurrentShapeCell = this;
			IsBusy = true;
			transform.DOMove(cellToPlace.transform.position, PLACE_SPEED).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() =>
			{
				// Check folding
				StartCoroutine(CheckFold());

				IsBusy = false;
			});
		}

		private IEnumerator CheckFold()
		{
			var currentCell = Grid.Instance.GetCell(Coordinates);

			var neighbours = Grid.Instance.GetSameNeighbours(currentCell);
			var tempNeighbours = neighbours;
			yield return new WaitUntil(() => !tempNeighbours.Any(x => x.IsBusy));
			yield return null;
			yield return new WaitUntil(() => !Grid.Instance.IsRearranging);

			if (!currentCell.CurrentShapeCell)
			{
				IsBusy = false;
				yield break;
			}

			neighbours = Grid.Instance.GetSameNeighbours(currentCell).ToArray();

			foreach (var neighbourCell in neighbours)
			{
				var neighbourGridCell = Grid.Instance.GetCell(neighbourCell.Coordinates);
				neighbourGridCell.CurrentShapeCell = null;
				neighbourCell.IsBusy = true;
			}

			yield return Fold(neighbours);
			// TODO: destroy neighbour cells and this cell

			OnFoldComplete?.Invoke(neighbours.Count());
		}

		private IEnumerator Fold(IEnumerable<ShapeCell> neighbours)
		{
			foreach (var neighbourCell in neighbours)
			{
				yield return neighbourCell.FoldTo(transform.position).WaitForCompletion();
			}
		}

		private const string SEPARATOR_TAG = "Separator";

		private Tween FoldTo(Vector3 position)
		{
			var middlePoint = (position + transform.position) / 2f;
			var separator = ObjectPooler.Instance.Spawn(SEPARATOR_TAG, middlePoint, Quaternion.identity);
			transform.SetParent(separator.transform);

			var dir = (position - transform.position).normalized;
			var dirCrossed = Vector3.Cross(dir, Vector3.forward);

			return separator.transform.DORotate(180 * dirCrossed, FOLD_DURATION).SetEase(Ease.Linear).OnComplete(() => { ObjectPooler.Instance.Release(separator, SEPARATOR_TAG); });
		}

		public ShapeCell GetCellUnder()
		{
			if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100, Deck.ShapeCellLayerMask))
			{
				if (hit.rigidbody && hit.rigidbody.TryGetComponent(out ShapeCell shapeCell))
				{
					currentShapeCellUnder = shapeCell;
					return shapeCell;
				}
			}

			return null;
		}

		#region Setup

		public void SetupGrid(Vector2Int coordinates, ColorType colorType)
		{
			Coordinates = coordinates;
			ColorType = colorType;
			meshRenderer.material = GameManager.Instance.ColorDataSO.ColorData[ColorType];
			col.enabled = true;
		}

		public void SetupShape(ColorType colorType, Vector2Int coordinates)
		{
			ColorType = colorType;
			meshRenderer.material = GameManager.Instance.ColorDataSO.ColorData[ColorType];
			ShapeCoordinates = coordinates;
			col.enabled = false;
		}

		#endregion
	}
}