using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using Fiber.LevelSystem;
using Fiber.Utilities;
using Fiber.Utilities.Extensions;
using GamePlay.GridSystem.GridBoosters;
using GamePlay.Obstacles;
using GamePlay.Shapes;
using LevelEditor;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.DeckSystem
{
	public class Deck : Singleton<Deck>
	{
		public Shape CurrentShape { get; private set; }

		[Title("Properties")]
		[SerializeField, ReadOnly] private List<Shape> shapes = new List<Shape>();
		public List<Shape> Shapes => shapes;

		[Title("References")]
		[SerializeField] private Transform spawnPoint;

		[Title("Prefabs")]
		[SerializeField] private Shape shapePrefab;
		[SerializeField] private ShapeCell shapeCellPrefab;

		private readonly Queue<Shape> shapeQueue = new Queue<Shape>();

		public static LayerMask ShapeCellLayerMask;
		public static LayerMask GridCellLayerMask;

		private void Awake()
		{
			ShapeCellLayerMask = LayerMask.GetMask("ShapeCell");
			GridCellLayerMask = LayerMask.GetMask("GridCell");
		}

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelLose += OnLevelLost;
			Shape.OnPlace += OnShapePlaced;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelLose -= OnLevelLost;
			Shape.OnPlace -= OnShapePlaced;
		}

		private void OnLevelStarted()
		{
			LoadShapes();

			SpawnShape();
		}

		private void OnLevelLost()
		{
			StopAllCoroutines();
		}

		private void OnShapePlaced(Shape shape)
		{
			CurrentShape = null;

			SpawnShape();
		}

		private void SpawnShape()
		{
			if (!shapeQueue.TryDequeue(out var shape))
			{
				LoadShapes(true);
				shape = shapeQueue.Dequeue();
			}

			StartCoroutine(WaitForSpawning(shape));
		}

		private readonly WaitForSeconds waitForGrid = new WaitForSeconds(.5f);

		private IEnumerator WaitForSpawning(Shape shape)
		{
			do
			{
				yield return waitForGrid;
			} while (Grid.Instance.IsRearranging || Grid.Instance.IsAnyCellBusy());

			if (StateManager.Instance.CurrentState != GameState.OnStart) yield break;

			yield return null;

			if (LevelManager.Instance.CurrentLevel.LevelType == LevelType.MoveCount && LevelManager.Instance.CurrentLevel.LevelTypeArgument <= 0)
			{
				LevelManager.Instance.Lose();
				yield break;
			}

			shape.gameObject.SetActive(true);
			shape.transform.position = spawnPoint.position;
			shape.transform.DOLocalMove(Vector3.zero, .25f).SetEase(Ease.OutBack).OnComplete(() => { CurrentShape = shape; });
		}

		private void LoadShapes(bool shuffle = false)
		{
			var tempShapes = new List<Shape>(shapes);
			if (shuffle)
				tempShapes.Shuffle();

			foreach (var shape in tempShapes)
			{
				var spawnedShape = Instantiate(shape, transform);
				shapeQueue.Enqueue(spawnedShape);
			}
		}

		#region Setup

#if UNITY_EDITOR
		public void Setup(List<DeckCellInfo[,]> deckCellInfosList)
		{
			foreach (var deckCellInfos in deckCellInfosList)
			{
				var middle = FindMiddle(deckCellInfos);
				var shape = (Shape)PrefabUtility.InstantiatePrefab(shapePrefab, transform);

				for (int y = 0; y < deckCellInfos.GetLength(1); y++)
				{
					for (int x = 0; x < deckCellInfos.GetLength(0); x++)
					{
						if (deckCellInfos[x, y].ColorType == ColorType.None) continue;

						var shapeCell = (ShapeCell)PrefabUtility.InstantiatePrefab(shapeCellPrefab, shape.transform);
						shape.ShapeCells.Add(shapeCell);

						int coorX = x - middle.firstLeft;
						int coorY = y - middle.firstTop;
						var coor = new Vector2Int(coorX, coorY);
						shapeCell.transform.localPosition = new Vector3(coorX - (middle.width / 2f - ShapeCell.SIZE / 2f), -(coorY - (middle.height / 2f - ShapeCell.SIZE / 2f)));

						shapeCell.SetupShape(deckCellInfos[x, y].ColorType, coor);

						if (deckCellInfos[x, y].Obstacle)
						{
							var obstacle = (BaseObstacle)PrefabUtility.InstantiatePrefab(deckCellInfos[x, y].Obstacle, shapeCell.transform);
							obstacle.SetupShape(coor);
							shapeCell.CurrentObstacle = obstacle;
						}
					}
				}

				// Reverse the cell list, because we need to check if it can fold from bottom first
				shape.ShapeCells.Reverse();
				shape.Setup(middle.width, middle.height);
				shape.gameObject.SetActive(false);
				shapes.Add(shape);
			}
		}

		private (float width, int firstLeft, float height, int firstTop) FindMiddle(DeckCellInfo[,] cells)
		{
			int count = cells.GetLength(0);
			int w = 0;
			int top = int.MaxValue;
			for (int x = 0; x < count; x++)
			{
				for (int y = 0; y < count; y++)
				{
					if (cells[x, y].ColorType == ColorType.None) continue;
					w++;

					if (y < top)
						top = y;
					break;
				}
			}

			int h = 0;
			int left = 999;
			for (int y = 0; y < count; y++)
			{
				for (int x = 0; x < count; x++)
				{
					if (cells[x, y].ColorType == ColorType.None) continue;
					h++;

					if (x < left)
						left = x;
					break;
				}
			}

			return (w, left, h, top);
		}
#endif

		#endregion
	}
}