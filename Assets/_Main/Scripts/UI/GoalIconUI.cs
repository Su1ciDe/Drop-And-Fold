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
			var colorData = GameManager.Instance.ColorDataSO.ColorDatas[colorType];
			goalImage.sprite = colorData.Sprite;
			// goalImage.color = colorData.Material.color;
		}
	}
}