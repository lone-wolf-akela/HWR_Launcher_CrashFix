using System;
using System.Windows.Media;

namespace Launcher
{
	internal class SoundEffect
	{
		private readonly MediaPlayer[] EffectInstances;

		private readonly Uri ResourceAddress;

		private readonly float Volume;

		private int CurrentInstance;

		private readonly MainWindow Owner;

		internal SoundEffect(MainWindow InOwner, int InMaxInstances, string InFileName, float InVolume)
		{
			Owner = InOwner;
			EffectInstances = new MediaPlayer[InMaxInstances];
			ResourceAddress = new Uri(string.Format("Audio/{0}.mp3", InFileName), UriKind.Relative);
			Volume = InVolume;
			CurrentInstance = 0;
		}

		internal void Play()
		{
			if (!Owner.IsSfxMuted)
			{
				MediaPlayer Instance = GetEffectInstance();
				if (Instance != null)
				{
					Instance.Position = TimeSpan.Zero;
					Instance.Play();
				}
			}
		}

		private MediaPlayer GetEffectInstance()
		{
			MediaPlayer Instance = EffectInstances[CurrentInstance];
			if (Instance == null)
			{
				Instance = new MediaPlayer();
				Instance.Open(ResourceAddress);
				Instance.Volume = Volume;
				EffectInstances[CurrentInstance] = Instance;
			}
			checked
			{
				CurrentInstance++;
				if (CurrentInstance >= EffectInstances.Length)
				{
					CurrentInstance = 0;
				}
				return Instance;
			}
		}
	}
}
