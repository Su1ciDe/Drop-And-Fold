using UnityEngine;

namespace GamePlay.Shapes
{
	public class LoadingShapeCell : ShapeCell
	{
		public void Fold(ShapeCell[] cells)
		{
			StartCoroutine(Fold(cells, cells.Length, false));
		}

		public void Unfold()
		{
		}
	}
}