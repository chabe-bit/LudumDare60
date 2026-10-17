using System;
using System.Collections.Generic;

// Create animations by cycling through different regions 
// through our texture atlas. 

namespace MonoGameLibrary.Graphics
{
    public class Animation
    {
        // Track animation frames, order matters because frames will animate in sequence they've been added 
        public List<TextureRegion> Frames { get; set; }
    
        // Time between each frame 
        public TimeSpan Delay { get; set; }

        public Animation(List<TextureRegion> frames, TimeSpan delay)
        {
            Frames = frames;
            Delay = delay;
        }
        


    }
}
