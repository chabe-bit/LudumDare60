using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace LudumDare60
{
    public class Entity
    {
        public EntityDef def;

        public Vector3 pos;
        public Vector3 vel;
        public Vector3 facing = Vector3.Backward;
        public Vector3 move;

        public int hp;
        public AnimatedSprite sprite;
        public RoboState state = RoboState.Idle;
        public Direction dir = Direction.Front;
        public int last_frame = -1;

        public bool dead;

        public EntityKind kind => def.kind;
        public bool is_enemy => def.kind != EntityKind.Player;
    }
}
