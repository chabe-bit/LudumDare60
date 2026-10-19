using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LudumDare60
{
    public class Game1 : Core
    {
        Camera3D _camera;
        GroundGrid _grid;
        BillboardRenderer _billboards;

        TextureAtlas _robo_atlas;
        Animation[,] _robo_anims = new Animation[2, 4];
        AnimatedSprite _robo;
        RoboState _robo_state = RoboState.Idle;
        Direction _robo_dir = Direction.Front;
        Vector3 _robo_facing = Vector3.Backward;
        Vector3 _robo_pos;
        int _robo_idle = (int)RoboState.Idle;
        int _robo_walk = (int)RoboState.Walk;

        // Audio -> SFX and Music
        private SoundEffect _step_sfx;
        int _robo_last_frame = -1;
        private int[] walk_frames = { 0, 2 };

        int _player;
        float _spawn_timer = 1f;
        const float spawn_interval = 3f;
        const int max_enemies = 10;
        Vector3 move = Vector3.Zero;

        List<Entity> _entities = new();

        // Enemy will walk one side, pause, walk back and pause while not aggroed
        TextureAtlas _e_robo_atlas;
        EntityDef _enemy_def;
        float _robo_hit_flash;

        const float startup_time = 0.4f;
        const int spins = 3;
        const float recover_time = 0.5f;
        const float enemy_attack_cooldown = 0.8f;
        const float spin_dist_px = 330f;
        const float player_radius = 0.25f;
        const int spin_damage = 1;

        public Game1() : base("LudumDare60", 1280, 720, false) { }

        protected override void Initialize()
        {
            base.Initialize();   // in this template, Initialize calls LoadContent

            _camera = new Camera3D();
            _grid = new GroundGrid(GraphicsDevice);
            _billboards = new BillboardRenderer(GraphicsDevice) { PixelsPerUnit = 250f };
        }

        protected override void LoadContent()
        {
            _robo_atlas = TextureAtlas.FromFile(Content, "images/robo-atlas-definition.xml");
            _robo_anims[_robo_idle, (int)Direction.Front] = _robo_atlas.GetAnimation("robo-idle-front-animation");
            _robo_anims[_robo_idle, (int)Direction.Back] = _robo_atlas.GetAnimation("robo-idle-back-animation");
            _robo_anims[_robo_idle, (int)Direction.Right] = _robo_atlas.GetAnimation("robo-idle-right-animation");
            _robo_anims[_robo_idle, (int)Direction.Left] = _robo_anims[_robo_idle, (int)Direction.Right];
            _robo_anims[_robo_walk, (int)Direction.Front] = _robo_atlas.GetAnimation("robo-walk-front-animation");
            _robo_anims[_robo_walk, (int)Direction.Back] = _robo_atlas.GetAnimation("robo-walk-back-animation");
            _robo_anims[_robo_walk, (int)Direction.Right] = _robo_atlas.GetAnimation("robo-walk-right-animation");
            _robo_anims[_robo_walk, (int)Direction.Left] = _robo_anims[_robo_walk, (int)Direction.Right];
            _robo = _robo_atlas.CreateAnimatedSprite("robo-idle-front-animation");
            _robo.Scale = Vector2.One;


            _e_robo_atlas = TextureAtlas.FromFile(Content, "images/enemy-atlas-definition.xml");
            _enemy_def = new EntityDef
            {
                name = "Security Bot",
                atlas = _e_robo_atlas,

                speed = 2.5f,
                max_hp = 3,
                radius = 0.4f,
                scale = Vector2.One,
                walk_anim = _e_robo_atlas.GetAnimation("enemy-walk-animation"),
                attack_anim = _e_robo_atlas.GetAnimation("enemy-attack-animation")
            };

            SpawnEnemy(new Vector3(3f, 0f, -3f));


            Song theme = Content.Load<Song>("audio/music/theme");


            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(theme);
            MediaPlayer.IsRepeating = true;


            _step_sfx = Content.Load<SoundEffect>("audio/sfx/collect");

        }
        
        private Entity SpawnEnemy(Vector3 pos)
        {
            var ent = new Entity
            {
                ai = EnemyState.Walk,
                def = _enemy_def,
                pos = pos,
                hp = _enemy_def.max_hp,
                is_enemy = true,
                sprite = new AnimatedSprite(_enemy_def.walk_anim) { Scale = _enemy_def.scale }
            };

            _entities.Add(ent);
            return ent;
        }

        private void SetEnemyState(Entity ent, EnemyState state)
        {
            ent.ai = state;
            ent.state_timer = 0f;
            ent.last_frame = -1;

            ent.sprite.Animation = state == EnemyState.Attack ? ent.def.attack_anim : ent.def.walk_anim;
        }

        private float SpinReach(Entity ent) => spin_dist_px / _billboards.PixelsPerUnit * ent.sprite.Scale.X;

        private float SpinTime(Entity ent)
        {
            Animation spin = ent.def.attack_anim;
            return spins * spin.Frames.Count * (float)spin.Delay.TotalSeconds;
        }
        private static float FlatDistSquare(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z; 
            return dx * dx + dz * dz;
        }

        private int UpdateEnemy(Entity ent, Vector3 player_pos, float dt, GameTime game_time)
        {
            ent.state_timer += dt;
            ent.attack_cooldown = MathF.Max(0f, ent.attack_cooldown - dt);

            float hit_radius = SpinReach(ent) + player_radius;
            float dist_square = FlatDistSquare(ent.pos, player_pos);
            int damage = 0;

            switch (ent.ai)
            {
                case EnemyState.Idle:
                case EnemyState.Walk:
                case EnemyState.Chase:
                    if (ent.attack_cooldown <= 0f && dist_square < hit_radius * hit_radius * 0.8f)
                        SetEnemyState(ent, EnemyState.StartupFrame);
                    break;

                case EnemyState.StartupFrame:
                    if (ent.state_timer >= startup_time)
                    {
                        SetEnemyState(ent, EnemyState.Attack);
                        ent.attack_hit_completed = false;
                    }
                    break;

                case EnemyState.Attack:          
                    // Sweep in a full circle
                    if (!ent.attack_hit_completed &&
                        dist_square < hit_radius * hit_radius)
                    {
                        ent.attack_hit_completed = true;
                        damage = spin_damage;
                    }
                    if (ent.state_timer >= SpinTime(ent))
                    {
                        SetEnemyState(ent, EnemyState.RecoverFrame);
                    }
                    break;

                case EnemyState.RecoverFrame:    
                    if (ent.state_timer >= recover_time)
                    {
                        ent.attack_cooldown = enemy_attack_cooldown;
                        SetEnemyState(ent, EnemyState.Chase);
                    }
                    break;
            }

            if (ent.ai == EnemyState.Walk || ent.ai == EnemyState.Chase || ent.ai ==EnemyState.Attack)
            {
                ent.sprite.Update(game_time);
            }

            int frame = ent.sprite.CurrFrame;
            if (frame != ent.last_frame)
            {
                if (ent.ai == EnemyState.Attack && frame == 0 && ent.def.attack_sfx != null)
                {
                    ent.def.attack_sfx[0].Play(0.5f, 0f, 0f);
                }

                ent.last_frame = frame;
            }

            return damage;
        }

        private void PlayerHit(Entity enemy, int damage)
        {
            _robo_hit_flash = 0.25f;

        }

        private void SetAnimation(RoboState state, Direction dir)
        {
            if (state == _robo_state && dir == _robo_dir)
                return;

            Animation anim = _robo_anims[(int)state, (int)dir];
            if (anim == null)
            {
                Debug.WriteLine($"ERROR: no animation for {state}, {dir}");
                return;
            }

            _robo_last_frame = -1;
            _robo_state = state;
            _robo_dir = dir;
            _robo.Animation = anim;
            _robo.Effects = dir == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        // SFX => [ Attack, Footsteps, Jump?, COllision (walls and enemies), ...]
        // Music => Title screen? In-Game theme
        private void PlayStep()
        {
            _step_sfx.Play(volume: 0.6f, pitch: 0f, pan: 0f);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var kb = Keyboard.GetState();
            
            if (kb.IsKeyDown(Keys.Escape)) Exit();
            Vector3 move = Vector3.Zero;
            if (kb.IsKeyDown(Keys.W)) { move += _camera.Forward; }
            if (kb.IsKeyDown(Keys.S)) { move -= _camera.Forward; }
            if (kb.IsKeyDown(Keys.D)) { move += _camera.Right; }
            if (kb.IsKeyDown(Keys.A)) { move -= _camera.Right; }
            
                 
            if (move != Vector3.Zero)
            {
                move.Normalize();
                _robo_pos += move * 5f * dt;
                _robo_facing = move;
            }


            int frame = _robo.CurrFrame;
            if (frame != _robo_last_frame)
            {
                if (_robo_state == RoboState.Walk && Array.IndexOf(walk_frames, frame) >= 0)
                {
                    PlayStep();
                }

                _robo_last_frame = frame;
            }


            RoboState state = move != Vector3.Zero ? RoboState.Walk : RoboState.Idle;
            Direction dir = PickDirection(_robo_facing, _camera);


            if (_robo_anims[(int)state, (int)dir] == null)
                state = RoboState.Idle;
            SetAnimation(state, dir);
            _robo.Update(gameTime);
            
            foreach (Entity ent in  _entities)
            {
                if (ent.dead || !ent.is_enemy)
                    continue;

                int damage = UpdateEnemy(ent, _robo_pos, dt, gameTime);
                if (damage > 0)
                {
                    PlayerHit(ent, damage);
                }
             }

            _robo_hit_flash = MathF.Max(0f, _robo_hit_flash - dt);
            _robo.Color = _robo_hit_flash > 0f ? Color.Red : Color.White;

            // camera follows the robot
            _camera.Target = _robo_pos;
            _camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;


            base.Update(gameTime);
        }

        private static Direction PickDirection(Vector3 facing, Camera3D cam)
        {
            float forward = Vector3.Dot(facing, cam.Forward);
            float side = Vector3.Dot(facing, cam.Right);

            if (MathF.Abs(forward) > MathF.Abs(side))
            {
                return forward > 0 ? Direction.Back : Direction.Front;
            }
            return side > 0 ? Direction.Right : Direction.Left;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);   // also clears the depth buffer

            Matrix view = _camera.View;
            Matrix projection = _camera.Projection;

            _grid.Draw(view, projection);

            _billboards.Begin(view, projection, _robo_atlas.Texture);
            _billboards.Draw(_robo, _robo_pos);
            _billboards.End();

            _billboards.Begin(view, projection, _e_robo_atlas.Texture);
            foreach (Entity ent in _entities)
            {
                _billboards.Draw(ent.sprite, ent.pos);
            }
            _billboards.End();


            base.Draw(gameTime);
        }

        private Color EnemyTint(Entity ent)
        {
            return Color.Aqua;
        }
    }
}