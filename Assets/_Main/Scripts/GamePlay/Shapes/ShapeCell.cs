using Fiber.Managers;
using TriInspector;
using UnityEngine;
using Utilities;

namespace GamePlay.Shapes
{
	public class ShapeCell : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public ColorType ColorType { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; private set; }
		[field: SerializeField, ReadOnly] public Vector2Int ShapeCoordinates { get; private set; }

		[Title("References")]
		[SerializeField] private MeshRenderer meshRenderer;

		public static readonly float SIZE = 1;

		public void SetupGrid(Vector2Int coordinates, ColorType colorType)
		{
			Coordinates = coordinates;
			ColorType = colorType;
			meshRenderer.material = GameManager.Instance.ColorDataSO.ColorData[ColorType];
		}

		public void SetupShape(ColorType colorType, Vector2Int coordinates)
		{
			ColorType = colorType;
			meshRenderer.material = GameManager.Instance.ColorDataSO.ColorData[ColorType];
			ShapeCoordinates = coordinates;
		}
	}
}