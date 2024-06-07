using System;
using UnityEngine.Events;
using Utilities;

namespace Models
{
	[Serializable]
	public class Goal
	{
		public ColorType ColorType;
		public int Amount;

		public int CurrentAmount { get; set; } = 0;

		public event UnityAction OnComplete;

		public Goal(ColorType colorType, int amount)
		{
			ColorType = colorType;
			Amount = amount;
		}

		public Goal()
		{
			ColorType = ColorType.None;
			Amount = 0;
		}

		public void Complete()
		{
			OnComplete?.Invoke();
		}
	}
}