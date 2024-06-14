using System.Collections;
using Fiber.AudioSystem;
using Fiber.Managers;
using Fiber.Utilities;
using UnityEngine;

namespace GamePlay.GridSystem.GridBoosters
{
	public class Bomb : GridBooster
	{
		protected override string poolName { get; set; } = "Bomb";

		[SerializeField] private float explosionDelay = 1;

		private WaitForSeconds delay;

		public static bool IsBombActive;

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
			IsBombActive = true;

			IsBusy = true;

			var currentCell = Grid.Instance.GetCell(Coordinates);
			var neighboursDiagonal = Grid.Instance.GetNeighboursDiagonal(currentCell);

			yield return delay;

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

			IsBombActive = false;
			IsBusy = false;
			yield return StartCoroutine(Grid.Instance.Rearrange());
			// OnBoost?.Invoke();
		}
	}
}