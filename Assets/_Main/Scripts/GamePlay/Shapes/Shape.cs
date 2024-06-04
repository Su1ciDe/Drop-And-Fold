using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace GamePlay.Shapes
{
	public class Shape : MonoBehaviour
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] private List<ShapeCell> shapeCells = new List<ShapeCell>();
		[SerializeField, ReadOnly] private float width, height;

		public void AddShapeCell(ShapeCell shapeCell)
		{
			shapeCells.Add(shapeCell);
		}

		public void Setup(float _width, float _height)
		{
			width = _width;
			height = _height;
		}
	}
}