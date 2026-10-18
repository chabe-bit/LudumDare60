using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoGameLibrary.Audio
{
    public class AudioController
    {
        // Track live SFX instances so they can be paused, unpaused, removed 
        private readonly List<SoundEffectInstance> _active_sfx_instances;
        
        // Tracks the state of the volume if toggling between muting/unmuting
        private float _prev_song_volume;
        private float _prev_sfx_volume;
        
        public bool is_muted { get; private set; }
    
        // Really loving this syntax C# 
        // This is setting up the GETTER and SETTER 
        public float SongVolume
        {
            get
            {
                if (is_muted)
                {              
                    return 0.0f;
                }
                return MediaPlayer.Volume;
            }
            set
            {
                if (is_muted)
                {
                    return;
                }
                MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        public float SFXVolume
        {
            get
            {
                if (is_muted)
                {
                    return 0.0f;
                }
                return SoundEffect.MasterVolume;
            }
            set
            {
                if (is_muted)
                {
                    return;
                }
                SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        public bool IsDisposed { get; private set; }
    
        public AudioController()
        {
            _active_sfx_instances = new List<SoundEffectInstance>();
        }

        ~AudioController() => Dispose(false);
        
        // Requirements:
        // - Update for resources to clear 
        // - Play both SFX and Music
        // - Manage audio states, play, pause and mute/unmute 
        // - Manage audio volume 
        
        public void Update()
        {
            // Start at the end of the list as we are worknig with the most recent SFX in the list 
            for (int i = _active_sfx_instances.Count - 1; i >= 0; i--)
            {
                SoundEffectInstance inst = _active_sfx_instances[i];
                if (inst.State == SoundState.Stopped)
                {
                    if (!inst.IsDisposed)
                    {
                        inst.Dispose();
                    }
                    _active_sfx_instances.RemoveAt(i);
                }
            }
        }
    
        
    
        protected void Dispose(bool dispose)
        {

        }
    }
}
