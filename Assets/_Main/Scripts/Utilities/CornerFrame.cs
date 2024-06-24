using UnityEngine;

namespace Utilities
{
	public class CornerFrame : MonoBehaviour
	{
		[SerializeField] private GameObject frameL;
		[SerializeField] private GameObject frameR;

		private void Awake()
		{
			if (transform.position.x < 0)
			{
				frameL.SetActive(true);
				frameR.SetActive(false);
			}
			else
			{
				frameL.SetActive(false);
				frameR.SetActive(true);
			}
		}
	}
}