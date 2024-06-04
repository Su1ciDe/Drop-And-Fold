using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor
{
	public class CellInfo
	{
		public Vector2Int Coordinates;
		public Button Button;
		public Color Color;

		public ColorType ColorType = ColorType.None;
	}
}