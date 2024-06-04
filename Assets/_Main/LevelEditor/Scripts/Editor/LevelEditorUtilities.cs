using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
	public static class LevelEditorUtilities
	{
		public static void LoadAssetPreview(VisualElement element, Object asset, object owner)
		{
			EditorCoroutineUtility.StartCoroutine(LoadAssetPreviewCoroutine(element, asset), owner);
		}

		private static IEnumerator LoadAssetPreviewCoroutine(VisualElement element, Object asset)
		{
			var instanceId = asset.GetInstanceID();
			var tex = AssetPreview.GetAssetPreview(asset);
			while (AssetPreview.IsLoadingAssetPreview(instanceId))
			{
				yield return null;
				tex = AssetPreview.GetAssetPreview(asset);
			}

			element.style.backgroundImage = tex;
		}
	}
}