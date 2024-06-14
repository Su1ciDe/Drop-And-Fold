using System.Collections.Generic;
using Fiber.Utilities;
using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public class WoodObstacle : BaseObstacle
	{
		public override ObstacleType ObstacleType { get; set; } = ObstacleType.Attached;

		[Title("Wood Obstacle")]
		[SerializeField] private int destroyCount = 2;
		[Space]
		[SerializeField] private List<GameObject> fractures;

		private int currentDestroyCount;

		private const string PARTICLE_TAG = "WoodObstacle";

		private void Awake()
		{
			currentDestroyCount = destroyCount;
		}

		public override void OnFold()
		{
			//
			ParticlePooler.Instance.Spawn(PARTICLE_TAG, transform.position);

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