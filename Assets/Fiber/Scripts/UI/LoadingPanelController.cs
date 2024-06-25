using DG.Tweening;
using Fiber.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Fiber.UI
{
	public class LoadingPanelController : SingletonPersistent<LoadingPanelController>
	{
		public static bool IsLoading = false;

		[Header("General Variables")]
		[SerializeField] private float minLoadingDuration = 4f;
		[SerializeField] private float maxLoadingDuration = 5f;
		[SerializeField] private Ease loadingEase;

		[Header("References")]
		[SerializeField] private Image imgFillBar;
		[SerializeField] private GameObject loadingPanelParent;
		[Space]
		[SerializeField] private Image imgBackground;
		[SerializeField] private Image imgLoadingScreen;
		[SerializeField] private Image imgLoadingScreenTitle;

		public static event UnityAction OnLoadingStarted;
		public static event UnityAction OnLoadingFinished;

		protected override void Awake()
		{
			base.Awake();

			imgFillBar.fillAmount = 0f;
			loadingPanelParent.SetActive(true);
			IsLoading = true;

			var dur = Random.Range(minLoadingDuration, maxLoadingDuration);

			OnLoadingStarted?.Invoke();
			imgFillBar.DOFillAmount(1f, dur).SetEase(loadingEase).SetLink(gameObject).SetTarget(gameObject).OnComplete(() =>
			{
				loadingPanelParent.SetActive(false);
				IsLoading = false;
				OnLoadingFinished?.Invoke();
			});
		}

		public void SetLoadingScreen(Sprite background, Sprite loadingScreen, Sprite loadingScreenTitle)
		{
			if (background)
				imgBackground.sprite = background;
			if (loadingScreen)
				imgLoadingScreen.sprite = loadingScreen;
			if (loadingScreenTitle)
				imgLoadingScreenTitle.sprite = loadingScreenTitle;
		}
	}
}