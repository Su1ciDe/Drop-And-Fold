using System.Collections;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.AudioSystem;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.GridSystem.GridBoosters
{
	public class Bomb : GridBooster
	{
		protected override string poolName { get; set; } = "Bomb";

		[SerializeField] private float explosionDelay = 1;

		private WaitForSeconds delay;

		public static event UnityAction<Bomb> OnSpawn;

		private const string PARTICLE_TAG = "Bomb";

		private void Awake()
		{
			delay = new WaitForSeconds(explosionDelay);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (Grid.Instance)
			{
				Grid.Instance.OnRearrangingFinished += OnGridRearrangingFinished;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (Grid.Instance)
			{
				Grid.Instance.OnRearrangingFinished -= OnGridRearrangingFinished;
			}
		}

		public override void Place(Vector2Int coordinates)
		{
			base.Place(coordinates);

			OnSpawn?.Invoke(this);
		}

		private void OnGridRearrangingFinished()
		{
			Boost();
		}

		public override void Boost()
		{
			base.Boost();

			StartCoroutine(BoostCoroutine());
		}

		private IEnumerator BoostCoroutine()
		{
			IsBusy = true;

			var currentCell = Grid.Instance.GetCell(Coordinates);
			var neighboursDiagonal = Grid.Instance.GetNeighboursDiagonal(currentCell);

			transform.DOScale(1.5f, explosionDelay).SetEase(Ease.OutCubic);
			transform.DOPunchRotation(30 * Vector3.forward, explosionDelay, 20).SetEase(Ease.InQuart).SetInverted(true);

			yield return delay;

			ParticlePooler.Instance.Spawn(PARTICLE_TAG, transform.position + .25f * Vector3.back);
			HapticManager.Instance.PlayHaptic(HapticManager.AdvancedHapticType.Teleport);
			AudioManager.Instance.PlayAudio(AudioName.Bomb);

			currentCell.CurrentTile = null;

			ObjectPooler.Instance.Release(gameObject, poolName);

			foreach (var shapeCell in neighboursDiagonal)
			{
				if (!shapeCell.CurrentObstacle)
				{
					shapeCell.Blast();
				}
				else
				{
					shapeCell.CurrentObstacle.OnFold();
				}
			}

			yield return new WaitForSeconds(1f);

			IsBusy = false;
			yield return StartCoroutine(Grid.Instance.Rearrange(0));
		}
	}
}