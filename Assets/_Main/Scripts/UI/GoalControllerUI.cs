using Fiber.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class GoalControllerUI : MonoBehaviour
	{
		[SerializeField] private HorizontalLayoutGroup goalParent;

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
		}

		private void OnLevelLoaded()
		{
		}
	}
}