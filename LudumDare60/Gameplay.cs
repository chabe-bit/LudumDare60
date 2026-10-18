using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;

namespace LudumDare60
{
    public class Gameplay
    {
        KeyboardState _prev_kb;
        readonly Random _rng = new Random();

        // Spawn
        const int max_enemies = 10;

        // AI 
        const float chase_stop_distance = 1.2f;

    }
}
