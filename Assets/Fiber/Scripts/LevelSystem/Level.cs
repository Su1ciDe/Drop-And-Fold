using GamePlay.DeckSystem;
using TriInspector;
using UnityEngine;
using Utilities;
using Grid = GamePlay.GridSystem.Grid;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField] public LevelType LevelType { get; private set; }
		[field: SerializeField] public int LevelTypeArgument { get; private set; }

		[Title("References")]
		[SerializeField] private Grid grid;
		public Grid Grid => grid;

		[SerializeField] private Deck deck;
		public Deck Deck => deck;

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
		}

		public void Setup(LevelType levelType, int levelTypeArgument)
		{
			LevelType = levelType;
			LevelTypeArgument = levelTypeArgument;
		}
	}
}