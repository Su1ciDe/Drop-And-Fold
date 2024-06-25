using System.Collections;
using DG.Tweening;
using Fiber.UI;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
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
		[SerializeField] private Transform point;
		[SerializeField] private Transform moveInPoint;
		[SerializeField] private Transform moveOutPoint;

		private bool isFold;

		private const ColorType COLOR_TYPE = ColorType.None;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			OnLoadingStarted();
		}

		private void OnEnable()
		{
			if (LoadingPanelController.Instance)
			{
				LoadingPanelController.OnLoadingFinished += OnLoadingFinished;
			}
		}

		private void OnDisable()
		{
			if (LoadingPanelController.Instance)
			{
				LoadingPanelController.OnLoadingFinished -= OnLoadingFinished;
			}

			StopAllCoroutines();
		}

		private void OnLoadingStarted()
		{
			StartCoroutine(PlayAnimation());
		}

		private void OnLoadingFinished()
		{
			Destroy(gameObject);
		}

		private IEnumerator PlayAnimation()
		{
			while (isActiveAndEnabled)
			{
				var r = COLOR_TYPE.RandomItem();
				var mat = GameManager.Instance.ColorDataSO.ColorDatas[r].Material;
				middleCell.SetupMaterials(mat);
				for (var i = 0; i < neighbourCells.Length; i++)
				{
					neighbourCells[i].SetupMaterials(mat);
					neighbourCells[i].gameObject.SetActive(true);
				}

				yield return cellsParent.DOMove(point.position, .25f).SetEase(Ease.OutBack).SetLink(cellsParent.gameObject).WaitForCompletion();
				yield return middleCell.Fold(neighbourCells);
				yield return cellsParent.DOMove(moveOutPoint.position, .25f).SetEase(Ease.InBack).WaitForCompletion();

				cellsParent.position = moveInPoint.position;
				for (var i = 0; i < neighbourCells.Length; i++)
				{
					neighbourCells[i].transform.SetParent(cellsParent);
					neighbourCells[i].ResetPosition();
				}
			}
		}
	}
}