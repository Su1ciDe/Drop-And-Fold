using System.Collections;
using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Obstacles;
using GamePlay.Shapes;
using LevelEditor;
using Models;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
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

		public bool IsRearranging { get; private set; }

		[Title("Grid Settings")]
		[SerializeField] private Vector2 cellSize = new Vector2Int(1, 1);
		[SerializeField] private float xSpacing = 0;
		[SerializeField] private float ySpacing = 0;
		[SerializeField] private GridCell cellPrefab;
		[SerializeField] private ShapeCell shapeCellPrefab;
		[Title("Grid Frames")]
		[SerializeField] private GameObject gridFrameCorner_Left;
		[SerializeField] private GameObject gridFrameCorner_Right;
		[SerializeField] private GameObject gridFrameHorizontal;
		[SerializeField] private GameObject gridFrameVertical;

		public event UnityAction OnRearrangingFinished;

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			ShapeCell.OnFoldComplete += OnFoldComplete;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			ShapeCell.OnFoldComplete -= OnFoldComplete;
		}

		private void OnLevelLoaded()
		{
		}

		private void OnFoldComplete(ColorType colorType, int count, Vector3 pos)
		{
			if (rearrangeCoroutine is not null)
				StopCoroutine(rearrangeCoroutine);

			rearrangeCoroutine = StartCoroutine(Rearrange(Tile.FOLD_DURATION * (count - 1) + 0.05F));
		}

		private Coroutine rearrangeCoroutine;

		public IEnumerator Rearrange(float waitDuration)
		{
			var width = gridCells.GetLength(0);
			var height = gridCells.GetLength(1);

			IsRearranging = true;
			yield return new WaitForSeconds(waitDuration);

			for (int y = height - 1; y >= 0; y--)
			{
				for (int x = 0; x < width; x++)
				{
					var tile = gridCells[x, y].CurrentTile;

					if (tile is null) continue;
					yield return new WaitUntil(() => !tile.IsBusy);

					var yCoor = height;
					GridCell cellUnder = null;
					while (cellUnder is null)
					{
						yCoor--;
						if (yCoor < 0 || yCoor.Equals(tile.Coordinates.y))
							break;

						cellUnder = TryToGetCell(tile.Coordinates.x, yCoor);
						if (cellUnder?.CurrentShapeCell || cellUnder?.CurrentTile is not null)
						{
							cellUnder = null;
							continue;
						}
					}

					if (cellUnder && !cellUnder.Coordinates.Equals(tile.Coordinates))
					{
						var cell = GetCell(tile.Coordinates);
						cell.CurrentShapeCell = null;
						cell.CurrentTile = null;
						tile.Drop(cellUnder);
					}
				}
			}

			OnRearrangingFinished?.Invoke();

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
			for (int i = 0; i < Directions.AllDirections.Length; i++)
			{
				var shapeCell = TryToGetCell(currentCell.Coordinates + Directions.AllDirections[i])?.CurrentShapeCell;
				if (shapeCell && !shapeCell.CurrentObstacle && currentCell.CurrentShapeCell.ColorType == shapeCell.ColorType)
					yield return shapeCell;
			}
		}

		public IEnumerable<ShapeCell> GetNeighboursDiagonal(GridCell currentCell)
		{
			for (int i = 0; i < Directions.AllDirections.Length; i++)
			{
				var shapeCell = TryToGetCell(currentCell.Coordinates + Directions.AllDirections[i])?.CurrentShapeCell;
				if (shapeCell)
					yield return shapeCell;
			}

			for (int i = 0; i < Directions.AllDirections.Length; i++)
			{
				var shapeCell = TryToGetCell(currentCell.Coordinates + Directions.AllDirections[i] + Directions.AllDirections[(i + 1) % Directions.AllDirections.Length])?.CurrentShapeCell;
				if (shapeCell)
					yield return shapeCell;
			}
		}

		public IEnumerable<BaseObstacle> GetObstacleNeighbours(GridCell currentCell)
		{
			for (int i = 0; i < Directions.AllDirections.Length; i++)
			{
				var shapeCell = TryToGetCell(currentCell.Coordinates + Directions.AllDirections[i])?.CurrentShapeCell;
				if (shapeCell && shapeCell.CurrentObstacle)
					yield return shapeCell.CurrentObstacle;
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

		public bool IsAnyCellBusy()
		{
			for (int y = gridCells.GetLength(1) - 1; y >= 0; y--)
			{
				for (int x = 0; x < gridCells.GetLength(0); x++)
				{
					if (gridCells[x, y].CurrentTile is not null && gridCells[x, y].CurrentTile.IsBusy)
						return true;
				}
			}

			return false;
		}

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
						var coor = new Vector2Int(x, y);
						var shapeCell = (ShapeCell)PrefabUtility.InstantiatePrefab(shapeCellPrefab, cell.transform);
						shapeCell.SetupGrid(coor, cellInfo.ColorType);
						cell.CurrentShapeCell = shapeCell;

						if (cellInfo.Obstacle && cellInfo.Obstacle.ObstacleType == ObstacleType.Attached)
						{
							var obstacle = (BaseObstacle)PrefabUtility.InstantiatePrefab(cellInfo.Obstacle, shapeCell.transform);
							obstacle.SetupGrid(coor);
							shapeCell.CurrentObstacle = obstacle;
						}
					}
					else if (cellInfo.Obstacle && cellInfo.Obstacle.ObstacleType == ObstacleType.Independent)
					{
					}
				}
			}

			SetupFrame(xOffset, yOffset, width, height);
		}

		private void SetupFrame(float xOffset, float yOffset, int width, int height)
		{
			var leftCornerPos = new Vector3(-(xOffset + cellSize.x / 2f), -(yOffset + cellSize.y / 2f));
			var rightCornerPos = new Vector3((xOffset + cellSize.x / 2f), -(yOffset + cellSize.y / 2f));

			var leftCornerPrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridFrameCorner_Left, transform);
			leftCornerPrefab.transform.localPosition = leftCornerPos;
			var rightCornerPrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridFrameCorner_Right, transform);
			rightCornerPrefab.transform.localPosition = rightCornerPos;

			var horizontalPrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridFrameHorizontal, transform);
			horizontalPrefab.transform.localPosition = new Vector3(leftCornerPos.x + cellSize.x, leftCornerPos.y);
			horizontalPrefab.transform.localScale = new Vector3(width - 2, 1, 1);
			var verticalLeftPrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridFrameVertical, transform);
			verticalLeftPrefab.transform.localPosition = new Vector3(leftCornerPos.x, leftCornerPos.y + cellSize.y);
			verticalLeftPrefab.transform.localScale = new Vector3(1, height - 1, 1);
			var verticalRightPrefab = (GameObject)PrefabUtility.InstantiatePrefab(gridFrameVertical, transform);
			verticalRightPrefab.transform.localPosition = new Vector3(rightCornerPos.x + cellSize.x / 2f, rightCornerPos.y + cellSize.y);
			verticalRightPrefab.transform.localScale = new Vector3(1, height - 1, 1);
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
	}
}