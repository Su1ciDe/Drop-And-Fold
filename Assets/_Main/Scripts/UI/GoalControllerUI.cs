using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class GoalControllerUI : MonoBehaviour
	{
		[SerializeField] private GoalUI goalUIPrefab;
		[SerializeField] private HorizontalLayoutGroup goalParent;

		private readonly List<GoalUI> goalUIs = new List<GoalUI>();

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			if (GoalManager.Instance)
			{
				OnLevelUnloaded();
			}
		}

		private void OnLevelLoaded()
		{
			foreach (var goal in GoalManager.Instance.GoalDictionary.Values)
			{
				var goalUI = Instantiate(goalUIPrefab, goalParent.transform);
				goalUI.Setup(goal);
				goalUI.OnComplete += OnGoalUICompleted;

				goalUIs.Add(goalUI);
			}
		}

		private void OnLevelUnloaded()
		{
			foreach (var goalUI in goalUIs)
			{
				goalUI.OnComplete -= OnGoalUICompleted;
				Destroy(goalUI.gameObject);
			}

			goalUIs.Clear();
		}

		private void OnGoalUICompleted(GoalUI goalUI)
		{
			goalUI.OnComplete -= OnGoalUICompleted;

			goalUIs.Remove(goalUI);
		}
	}
}