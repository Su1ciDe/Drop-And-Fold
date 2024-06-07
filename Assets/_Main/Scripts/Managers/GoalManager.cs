using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Fiber.Utilities;
using GamePlay.Shapes;
using Models;
using TriInspector;
using UnityEngine;
using Utilities;

namespace Managers
{
	public class GoalManager : Singleton<GoalManager>
	{
		[SerializeField, ReadOnly] private SerializedDictionary<ColorType, Goal> goalDictionary = new SerializedDictionary<ColorType, Goal>();
		public SerializedDictionary<ColorType, Goal> GoalDictionary => goalDictionary;

		private void OnEnable()
		{
			ShapeCell.OnFoldComplete += OnFoldCompleted;
		}

		private void OnDisable()
		{
			ShapeCell.OnFoldComplete -= OnFoldCompleted;
		}

		private void OnFoldCompleted(ColorType colorType, int count)
		{
			if (!goalDictionary.TryGetValue(colorType, out var goal)) return;
			
			goal.CurrentAmount = Mathf.Clamp(goal.CurrentAmount + count, 0, goal.Amount);
			if (goal.CurrentAmount >= goal.Amount)
			{
				goal.Complete();
			}
		}

		#region Setup

		public void Setup(List<Goal> goalsSetup)
		{
			foreach (var goal in goalsSetup)
			{
				goalDictionary.Add(goal.ColorType, goal);
			}
		}

		#endregion
	}
}