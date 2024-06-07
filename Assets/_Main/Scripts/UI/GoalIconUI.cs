using Fiber.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI
{
	public class GoalIconUI : MonoBehaviour
	{
		[SerializeField] private Image goalImage;

		public void Setup(ColorType colorType)
		{
			goalImage.sprite = GameManager.Instance.ColorDataSO.ColorDatas[colorType].Sprite;
		}
	}
}