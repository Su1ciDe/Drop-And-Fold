using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using GamePlay.Shapes;
using LevelEditor;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace GamePlay.DeckSystem
{
	public class Deck : MonoBehaviour
	{
		[SerializeField, ReadOnly] private List<Shape> shapes = new List<Shape>();

		[Title("References")]
		[SerializeField] private Transform spawnPoint;
		
		[Title("Prefabs")]
		[SerializeField] private Shape shapePrefab;
		[SerializeField] private ShapeCell shapeCellPrefab;

		private Queue<Shape> shapeQueue = new Queue<Shape>();

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
		}

		private void OnLevelStarted()
		{
			SpawnShapes();
		}

		private void SpawnShapes(bool shuffle = false)
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

				for (int x = 0; x < deckCellInfos.GetLength(1); x++)
				{
					for (int y = 0; y < deckCellInfos.GetLength(0); y++)
					{
						if (deckCellInfos[x, y].ColorType == ColorType.None) continue;

						var shapeCell = (ShapeCell)PrefabUtility.InstantiatePrefab(shapeCellPrefab, shape.transform);
						shape.AddShapeCell(shapeCell);

						int coorX = x - middle.firstLeft;
						int coorY = y - middle.firstTop;
						shapeCell.transform.localPosition = new Vector3(coorX - (middle.width / 2f - ShapeCell.SIZE / 2f), -(coorY - (middle.height / 2f - ShapeCell.SIZE / 2f)));
						shapeCell.SetupShape(deckCellInfos[x, y].ColorType, new Vector2Int(-coorX, -coorY));
					}
				}

				shape.Setup(middle.width, middle.height);
				shapes.Add(shape);
			}
		}

		private (float height, int firstLeft, float width, int firstTop) FindMiddle(DeckCellInfo[,] cells)
		{
			int height = 0;
			int firstLeft = 999;
			for (int y = 0; y < cells.GetLength(1); y++)
			{
				for (int x = 0; x < cells.GetLength(0); x++)
				{
					if (cells[y, x].ColorType == ColorType.None) continue;
					height++;

					if (x < firstLeft)
						firstLeft = x;
					break;
				}
			}

			int width = 0;
			int firstTop = 999;
			for (int x = 0; x < cells.GetLength(1); x++)
			{
				for (int y = 0; y < cells.GetLength(0); y++)
				{
					if (cells[y, x].ColorType == ColorType.None) continue;
					width++;

					if (y < firstTop)
						firstTop = y;
					break;
				}
			}

			return (height, firstLeft, width, firstTop);
		}
#endif

		#endregion
	}
}