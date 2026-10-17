using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameLibrary.Graphics
{
    public class Sprite
    {
        // Contains the properties similar to SpriteBatch so it is self-contained, understand
        // that SpriteBatch may be a large library where we may instanciate many sprites, calling 
        // things that don't need to be.

        public TextureRegion Region { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Rot { get; set; } = 0.0f;

        public Vector2 Scale { get; set; } = Vector2.One;
        
    }
}
