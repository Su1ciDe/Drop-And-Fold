using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace GamePlay.Obstacles
{
	public abstract class BaseObstacle : MonoBehaviour
	{
		public abstract ObstacleType ObstacleType { get; set; }
		protected abstract GoalType goalType { get; set; }

		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }

		public static event UnityAction<GoalType, int, Vector3> OnObstacleDestroyed; //GoalType goalType, int foldCount, Vector3 foldPosition

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
			OnObstacleDestroyed?.Invoke(goalType, 1, transform.position);

			Destroy(gameObject);
		}
	}
}