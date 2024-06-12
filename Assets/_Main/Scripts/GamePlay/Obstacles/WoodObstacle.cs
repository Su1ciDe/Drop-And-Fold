using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public class WoodObstacle : BaseObstacle
	{
		public override ObstacleType ObstacleType { get; set; } = ObstacleType.Attached;

		[Title("Wood Obstacle")]
		[SerializeField] private int destroyCount = 2;

		private int currentDestroyCount;

		private void Awake()
		{
			currentDestroyCount = destroyCount;
		}

		public override void OnFold()
		{
			//

			currentDestroyCount--;
			if (currentDestroyCount <= 0)
			{
				RemoveObstacle();
			}
		}

		public override void RemoveObstacle()
		{
			base.RemoveObstacle();

			//TODO: polish
			Destroy(gameObject);
		}
	}
}