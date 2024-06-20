using System.Collections.Generic;
using Fiber.Managers;
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

		private GoalManager goalManager => LevelManager.Instance.CurrentLevel.GoalManager;

		private void Awake()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;

			if (GoalManager.Instance)
			{
				OnLevelUnloaded();
			}
		}

		private void OnLevelLoaded()
		{
			foreach (var goal in goalManager.GoalDictionary.Values)
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
				DestroyImmediate(goalUI.gameObject);
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