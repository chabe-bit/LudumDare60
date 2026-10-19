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

        EntityDef _player_def;

        // Enemy will walk one side, pause, walk back and pause while not aggroed
        TextureAtlas _e_robo_atlas;
        EntityDef _enemy_def;
        float _robo_hit_flash;

        const float startup_time = 0.4f;
        const int spins = 3;
        const float recover_time = 0.5f;
        const float enemy_attack_cooldown = 0.8f;
        const float spin_dist_px = 330f;
        const float player_radius = 0.4f;
        const int spin_damage = 1;

        // Fixed enemy spawns: position and the direction they face and patrol along
        static readonly (Vector3 pos, Vector3 facing)[] s_enemy_spawns =
        {
            (new Vector3(6f, 0f, -4f), Vector3.Right),
            (new Vector3(-8f, 0f, -6f), Vector3.Backward),
            (new Vector3(4f, 0f, 7f), Vector3.Left),
        };

        // Patrol
        const float patrol_dist = 4f;           // how far from spawn the patrol walks
        const float patrol_pause_time = 1.2f;
        const float patrol_walk_timeout = 6f;   // blocked by another enemy: give up and turn around
        const float arrive_dist = 0.1f;

        // Aggro / chase
        const float aggro_range = 7f;
        const float aggro_cone_cos = 0.7f;      // cos(45°): sees 45° either side of facing
        const float chase_speed_mult = 1.4f;    // 2.5 * 1.4 = 3.5, still slower than the player's 5

        // Scrap (mocked with the slime sprite until there's scrap art)
        TextureAtlas _scrap_atlas;
        List<Pickup> _pickups = new();
        SoundEffect _collect_sfx;
        int _scrap;

        const float scrap_scale = 8f;            // slime is 20px; unscaled it'd be 0.08 units at 250 px per unit
        const float scrap_pickup_radius = 0.4f;

        // Fixed scrap spots, a few placed on enemy patrol routes
        static readonly Vector3[] s_scrap_spawns =
        {
            new Vector3(2f, 0f, -2f),
            new Vector3(-3f, 0f, 1f),
            new Vector3(8f, 0f, -4f),     // in the right enemy's patrol
            new Vector3(-8f, 0f, -3f),    // in the left enemy's patrol
            new Vector3(2f, 0f, 7f),      // in the bottom enemy's patrol
        };

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

            foreach (var (pos, facing) in s_enemy_spawns)
                SpawnEnemy(pos, facing);

            _scrap_atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _collect_sfx = Content.Load<SoundEffect>("audio/sfx/collect");
            foreach (Vector3 pos in s_scrap_spawns)
                SpawnScrap(pos, 1);


            Song theme = Content.Load<Song>("audio/music/bgm");


            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(theme);
            MediaPlayer.IsRepeating = true;


            _step_sfx = Content.Load<SoundEffect>("audio/sfx/collect");

        }
        
        private Entity SpawnEnemy(Vector3 pos, Vector3 facing)
        {
            var ent = new Entity
            {
                ai = EnemyState.Walk,
                def = _enemy_def,
                pos = pos,
                facing = facing,
                patrol_a = pos,
                patrol_b = pos + facing * patrol_dist,
                hp = _enemy_def.max_hp,
                is_enemy = true,
                sprite = new AnimatedSprite(_enemy_def.walk_anim) { Scale = _enemy_def.scale }
            };

            _entities.Add(ent);
            return ent;
        }

        // Enemies can call this on death later to drop scrap
        private void SpawnScrap(Vector3 pos, int value)
        {
            var sprite = _scrap_atlas.CreateAnimatedSprite("slime-animation");
            sprite.Scale = new Vector2(scrap_scale, scrap_scale);
            _pickups.Add(new Pickup { pos = pos, value = value, sprite = sprite });
        }

        private void UpdatePickups(GameTime game_time)
        {
            float reach = scrap_pickup_radius + player_radius;

            // backwards so RemoveAt doesn't skip the next pickup
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                Pickup p = _pickups[i];
                p.sprite.Update(game_time);

                if (FlatDistSquare(p.pos, _robo_pos) < reach * reach)
                {
                    _scrap += p.value;
                    _collect_sfx.Play(0.8f, 0.5f, 0f);   // pitched up: footsteps use the same wav
                    _pickups.RemoveAt(i);
                    Window.Title = $"LudumDare60 - Scrap: {_scrap}";   // stand-in HUD until there's a font
                }
            }
        }

        private void SetEnemyState(Entity ent, EnemyState state)
        {
            ent.ai = state;
            ent.state_timer = 0f;
            ent.last_frame = -1;

            ent.sprite.Animation = state == EnemyState.Attack ? ent.def.attack_anim : ent.def.walk_anim;
        }

        private void MoveEnemy(Entity ent, Vector3 dir, float speed, float dt)
        {
            dir.Y = 0f;
            if (dir.LengthSquared() < 0.0001f) return;
            dir.Normalize();
            ent.facing = dir;
            ent.pos += dir * speed * dt;
        }

        private bool CanSeePlayer(Entity ent, Vector3 player_pos)
        {
            Vector3 to_player = player_pos - ent.pos;
            to_player.Y = 0f;
            float dist = to_player.Length();

            if (dist > aggro_range) return false;
            return dist < 0.001f || Vector3.Dot(ent.facing, to_player / dist) >= aggro_cone_cos;
        }

        // Also call this from the player's attack, so hitting an enemy from behind aggros it
        private void Aggro(Entity ent)
        {
            if (ent.ai == EnemyState.Idle || ent.ai == EnemyState.Walk)
                SetEnemyState(ent, EnemyState.Chase);
        }

        // Circles push each other apart so nothing overlaps
        private void SeparateEntities()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                Entity a = _entities[i];
                if (a.dead) continue;

                PushApart(ref a.pos, ref _robo_pos, a.def.radius, player_radius);
                for (int j = i + 1; j < _entities.Count; j++)
                {
                    Entity b = _entities[j];
                    if (b.dead) continue;
                    PushApart(ref a.pos, ref b.pos, a.def.radius, b.def.radius);
                }
            }
        }

        private static void PushApart(ref Vector3 a, ref Vector3 b, float radius_a, float radius_b)
        {
            float dx = b.X - a.X, dz = b.Z - a.Z;
            float min = radius_a + radius_b;
            float d2 = dx * dx + dz * dz;
            if (d2 >= min * min) return;

            float d = MathF.Sqrt(d2);
            Vector2 n = d > 0.0001f ? new Vector2(dx / d, dz / d) : Vector2.UnitX;   // exactly on top: pick any direction
            float half = (min - d) * 0.5f;
            a.X -= n.X * half; a.Z -= n.Y * half;
            b.X += n.X * half; b.Z += n.Y * half;
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
                case EnemyState.Walk:
                {
                    if (CanSeePlayer(ent, player_pos)) { Aggro(ent); break; }

                    Vector3 target = ent.patrol_to_b ? ent.patrol_b : ent.patrol_a;
                    bool arrived = FlatDistSquare(ent.pos, target) < arrive_dist * arrive_dist;
                    if (arrived || ent.state_timer >= patrol_walk_timeout)
                        SetEnemyState(ent, EnemyState.Idle);
                    else
                        MoveEnemy(ent, target - ent.pos, ent.def.speed, dt);
                    break;
                }

                case EnemyState.Idle:
                    if (CanSeePlayer(ent, player_pos)) { Aggro(ent); break; }

                    if (ent.state_timer >= patrol_pause_time)
                    {
                        // turn around and walk back
                        ent.patrol_to_b = !ent.patrol_to_b;
                        ent.facing = -ent.facing;
                        SetEnemyState(ent, EnemyState.Walk);
                    }
                    break;

                case EnemyState.Chase:
                {
                    if (ent.attack_cooldown <= 0f && dist_square < hit_radius * hit_radius * 0.8f)
                    {
                        SetEnemyState(ent, EnemyState.StartupFrame);
                        break;
                    }

                    // hold just outside the player while the attack cools down
                    float stop = hit_radius * 0.6f;
                    if (dist_square > stop * stop)
                        MoveEnemy(ent, player_pos - ent.pos, ent.def.speed * chase_speed_mult, dt);
                    break;
                }

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
            SeparateEntities();
            UpdatePickups(gameTime);

            _robo_hit_flash = MathF.Max(0f, _robo_hit_flash - dt);
            //_robo.Color = _robo_hit_flash > 0f ? Color.Red : Color.White;

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

            _billboards.Begin(view, projection, _scrap_atlas.Texture);
            foreach (Pickup p in _pickups)
                _billboards.Draw(p.sprite, p.pos);
            _billboards.End();


            base.Draw(gameTime);
        }

        private Color EnemyTint(Entity ent)
        {
            return Color.Aqua;
        }
    }
}