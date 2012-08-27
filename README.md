WickedWolfApps.Common.Audio.SoundFxEngine
=========================================

A Simple Sound FX engine for Windows 8 XAML C# apps that does some basic caching of MediaElement for performance and memory optimization.  

Nuget = PM> Install-Package WickedWolfApps.SoundFxEngine 

---------------------------------------------------------------------------
Example Usage:

List<string> sounds = new List<string>() { "MyTestSound1", "MyTestSound2" };

this.SoundEngine = new SoundFxEngine(sounds);

this.SoundEngine.PlaySound(soundFile);
