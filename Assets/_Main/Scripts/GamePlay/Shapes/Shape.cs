using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using GamePlay.GridSystem;
using Lofelt.NiceVibrations;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.Shapes
{
	public class Shape : MonoBehaviour
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] public List<ShapeCell> ShapeCells = new List<ShapeCell>();
		[SerializeField] private float width, height;

		private float offset;

		private List<GridCell> touchingGridCells = new List<GridCell>();

		public static event UnityAction<Shape> OnPlace;

		private void Start()
		{
			offset = Grid.Instance.Offset.x - width * ShapeCell.SIZE / 4f;
		}

		public void Move(float xPos)
		{
			transform.position = new Vector3(Mathf.Clamp(xPos, -offset, offset), transform.position.y, transform.position.z);

			SetHighlights(false);
			touchingGridCells.Clear();

			for (var i = 0; i < ShapeCells.Count; i++)
			{
				var shapeCell = ShapeCells[i].GetShapeCellUnder();
				GridCell gridCell;
				if (shapeCell)
					gridCell = Grid.Instance.TryToGetCell(shapeCell.Coordinates);
				else
				{
					gridCell = ShapeCells[i].GetGridCellUnder();
					gridCell = Grid.Instance.GetCell(gridCell.X, Grid.Instance.GridCells.GetLength(1) - 1);
				}

				if (gridCell)
					touchingGridCells.AddIfNotContains(gridCell);
			}

			SetHighlights(true);
		}

		private void SetHighlights(bool show)
		{
			for (var i = 0; i < touchingGridCells.Count; i++)
			{
				var x = touchingGridCells[i].X;
				var y = touchingGridCells[i].Y;
				for (int j = 0; j <= y; j++)
				{
					if (show)
						Grid.Instance.GetCell(x, j).ShowHighlight();
					else
						Grid.Instance.GetCell(x, j).HideHighlight();
				}
			}
		}

		public void Place()
		{
			if (touchingGridCells is null || touchingGridCells.Count <= 0) return;

			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.RigidImpact);

			SetHighlights(false);
			var highestCell = FindHighestCell();
			if (highestCell?.Y >= height)
			{
				// snap to grid
				var firstCell = ShapeCells[0];
				var firstGridCell = touchingGridCells[0];

				var posX = (firstGridCell.transform.position - firstCell.transform.localPosition).x;

				transform.position = new Vector3(posX, Grid.Instance.Offset.y + (height * ShapeCell.SIZE) / 2f + ShapeCell.SIZE / 2f, transform.position.z);

				// Place cells
				// Note: This is Bottom to Top 
				for (var i = 0; i < ShapeCells.Count; i++)
				{
					ShapeCells[i].Place();
				}
			}

			touchingGridCells.Clear();

			OnPlace?.Invoke(this);
		}

		private GridCell FindHighestCell()
		{
			if (touchingGridCells is null || touchingGridCells.Count <= 0) return null;

			var highestCell = touchingGridCells[0];
			for (var i = 1; i < touchingGridCells.Count; i++)
			{
				if (touchingGridCells[i].Y < highestCell.Y)
				{
					highestCell = touchingGridCells[i];
				}
			}

			return highestCell;
		}

		public void Setup(float _width, float _height)
		{
			width = _width;
			height = _height;
		}
	}
}