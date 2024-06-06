using System.Collections;
using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Shapes;
using LevelEditor;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace GamePlay.GridSystem
{
	public class Grid : Singleton<Grid>
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] private GridCellMatrix gridCells;
		public GridCellMatrix GridCells => gridCells;
		[SerializeField, ReadOnly] private Vector2 offset;
		public Vector2 Offset => offset;

		public bool IsRearranging { get; set; }

		[Title("Grid Settings")]
		[SerializeField] private Vector2 cellSize = new Vector2Int(1, 1);
		[SerializeField] private float xSpacing = 0;
		[SerializeField] private float ySpacing = 0;
		[SerializeField] private GridCell cellPrefab;
		[SerializeField] private ShapeCell shapeCellPrefab;

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			ShapeCell.OnFoldComplete +=OnFoldComplete;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
		}

		private void OnLevelLoaded()
		{
		}

		private void OnFoldComplete(int count)
		{
			if (rearrangeCoroutine is not null)
				StopCoroutine(rearrangeCoroutine);

			rearrangeCoroutine = StartCoroutine(Rearrange());
		}

		private Coroutine rearrangeCoroutine;
		public IEnumerator Rearrange()
		{
			IsRearranging = true;
			yield return null;
			for (int y = gridCells.GetLength(1) - 1; y >= 0; y--)
			{
				for (int x = 0; x < gridCells.GetLength(0); x++)
				{
					var shapeCell = gridCells[x, y].CurrentShapeCell; 
					if (!shapeCell) continue;
					yield return new WaitUntil(() => !shapeCell.IsBusy);

				}
			}

			IsRearranging = false;
			rearrangeCoroutine = null;
		}

		#region Neighbours

		public List<GridCell> GetNeighbours(GridCell currentCell)
		{
			var neighbourList = new List<GridCell>();

			// Up
			if (currentCell.Y - 1 >= 0)
				neighbourList.Add(gridCells[currentCell.X, currentCell.Y - 1]);
			// Right
			if (currentCell.X + 1 < GridCells.GetLength(0))
				neighbourList.Add(gridCells[currentCell.X + 1, currentCell.Y]);
			// Down
			if (currentCell.Y + 1 < gridCells.GetLength(1))
				neighbourList.Add(gridCells[currentCell.X, currentCell.Y + 1]);
			// Left 
			if (currentCell.X - 1 >= 0)
				neighbourList.Add(gridCells[currentCell.X - 1, currentCell.Y]);

			return neighbourList;
		}

		public IEnumerable<ShapeCell> GetSameNeighbours(GridCell currentCell)
		{
			// Up
			if (currentCell.Y - 1 >= 0 && currentCell.CurrentShapeCell.ColorType == gridCells[currentCell.X, currentCell.Y - 1].CurrentShapeCell?.ColorType)
				yield return gridCells[currentCell.X, currentCell.Y - 1].CurrentShapeCell;
			// Right
			if (currentCell.X + 1 < GridCells.GetLength(0) && currentCell.CurrentShapeCell.ColorType == gridCells[currentCell.X + 1, currentCell.Y].CurrentShapeCell?.ColorType)
				yield return gridCells[currentCell.X + 1, currentCell.Y].CurrentShapeCell;
			// Down
			if (currentCell.Y + 1 < gridCells.GetLength(1) && currentCell.CurrentShapeCell.ColorType == gridCells[currentCell.X, currentCell.Y + 1].CurrentShapeCell?.ColorType)
				yield return gridCells[currentCell.X, currentCell.Y + 1].CurrentShapeCell;
			// Left 
			if (currentCell.X - 1 >= 0 && currentCell.CurrentShapeCell.ColorType == gridCells[currentCell.X - 1, currentCell.Y].CurrentShapeCell?.ColorType)
				yield return gridCells[currentCell.X - 1, currentCell.Y].CurrentShapeCell;
		}

		#endregion

		#region Setup

#if UNITY_EDITOR
		public void Setup(CellInfo[,] cellInfos)
		{
			var width = cellInfos.GetLength(0);
			var height = cellInfos.GetLength(1);
			gridCells = new GridCellMatrix(width, height);

			var xOffset = (cellSize.x * width + xSpacing * (width - 1)) / 2f - cellSize.x / 2f;
			var yOffset = (cellSize.y * height + ySpacing * (height - 1)) / 2f - cellSize.y / 2f;
			offset = new Vector2(xOffset, yOffset);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var cellInfo = cellInfos[x, y];
					var cell = (GridCell)PrefabUtility.InstantiatePrefab(cellPrefab, transform);
					cell.Setup(x, y, cellSize);
					cell.gameObject.name = x + " - " + y;
					var pos = new Vector3(x * (cellSize.x + xSpacing) - xOffset, -y * (cellSize.y + ySpacing) + yOffset);
					cell.transform.localPosition = pos;
					gridCells[x, y] = cell;

					if (cellInfo.ColorType != ColorType.None)
					{
						var shapeCell = (ShapeCell)PrefabUtility.InstantiatePrefab(shapeCellPrefab, cell.transform);
						shapeCell.SetupGrid(new Vector2Int(x, y), cellInfo.ColorType);
						cell.CurrentShapeCell = shapeCell;
					}
				}
			}
		}
#endif

		[System.Serializable]
		public class GridCellArray
		{
			public GridCell[] Cells;
			public GridCell this[int index]
			{
				get => Cells[index];
				set => Cells[index] = value;
			}

			public GridCellArray(int index0)
			{
				Cells = new GridCell[index0];
			}
		}

		[System.Serializable]
		public class GridCellMatrix
		{
			public GridCellArray[] Arrays;
			public GridCell this[int x, int y]
			{
				get => Arrays[x][y];
				set => Arrays[x][y] = value;
			}

			public GridCellMatrix(int index0, int index1)
			{
				Arrays = new GridCellArray[index0];
				for (int i = 0; i < index0; i++)
					Arrays[i] = new GridCellArray(index1);
			}

			public int GetLength(int dimension)
			{
				return dimension switch
				{
					0 => Arrays.Length,
					1 => Arrays[0].Cells.Length,
					_ => 0
				};
			}
		}

		#endregion

		public GridCell GetCell(int x, int y)
		{
			return gridCells[x, y];
		}

		public GridCell GetCell(Vector2Int coordinates)
		{
			return GetCell(coordinates.x, coordinates.y);
		}

		public GridCell TryToGetCell(int x, int y)
		{
			if (x >= 0 && x < GridCells.GetLength(0) && y >= 0 && y < GridCells.GetLength(1))
				return gridCells[x, y];

			return null;
		}

		public GridCell TryToGetCell(Vector2Int coordinates)
		{
			return TryToGetCell(coordinates.x, coordinates.y);
		}

		public Vector3 GetCellPosition(int x, int y)
		{
			return gridCells[x, y].transform.position;
		}

		public Vector3 GetCellPosition(Vector2Int coordinates)
		{
			return GetCellPosition(coordinates.x, coordinates.y);
		}
	}
}