using System.Collections;
using Fiber.Utilities;
using AYellowpaper.SerializedCollections;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Fiber.Managers
{
	public class HapticManager : SingletonInit<HapticManager>
	{
		[SerializeField] private bool hapticsEnabled = true;
		public bool HapticsEnabled
		{
			get => PlayerPrefs.GetInt(PlayerPrefsNames.HAPTICS_ENABLED, 1).Equals(1);
			set
			{
				PlayerPrefs.SetInt(PlayerPrefsNames.HAPTICS_ENABLED, value ? 1 : 0);
				HapticController.hapticsEnabled = value;
			}
		}

		public bool IsPlaying => HapticController.IsPlaying() || hapticMultiple is not null;

		[Header("Advanced Haptics")]
		[SerializeField] private SerializedDictionary<AdvancedHapticType, HapticClip> clips;

		private Coroutine hapticMultiple;

		protected override void Awake()
		{
			base.Awake();
			HapticsEnabled = hapticsEnabled;
		}

		/// <summary>
		/// Plays a haptic preset
		/// </summary>
		/// <param name="hapticType">Preset type</param>
		public void PlayHaptic(HapticPatterns.PresetType hapticType)
		{
			HapticPatterns.PlayPreset(hapticType);
		}

		/// <summary>
		/// Plays a single emphasis point
		/// </summary>
		/// <param name="amplitude">The amplitude of haptic, from 0.0 to 1.0</param>
		/// <param name="frequency">The frequency of haptic, from 0.0 to 1.0</param>
		public void PlayHaptic(float amplitude, float frequency)
		{
			HapticPatterns.PlayEmphasis(amplitude, frequency);
		}

		/// <summary>
		/// Plays a continuous haptic
		/// </summary>
		/// <param name="amplitude">The amplitude of haptic, from 0.0 to 1.0</param>
		/// <param name="frequency">The frequency of haptic, from 0.0 to 1.0</param>
		/// <param name="duration">Play duration in seconds</param>
		public void PlayHaptic(float amplitude, float frequency, float duration)
		{
			StopHaptics();

			HapticPatterns.PlayConstant(amplitude, frequency, duration);
		}

		/// <summary>
		/// Plays an advanced predefined haptic
		/// </summary>
		/// <param name="advancedHapticType"></param>
		public void PlayHaptic(AdvancedHapticType advancedHapticType)
		{
			StopHaptics();

			HapticController.Play(clips[advancedHapticType]);
		}

		/// <summary>
		/// Plays a predefined haptic clip
		/// </summary>
		/// <param name="hapticClip">Haptic Clip</param>
		public void PlayHaptic(HapticClip hapticClip)
		{
			StopHaptics();

			HapticController.Play(hapticClip);
		}

		/// <summary>
		/// Plays a multiple emphasis haptics at given amount of times and delay
		/// </summary>
		/// <param name="amplitude">The amplitude of haptic, from 0.0 to 1.0</param>
		/// <param name="frequency">The frequency of haptic, from 0.0 to 1.0</param>
		/// <param name="amount">How many times the haptics should play</param>
		/// <param name="delayInBetween">The time between haptics <br/><i>Note: Delays are unscaled time</i></param>
		public void PlayHapticMultiple(float amplitude, float frequency, int amount, int delayInBetween)
		{
			StopHaptics();

			hapticMultiple = StartCoroutine(HapticMultiple(amplitude, frequency, amount, delayInBetween));
		}

		private IEnumerator HapticMultiple(float amplitude, float frequency, int amount, int delayInBetween)
		{
			var delay = new WaitForSecondsRealtime(delayInBetween);

			for (int i = 0; i < amount; i++)
			{
				HapticPatterns.PlayEmphasis(amplitude, frequency);
				yield return delay;
			}

			hapticMultiple = null;
		}

		public void StopHaptics()
		{
			if (hapticMultiple is not null)
			{
				StopCoroutine(hapticMultiple);
				hapticMultiple = null;
			}

			if (HapticController.IsPlaying())
				HapticController.Stop();
		}

		public enum AdvancedHapticType
		{
			None = -1,
			Carillon,
			Dice,
			Drums,
			GameOver,
			Heartbeats,
			Laser,
			PowerOff,
			Reload,
			Teleport
		}
	}
}