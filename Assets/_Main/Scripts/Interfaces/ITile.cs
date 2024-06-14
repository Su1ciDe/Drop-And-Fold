using GamePlay.GridSystem;
using UnityEngine;

namespace Interfaces
{
	public interface ITile
	{
		public Vector2Int Coordinates { get; set; }
		public bool IsBusy { get; set; }

		public void Drop(GridCell cellToPlace);
		
		public Transform GetTransform();
	}
}