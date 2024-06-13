using UnityEngine;

namespace Utilities
{
	public static class Directions
	{
		public static Vector2Int Up = new Vector2Int(0, -1);
		public static Vector2Int Right = new Vector2Int(1, 0);
		public static Vector2Int Down = new Vector2Int(0, 1);
		public static Vector2Int Left = new Vector2Int(-1, 0);

		public static readonly Vector2Int[] AllDirections = { Up, Right, Down, Left };
	}
}