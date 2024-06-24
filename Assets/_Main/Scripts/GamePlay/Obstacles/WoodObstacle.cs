using System.Collections.Generic;
using DG.Tweening;
using Fiber.Utilities;
using Fiber.Utilities.Extensions;
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
			if (destroyCount.Equals(1))
			{
				Damage(false);
			}
		}

		private void OnDestroy()
		{
			transform.DOKill();
		}

		public override void OnFold()
		{
			Damage();

			currentDestroyCount--;
			if (currentDestroyCount <= 0)
			{
				RemoveObstacle();
			}
		}

		private void Damage(bool playEffects = true)
		{
			if (playEffects)
			{
				transform.DOPunchPosition(0.075f * Vector3.one, .15f, 25);
				ParticlePooler.Instance.Spawn(PARTICLE_TAG, transform.position);
			}

			if (currentDestroyCount <= 0) return;

			for (var i = 0; i < fractures.Count; i++)
			{
				var fracture = fractures[i];
				var direction = fracture.transform.position - transform.position;
				fracture.transform.localPosition += direction * Random.Range(0.01f, 0.05f);
				fracture.transform.eulerAngles += new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5));
			}

			for (int j = 0; j < fractures.Count / currentDestroyCount; j++)
			{
				var fracture = fractures.PickRandomItem();
				fracture.gameObject.SetActive(false);
				fractures.Remove(fracture);
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