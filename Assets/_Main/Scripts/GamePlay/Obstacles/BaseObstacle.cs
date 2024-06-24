using GamePlay.Shapes;
using TriInspector;
using UnityEngine;
using Grid = GamePlay.GridSystem.Grid;

namespace GamePlay.Obstacles
{
	public abstract class BaseObstacle : MonoBehaviour
	{
		public abstract ObstacleType ObstacleType { get; set; }

		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }

		public virtual void SetupGrid(Vector2Int coordinates)
		{
			Coordinates = coordinates;
		}

		public virtual void SetupShape(Vector2Int coordinates)
		{
			ShapeCoordinates = coordinates;
		}

		public abstract void OnFold();

		public virtual void RemoveObstacle()
		{
			var gridCell = Grid.Instance.TryToGetCell(Coordinates);
			if (gridCell && gridCell.CurrentTile is ShapeCell shapeCell)
			{
				shapeCell.CheckFold();
			}
		}
	}
}