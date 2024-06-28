using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Fiber.Managers;
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
		[SerializeField, ReadOnly]
		private SerializedDictionary<ColorType, Goal> goalDictionary = new SerializedDictionary<ColorType, Goal>();
		public SerializedDictionary<ColorType, Goal> GoalDictionary => goalDictionary;

		private const string SMOKE_PARTICLE_TAG = "Smoke";

		private void OnEnable()
		{
			ShapeCell.OnFoldComplete += OnFoldCompleted;
		}

		private void OnDisable()
		{
			ShapeCell.OnFoldComplete -= OnFoldCompleted;
		}

		private void OnFoldCompleted(ColorType colorType, int count, Vector3 pos)
		{
			Goal goal = null;
			if (goalDictionary.ContainsKey(ColorType.None))
			{
				goal = goalDictionary[ColorType.None];
			}
			else
			{
				if (!goalDictionary.TryGetValue(colorType, out goal))
				{
					ParticlePooler.Instance.Spawn(SMOKE_PARTICLE_TAG, pos);
					return;
				}
			}

			goal.CurrentAmount = Mathf.Clamp(goal.CurrentAmount + count, 0, goal.Amount);
			if (goal.CurrentAmount >= goal.Amount)
			{
				goal.Complete();
				goalDictionary.Remove(goal.ColorType);

				// Level is completed if all the goals are finished
				if (goalDictionary.Count <= 0)
				{
					LevelManager.Instance.Win();
				}
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