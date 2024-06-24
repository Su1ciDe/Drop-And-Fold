using Models;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.GridSystem.GridBoosters
{
	public abstract class GridBooster : Tile
	{
		protected abstract string poolName { get; set; }

		public static UnityAction OnBoost;

		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		public virtual void Place(Vector2Int coordinates)
		{
			var currentCell = Grid.Instance.GetCell(coordinates);
			currentCell.CurrentTile = this;
			transform.position = currentCell.transform.position + offset;
			Coordinates = coordinates;
			gameObject.SetActive(true);
		}

		public virtual void Boost()
		{
		}
	}
}