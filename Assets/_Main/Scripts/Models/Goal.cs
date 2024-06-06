using System;
using Utilities;

namespace Models
{
	[Serializable]
	public class Goal
	{
		public ColorType ColorType;
		public int Amount;

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
	}
}