using System.Collections;
using Fiber.UI;
using GamePlay.Shapes;
using UnityEngine;

namespace Utilities
{
	public class LoadingAnimation : MonoBehaviour
	{
		[SerializeField] private Transform cellsParent;
		[SerializeField] private LoadingShapeCell middleCell;
		[SerializeField] private LoadingShapeCell[] neighbourCells;

		[Space]
		[SerializeField] private Transform moveInPoint;
		[SerializeField] private Transform moveOutPoint;

		private bool isFold;
		
		private void OnEnable()
		{
			if (LoadingPanelController.Instance)
			{
				LoadingPanelController.Instance.OnLoadingFinished += OnLoadingFinished;
			}
		}

		private void OnDisable()
		{
			if (LoadingPanelController.Instance)
			{
				LoadingPanelController.Instance.OnLoadingFinished -= OnLoadingFinished;
			}

			StopAllCoroutines();
		}

		private void OnLoadingFinished()
		{
		}

		private IEnumerator PlayAnimation()
		{
			yield return null;
			
			middleCell.Fold(neighbourCells);

			for (var i = 0; i < neighbourCells.Length; i++)
			{
			}
		}
	}
}