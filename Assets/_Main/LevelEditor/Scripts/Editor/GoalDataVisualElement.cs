using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor
{
	public class GoalDataVisualElement : VisualElement
	{
		public GoalDataVisualElement()
		{
			var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

			// Color enum
			var colorEnum = new EnumField { name = "enum_Color", label = "Color", style = { flexGrow = 1, width = .6f} };
			colorEnum.Init(ColorType.None);

			// Amount
			var goalAmount = new UnsignedIntegerField
			{
				name = "uintField_GoalAmount",
				label = "Gaol Amount",
				style = { flexGrow = 1, width = .4f },
				value = 1,
				maxLength = 2
			};

			root.Add(colorEnum);
			root.Add(goalAmount);

			Add(root);
		}
	}
}