using UnityEngine;
using AYellowpaper.SerializedCollections;
using Utilities;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "ColorData", menuName = "Drop N Fold/Color Data", order = 0)]
	public class ColorDataSO : ScriptableObject
	{
		public SerializedDictionary<ColorType, Material> ColorData = new SerializedDictionary<ColorType, Material>();
	}
}