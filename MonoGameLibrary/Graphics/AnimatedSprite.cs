using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics
{
    public class AnimatedSprite : Sprite
    {
        private int _curr_frame; // track frame being displayed 
        private TimeSpan _elapsed; // track time between each frame 
        private Animation _animation; // playlist of animation being played 

        public int CurrFrame => _curr_frame;

        public Animation Animation
        {
            // what is this syntax C# ???
            get => _animation; // instanciates value from the right of => 
            set // okay
            {
                _animation = value;
                _curr_frame = 0; // restart the frame 
                _elapsed = TimeSpan.Zero;
                Region = _animation.Frames[0];
            }
        }

        public AnimatedSprite() { }

        public AnimatedSprite(Animation animation)
        {
            Animation = animation;
        }

        public void Update(GameTime game_time)
        {
            // Start the timer 
            _elapsed += game_time.ElapsedGameTime;
            
            // As soon as a frame has completed ...
            if (_elapsed >= _animation.Delay)
            {
                // Reset the time elapse counter  
                _elapsed -= Animation.Delay;
                
                // Go to the next frame, continue until we have exceeded the max frame count  
                _curr_frame++;

                // If current has exceed max frame count ...
                if (_curr_frame >= _animation.Frames.Count)
                {
                    // Reset 
                    _curr_frame = 0;
                }

                // Fetch next frame from the region 
                Region = _animation.Frames[_curr_frame];
            }
        }
    }
}
