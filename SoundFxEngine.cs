//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//// PARTICULAR PURPOSE.
////
//// Copyright (c) Wicked Wolf Apps, LLC. All rights reserved
//// www.WickedWolfApps.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WickedWolfApps.Common.Audio
{
    /// <summary>
    /// A Sound Effect Engine for C# apps
    /// </summary>
	public class SoundFxEngine
	{
        /// <summary>
        /// For each "SoundName", Cache up to N items for re-use, where N = MaxSoundFxInstances
        /// </summary>
        private Dictionary<string, List<MediaElement>> cachedSounds = new Dictionary<string, List<MediaElement>>();

        /// <summary>
        /// MaxSoundFxInstances, allow the cache for each sound effect to contain this many MediaElements
        /// </summary>
        public int MaxSoundFxInstances { get; set; }

        /// <summary>
        /// Location where the sounds are. Default is "Assets\\Sounds\\{0}.wav"
        /// </summary>
        public string SoundLocationFormatString { get; set; }

		/// <summary>
		/// CTOR - Preload sounds
		/// </summary>
        /// <param name="soundFiles">Names of the sounds located at "Assets\\Sounds\\{0}.wav" where {0} is the name of the file</param>
		public SoundFxEngine(List<string> soundFiles) : this()
		{
			LoadSounds(soundFiles);
		}

        /// <summary>
        /// Default CTOR - Does not Pre-cache Sounds
        /// </summary>
		public SoundFxEngine()
		{
            // Set the defaults
            this.MaxSoundFxInstances = 16;
            this.SoundLocationFormatString = "Assets\\Sounds\\{0}.wav";
		}

		/// <summary>
		/// Load Sound Files into a dict for faster access later in parallel
		/// </summary>
		/// <param name="soundFiles"></param>
		public async void LoadSounds(List<string> soundFiles)
		{
            List<Task> loadingTasks = new List<Task>();
            foreach (var soundFile in soundFiles)
            {
                loadingTasks.Add(CacheSound(soundFile));
            }
            await Task.WhenAll(loadingTasks);
		}

		/// <summary>
		/// Loads and adds an item to the cache
		/// </summary>
		/// <param name="soundName"></param>
		/// <returns></returns>
		private async Task CacheSound(string soundName)
		{
            soundName = soundName.ToLower();

            // Check if the sound is already cached
            if (cachedSounds.Keys.Contains(soundName))
            {
                return;
            }

			var package = Windows.ApplicationModel.Package.Current;
			var installedLocation = package.InstalledLocation;
            var storageFile = await installedLocation.GetFileAsync(string.Format(this.SoundLocationFormatString, soundName));

			if (storageFile != null)
			{
				var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

                List<MediaElement> sounds = new List<MediaElement>();
                for (var i = 0; i < 1; i++)
                {
    				MediaElement snd = new MediaElement();
                    snd.AutoPlay = false;
                    var itemStream = stream.CloneStream();
                    snd.SetSource(itemStream, storageFile.ContentType);
                    sounds.Add(snd);
                }
				cachedSounds.Add(soundName, sounds);
    		}
		}

		/// <summary>
		/// Plays a sound file
		/// </summary>
		/// <param name="soundName"></param>
		public async void PlaySound(string soundName)
        {
            soundName = soundName.ToLower();
            if (!cachedSounds.ContainsKey(soundName))
            {
                Debug.WriteLine("Sound not found: {0}", soundName);
                return;
            }

            MediaElement sound = await TryGetMediaElementFromCache(soundName);
            if (null != sound)
            {
                sound.Play();
            }
            else
            {
                Debug.WriteLine("Max instances of {0} are currently playing");
            }
        }

        /// <summary>
        /// Attempt to get an existing MediaElement instance from the cache.
        /// If it doesn't exist add a new one unless the max instances of the sound are already playing.
        /// </summary>
        /// <param name="soundName"></param>
        /// <returns></returns>
        private async Task<MediaElement> TryGetMediaElementFromCache(string soundName)
        {
            if (!cachedSounds.ContainsKey(soundName))
            {
                Debug.WriteLine("SoundEngineManager: Adding {0} to the cache", soundName);
                cachedSounds.Add(soundName, new List<MediaElement>());
            }

            MediaElement sound = (cachedSounds[soundName].Where(s => s.CurrentState == MediaElementState.Stopped || s.CurrentState == MediaElementState.Paused).FirstOrDefault());

            // If there isn't an available sound, add a new instance
            if (null == sound && cachedSounds[soundName].Count < MaxSoundFxInstances)
            {
                var package = Windows.ApplicationModel.Package.Current;
                var installedLocation = package.InstalledLocation;
                var storageFile = await installedLocation.GetFileAsync(string.Format(this.SoundLocationFormatString, soundName));
                var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                sound = new MediaElement();
                sound.AutoPlay = true;
                sound.SetSource(stream, storageFile.ContentType);
                cachedSounds[soundName].Add(sound);
                Debug.WriteLine("SoundEngineManager: Adding sound fx instance for sound {0}: {1} of {2}", soundName, cachedSounds[soundName].Count, this.MaxSoundFxInstances);
            }
            return sound;
        }
	}
}
