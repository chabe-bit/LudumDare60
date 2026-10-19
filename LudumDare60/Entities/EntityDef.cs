using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGameLibrary.Graphics;
using System;

namespace LudumDare60
{
    public class EntityDef
    {
        // Entity Metadata 
        public string name;
        public TextureAtlas atlas;


        // stats 
        public float speed;
        public int max_hp;
        public float radius;

        public float pixels_per_unit = 250f;
        public Vector2 scale = Vector2.One;
        public Color tint = Color.White;

        public SoundEffect[] step_sfx;
        public SoundEffect[] attack_sfx; // todo 
        public SoundEffect[] death_sfx; // todo 
        public int[,] step_frames;

        public Animation[,] sprite_anims = new Animation[2, 4];
        public bool[,] sprite_flip = new bool[2, 4];


        public Animation walk_anim;
        public Animation attack_anim;

        public float spin_px;
        public int spins = 1;

        public void SetAnim(Animation anim)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sprite_anims[i, j] = anim;
                    sprite_flip[i, j] = false;
                }
            }
        }

        public void LoadAnim(string prefix)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string state = ((RoboState)i).ToString().ToLowerInvariant();
                    string dir = ((Direction)j).ToString().ToLowerInvariant();
                    string name = $"{prefix}-{state}-{dir}-animation";

                    if (atlas.TryGetAnimation(name, out Animation anim))
                    {
                        sprite_anims[i, j] = anim;
                    }
                }

                if (sprite_anims[i, (int)Direction.Left] == null)
                {
                    sprite_anims[i, (int)Direction.Left] = sprite_anims[i, (int)Direction.Right];
                    sprite_flip[i, (int)Direction.Left] = true;
                }
            }
        }
    }
}
