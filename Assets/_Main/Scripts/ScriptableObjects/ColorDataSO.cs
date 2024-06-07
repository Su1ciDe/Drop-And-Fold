using UnityEngine;
using AYellowpaper.SerializedCollections;
using Utilities;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "ColorData", menuName = "Drop N Fold/Color Data", order = 0)]
	public class ColorDataSO : ScriptableObject
	{
		[System.Serializable]
		public class ColorData
		{
			public Material Material;
			public Sprite Sprite;
		}

		public SerializedDictionary<ColorType, ColorData> ColorDatas = new SerializedDictionary<ColorType, ColorData>();
	}
}