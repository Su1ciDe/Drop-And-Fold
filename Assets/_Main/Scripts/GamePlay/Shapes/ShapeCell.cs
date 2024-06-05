using DG.Tweening;
using Fiber.Managers;
using GamePlay.DeckSystem;
using GamePlay.GridSystem;
using TriInspector;
using UnityEngine;
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

		public const float PLACE_SPEED = 15;
		public static readonly float SIZE = 1;

		public void Place()
		{
			var gridCell = Grid.Instance.GetCell(currentShapeCellUnder.Coordinates);

			int yAbove = gridCell.Coordinates.y;
			GridCell cellAbove = null;
			while (cellAbove is null)
			{
				cellAbove = Grid.Instance.TryToGetCell(gridCell.Coordinates.x, --yAbove);
				if (cellAbove.CurrentShapeCell)
				{
					cellAbove = null;
					continue;
				}

				if (yAbove < 0)
				{
					// Fail
					LevelManager.Instance.Lose();

					break;
				}
			}

			Coordinates = cellAbove.Coordinates;
			cellAbove.CurrentShapeCell = this;
			IsBusy = true;
			transform.DOMove(cellAbove.transform.position, PLACE_SPEED).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() =>
			{
				IsBusy = false;

				// TODO: Check folding
			});
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