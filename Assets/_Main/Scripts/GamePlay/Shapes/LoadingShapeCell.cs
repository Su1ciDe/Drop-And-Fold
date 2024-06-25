using System.Collections;
using UnityEngine;

namespace GamePlay.Shapes
{
	public class LoadingShapeCell : ShapeCell
	{
		private Vector3 startingPos;
		private Quaternion startingRot;

		private void Awake()
		{
			startingPos = transform.localPosition;
			startingRot = transform.localRotation;
		}

		public IEnumerator Fold(ShapeCell[] cells)
		{
			yield return StartCoroutine(Fold(cells, cells.Length, false));
		}

		public void ResetPosition()
		{
			transform.localPosition = startingPos;
			transform.localRotation = startingRot;
		}
	}
}