using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor
{
	public class RandomizerColorDataVisualElement : VisualElement
	{
		public RandomizerColorDataVisualElement()
		{
			var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

			// Color enum
			var colorEnum = new EnumField { name = "enum_RandomizerColor", label = "Color", style = { flexGrow = 1, width = .5f } };
			colorEnum.Init(ColorType.None);

			// Amount
			var goalAmount = new SliderInt
			{
				name = "slider_RandomizerColorPercentage",
				label = "Percentage",
				style = { flexGrow = 1, width = .5f },
				value = 50,
				lowValue = 0,
				highValue = 100,
				showInputField = true,
			};

			root.Add(colorEnum);
			root.Add(goalAmount);

			Add(root);
		}
	}
}