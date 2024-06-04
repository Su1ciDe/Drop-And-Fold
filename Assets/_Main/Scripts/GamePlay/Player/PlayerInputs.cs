using UnityEngine;

namespace GamePlay.Player
{
	public class PlayerInputs : MonoBehaviour
	{
		public bool CanInput { get; set; }

		private void Update()
		{
			if (!CanInput) return;
		}
	}
}