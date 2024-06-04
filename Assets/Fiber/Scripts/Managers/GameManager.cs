using Fiber.Utilities;
using ScriptableObjects;
using UnityEngine;

namespace Fiber.Managers
{
	[DefaultExecutionOrder(-1)]
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField] private ColorDataSO colorDataSO;
		public ColorDataSO ColorDataSO => colorDataSO;

		private void Awake()
		{
			Application.targetFrameRate = 60;
			Input.multiTouchEnabled = false;
			Debug.unityLogger.logEnabled = Debug.isDebugBuild;
		}
	}
}