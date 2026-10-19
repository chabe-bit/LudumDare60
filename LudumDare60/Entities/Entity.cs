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
        public float hit_flash;
        public float death_timer;

        // Enemy
        public EnemyState ai = EnemyState.Idle;
        public float state_timer;
        public float attack_cooldown;
        public bool attack_hit_completed;
        public bool is_enemy;
        public Vector3 patrol_a;
        public Vector3 patrol_b;
        public bool patrol_to_b = true;



    }
}
