using GamePlay.Shapes;
using Interfaces;
using TriInspector;
using UnityEngine;

namespace GamePlay.GridSystem
{
	[SelectionBase]
	public class GridCell : MonoBehaviour
	{
		public int X => Coordinates.x;
		public int Y => Coordinates.y;
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; private set; }
		[field: SerializeField, ReadOnly] public ShapeCell CurrentShapeCell { get; set; }
		public ITile CurrentTile { get; set; }

		[Space]
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private Material material;
		[SerializeField] private Material highlightMaterial;

		private void Awake()
		{
			CurrentTile = CurrentShapeCell;
		}

		public void Setup(int x, int y, Vector2 nodeSize)
		{
			Coordinates = new Vector2Int(x, y);
			transform.localScale = new Vector3(nodeSize.x, 1f, nodeSize.y);
		}

		public void ShowHighlight()
		{
			meshRenderer.material = highlightMaterial;
		}

		public void HideHighlight()
		{
			meshRenderer.material = material;
		}

		public void DisableModel()
		{
			meshRenderer.gameObject.SetActive(false);
		}
	}
}