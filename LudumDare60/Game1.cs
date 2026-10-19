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
        FloorRenderer _floor;
        TextureAtlas _floor_atlas;
        BillboardRenderer _billboards;

        const float arena_half = 20f;
        const float wall_height = 6.25f;
        TextureAtlas _wall_atlas;
        WallRenderer _wall_renderer;
        List<BoundingBox> _walls = new();

        TextureAtlas _robo_atlas;
        EntityDef _robo_def;
        Entity _player;

        float _spawn_timer = 1f;
        const float spawn_interval = 3f;
        const int max_enemies = 10;
        Vector3 move = Vector3.Zero;

        List<Entity> _entities = new();


        // Enemy will walk one side, pause, walk back and pause while not aggroed
        TextureAtlas _e_robo_atlas;
        EntityDef _enemy_def;

        const float startup_time = 0.4f;
        const float recover_time = 0.5f;
        const float enemy_attack_cooldown = 0.8f;
        const int spin_damage = 1;
        const float hit_flash_time = 0.25f;

        const float player_attack_cooldown = 0.2f;
        const float player_spin_move_mult = 0.5f;
        HashSet<object> _spin_hits = new();

        const float knockback_speed = 10f;
        const float knockback_friction = 25f;
        const float bounce_restitution = 0.8f;
        const float bounce_transfer = 0.5f;
        const int enemy_scrap_drop = 1;

        const float death_hold_time = 0.4f;
        const int death_flickers = 3;
        const float death_flicker_time = 0.2f;
        const float death_time = death_hold_time + death_flickers * death_flicker_time;

        static readonly (Vector3 pos, Vector3 facing)[] s_enemy_spawns =
        {
            (new Vector3(6f, 0f, -4f), Vector3.Right),
            (new Vector3(-8f, 0f, -6f), Vector3.Backward),
            (new Vector3(4f, 0f, 7f), Vector3.Left),
        };

        const float patrol_dist = 4f;
        const float patrol_pause_time = 1.2f;
        const float patrol_walk_timeout = 6f;
        const float arrive_dist = 0.1f;

        const float aggro_range = 7f;
        const float aggro_cone_cos = 0.7f;
        const float chase_speed_mult = 1.4f;

        TextureAtlas _scrap_atlas;
        List<Pickup> _pickups = new();
        SoundEffect _collect_sfx;
        SoundEffect _wall_collision_sfx;
        SoundEffect _hit_sfx;
        Vector3 _player_wall_n;

        int _scrap;

        const float scrap_scale = 1f;
        const float scrap_pickup_radius = 0.4f;

        static readonly Vector3[] s_scrap_spawns =
        {
            new Vector3(2f, 0f, -2f),
            new Vector3(-3f, 0f, 1f),
            new Vector3(8f, 0f, -4f),
            new Vector3(-8f, 0f, -3f),
            new Vector3(2f, 0f, 7f),
        };

        TextureAtlas _computer_atlas;
        TextureAtlas _computer_damaged_atlas;
        List<Breakable> _computers = new();
        SoundEffect[] _computer_hit_sfx;

        const int computer_hp = 2;
        const int computer_scrap_drop = 1;
        const float computer_half_width = 0.75f;
        const float computer_half_depth = 0.5f;

        static readonly Vector3[] s_computer_spawns =
        {
            new Vector3(0f, 0f, -4f),
            new Vector3(-12f, 0f, -12f),
            new Vector3(12f, 0f, 8f),
        };

        public Game1() : base("LudumDare60", 1280, 720, false) { }

        protected override void Initialize()
        {
            base.Initialize();   // in this template, Initialize calls LoadContent

            _camera = new Camera3D();
            _billboards = new BillboardRenderer(GraphicsDevice) { PixelsPerUnit = 250f };
            _floor = new FloorRenderer(GraphicsDevice, _floor_atlas.GetRegion("floor"), _billboards.PixelsPerUnit, arena_half);

            float a = arena_half;
            _wall_renderer = new WallRenderer(GraphicsDevice, _wall_atlas.GetRegion("wall-plain"), wall_height, new[]
            {
                (new Vector3(-a, 0f, -a), new Vector3(a, 0f, -a)),
                (new Vector3(-a, 0f, a), new Vector3(-a, 0f, -a)),
                (new Vector3(a, 0f, -a), new Vector3(a, 0f, a)),
            });
        }

        protected override void LoadContent()
        {
            _floor_atlas = TextureAtlas.FromFile(Content, "images/floor-atlas-definition.xml");
            _wall_atlas = TextureAtlas.FromFile(Content, "images/wall-plain-atlas-definition.xml");
            BuildArenaBounds();

            _robo_atlas = TextureAtlas.FromFile(Content, "images/robo-atlas-definition.xml");
            _robo_def = new EntityDef
            {
                name = "Robo",
                atlas = _robo_atlas,
                speed = 5f,
                max_hp = 3,
                radius = 0.4f,
                scale = Vector2.One,
                attack_anim = _robo_atlas.GetAnimation("robo-attack-animation"),
                spin_px = 240f,
                spins = 3,
                attack_sfx = new[] { Content.Load<SoundEffect>("audio/sfx/spin_attack") },
                step_sfx = new[] { Content.Load<SoundEffect>("audio/sfx/left_footstep"), Content.Load<SoundEffect>("audio/sfx/right_footstep") },

                step_frames = new int[,]
                {
                    { 3, 1 },
                    { 3, 1 },
                    { 2, 0 },
                    { 2, 0 },
                },
            };
            _robo_def.LoadAnim("robo");

            _player = new Entity
            {
                def = _robo_def,
                hp = _robo_def.max_hp,
                sprite = new AnimatedSprite(_robo_def.sprite_anims[(int)RoboState.Idle, (int)Direction.Front]) { Scale = _robo_def.scale },
            };


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
                attack_anim = _e_robo_atlas.GetAnimation("enemy-attack-animation"),
                spin_px = 330f,
                spins = 3,
                attack_sfx = new[] { Content.Load<SoundEffect>("audio/sfx/spin_attack") },
            };

            foreach (var (pos, facing) in s_enemy_spawns)
                SpawnEnemy(pos, facing);

            _scrap_atlas = TextureAtlas.FromFile(Content, "images/old-scrap-atlas-definition.xml");
            _collect_sfx = Content.Load<SoundEffect>("audio/sfx/collect");
            _wall_collision_sfx = Content.Load<SoundEffect>("audio/sfx/wall_collsion");
            foreach (Vector3 pos in s_scrap_spawns)
                SpawnScrap(pos, 1);

            _computer_atlas = TextureAtlas.FromFile(Content, "images/computer-undamaged-atlas-definition.xml");
            _computer_damaged_atlas = TextureAtlas.FromFile(Content, "images/computer-damaged-atlas-definition.xml");
            _hit_sfx = Content.Load<SoundEffect>("audio/sfx/break_item_1");
            _computer_hit_sfx = new[] { _hit_sfx, _hit_sfx };
            foreach (Vector3 pos in s_computer_spawns)
                SpawnComputer(pos);


            Song theme = Content.Load<Song>("audio/music/main_bgm");


            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            //MediaPlayer.Play(theme);
            MediaPlayer.IsRepeating = true;
        }
        
        private void BuildArenaBounds()
        {
            float a = arena_half, t = 2f;
            _walls.Add(new BoundingBox(new Vector3(-a - t, 0f, -a - t), new Vector3(a + t, 1f, -a)));
            _walls.Add(new BoundingBox(new Vector3(-a - t, 0f, a), new Vector3(a + t, 1f, a + t)));
            _walls.Add(new BoundingBox(new Vector3(-a - t, 0f, -a - t), new Vector3(-a, 1f, a + t)));
            _walls.Add(new BoundingBox(new Vector3(a, 0f, -a - t), new Vector3(a + t, 1f, a + t)));
        }

        private Vector3 ClampCameraTarget(Vector3 target)
        {
            float sin = MathF.Sin(_camera.Pitch), cos = MathF.Cos(_camera.Pitch);
            float tan_v = MathF.Tan(_camera.FOV / 2f);
            float tan_h = tan_v * _camera.AspectRatio;

            float t = _camera.Distance * sin / (sin + tan_v * cos);
            float bottom_z = _camera.Distance * cos - t * (cos - tan_v * sin);
            float half_width = t * tan_h;

            target.X = MathHelper.Clamp(target.X, -arena_half + half_width, arena_half - half_width);
            target.Z = MathF.Min(target.Z, arena_half - bottom_z);
            return target;
        }

        private Vector3 ResolveWalls(ref Vector3 pos, float radius)
        {
            Vector3 pushed = Vector3.Zero;
            foreach (BoundingBox wall in _walls)
            {
                float cx = MathHelper.Clamp(pos.X, wall.Min.X, wall.Max.X);
                float cz = MathHelper.Clamp(pos.Z, wall.Min.Z, wall.Max.Z);
                float dx = pos.X - cx, dz = pos.Z - cz;
                float d2 = dx * dx + dz * dz;
                if (d2 >= radius * radius) continue;

                Vector3 n;
                float depth;
                if (d2 > 0.000001f)
                {
                    float d = MathF.Sqrt(d2);
                    n = new Vector3(dx / d, 0f, dz / d);
                    depth = radius - d;
                }
                else
                {
                    float left = pos.X - wall.Min.X, right = wall.Max.X - pos.X;
                    float back = pos.Z - wall.Min.Z, front = wall.Max.Z - pos.Z;
                    float m = MathF.Min(MathF.Min(left, right), MathF.Min(back, front));
                    if (m == left) n = -Vector3.UnitX;
                    else if (m == right) n = Vector3.UnitX;
                    else if (m == back) n = -Vector3.UnitZ;
                    else n = Vector3.UnitZ;
                    depth = m + radius;
                }

                pos += n * depth;
                pushed += n;
            }
            return pushed;
        }

        private void KeepOutOfWalls()
        {
            _player_wall_n = ResolveWalls(ref _player.pos, _player.def.radius);
            foreach (Entity ent in _entities)
            {
                if (!ent.dead)
                    ResolveWalls(ref ent.pos, ent.def.radius);
            }
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

        private void SpawnScrap(Vector3 pos, int value)
        {
            var sprite = _scrap_atlas.CreateAnimatedSprite("old-scrap-animation");
            sprite.Scale = new Vector2(scrap_scale, scrap_scale);
            _pickups.Add(new Pickup { pos = pos, value = value, sprite = sprite });
        }

        private void SpawnComputer(Vector3 pos)
        {
            Vector3 half = new Vector3(computer_half_width, 0f, computer_half_depth);
            BoundingBox box = new BoundingBox(pos - half, pos + half + Vector3.Up);
            _walls.Add(box);

            _computers.Add(new Breakable
            {
                pos = pos,
                hp = computer_hp,
                sprite = _computer_atlas.CreateSprite("computer-undamaged"),
                box = box,
            });
        }

        private void ComputerHit(Breakable c)
        {
            int hit = computer_hp - c.hp;
            _computer_hit_sfx[Math.Min(hit, _computer_hit_sfx.Length - 1)].Play(0.8f, 0f, 0f);

            c.hp--;
            if (c.hp > 0)
            {
                c.sprite.Region = _computer_damaged_atlas.GetRegion("computer-damaged");
                c.hit_flash = hit_flash_time;
                return;
            }

            _walls.Remove(c.box);
            _computers.Remove(c);
            SpawnScrap(c.pos, computer_scrap_drop);
        }

        private void UpdatePickups(GameTime game_time)
        {
            float reach = scrap_pickup_radius + _player.def.radius;

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                Pickup p = _pickups[i];
                p.sprite.Update(game_time);

                if (FlatDistSquare(p.pos, _player.pos) < reach * reach)
                {
                    _scrap += p.value;
                    _collect_sfx.Play(0.8f, 0.5f, 0f);
                    _pickups.RemoveAt(i);
                    Window.Title = $"LudumDare60 - Scrap: {_scrap}";
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

        private void Aggro(Entity ent)
        {
            if (ent.ai == EnemyState.Idle || ent.ai == EnemyState.Walk)
                SetEnemyState(ent, EnemyState.Chase);
        }

        private void SeparateEntities()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                Entity a = _entities[i];
                if (a.dead) continue;

                PushApart(ref a.pos, ref _player.pos, a.def.radius, _player.def.radius);
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
            Vector2 n = d > 0.0001f ? new Vector2(dx / d, dz / d) : Vector2.UnitX;
            float half = (min - d) * 0.5f;
            a.X -= n.X * half; a.Z -= n.Y * half;
            b.X += n.X * half; b.Z += n.Y * half;
        }

        private void UpdateKnockback(Entity ent, float dt)
        {
            ent.pos += ent.vel * dt;

            foreach (Entity other in _entities)
            {
                if (other != ent && !other.dead)
                    Bounce(ent, other);
            }

            Vector3 wall_n = ResolveWalls(ref ent.pos, ent.def.radius);
            if (wall_n != Vector3.Zero)
            {
                wall_n.Normalize();
                float into = Vector3.Dot(ent.vel, wall_n);
                if (into < 0f)
                    ent.vel -= wall_n * into * (1f + bounce_restitution);
            }

            float speed = ent.vel.Length();
            float slowed = speed - knockback_friction * dt;
            ent.vel = slowed > 0f ? ent.vel * (slowed / speed) : Vector3.Zero;
        }

        private static void Bounce(Entity ent, Entity other)
        {
            Vector3 n = other.pos - ent.pos;
            n.Y = 0f;
            float min = ent.def.radius + other.def.radius;
            float d2 = n.LengthSquared();
            if (d2 >= min * min || d2 < 0.0001f) return;

            n /= MathF.Sqrt(d2);
            float into = Vector3.Dot(ent.vel, n);
            if (into <= 0f) return;

            ent.vel -= n * into * (1f + bounce_restitution);
            other.vel += n * into * bounce_transfer;
        }

        private void UpdateDying(Entity ent, float dt)
        {
            if (ent.vel != Vector3.Zero)
                UpdateKnockback(ent, dt);

            ent.death_timer += dt;
            if (ent.death_timer >= death_time)
            {
                ent.dead = true;
                if (ent.is_enemy)
                    SpawnScrap(ent.pos, enemy_scrap_drop);
            }
        }

        private static bool DeathFlickerHidden(Entity ent)
        {
            if (ent.hp > 0) return false;

            float t = ent.death_timer - death_hold_time;
            return t >= 0f && t % death_flicker_time < death_flicker_time * 0.5f;
        }

        private float SpinReach(Entity ent) => ent.def.spin_px / _billboards.PixelsPerUnit * ent.sprite.Scale.X;

        private float SpinTime(Entity ent)
        {
            Animation spin = ent.def.attack_anim;
            return ent.def.spins * spin.Frames.Count * (float)spin.Delay.TotalSeconds;
        }
        private static float FlatDistSquare(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return dx * dx + dz * dz;
        }

        private static float FlatDistSquareToBox(Vector3 p, BoundingBox box)
        {
            float dx = p.X - MathHelper.Clamp(p.X, box.Min.X, box.Max.X);
            float dz = p.Z - MathHelper.Clamp(p.Z, box.Min.Z, box.Max.Z);
            return dx * dx + dz * dz;
        }

        private int UpdateEnemy(Entity ent, Vector3 player_pos, float dt, GameTime game_time)
        {
            ent.state_timer += dt;
            ent.attack_cooldown = MathF.Max(0f, ent.attack_cooldown - dt);

            float hit_radius = SpinReach(ent) + _player.def.radius;
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
                        ent.def.attack_sfx?[0].Play(0.5f, 0f, 0f);
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

            return damage;
        }

        private void PlayerHit(Entity enemy, int damage)
        {
            _player.hit_flash = hit_flash_time;
            _hit_sfx.Play(0.8f, 0f, 0f);
        }

        private void PlayerSpinHits()
        {
            float reach = SpinReach(_player);
            foreach (Entity ent in _entities)
            {
                if (ent.dead || !ent.is_enemy || ent.hp <= 0) continue;

                float hit_radius = reach + ent.def.radius;
                if (FlatDistSquare(ent.pos, _player.pos) >= hit_radius * hit_radius) continue;
                if (!_spin_hits.Add(ent)) continue;

                EnemyHit(ent, spin_damage);
            }

            for (int i = _computers.Count - 1; i >= 0; i--)
            {
                Breakable c = _computers[i];
                if (FlatDistSquareToBox(_player.pos, c.box) >= reach * reach) continue;
                if (!_spin_hits.Add(c)) continue;

                ComputerHit(c);
            }
        }

        private void EnemyHit(Entity ent, int damage)
        {
            ent.hp -= damage;
            ent.hit_flash = hit_flash_time;
            _hit_sfx.Play(0.8f, 0f, 0f);

            Vector3 away = ent.pos - _player.pos;
            away.Y = 0f;
            away = away.LengthSquared() > 0.0001f ? Vector3.Normalize(away) : _player.facing;
            ent.vel = away * knockback_speed;

            Aggro(ent);
        }

        private void SetAnimation(RoboState state, Direction dir)
        {
            Entity p = _player;
            if (state == p.state && dir == p.dir)
                return;

            Animation anim = state == RoboState.Attack ? p.def.attack_anim : p.def.sprite_anims[(int)state, (int)dir];
            if (anim == null)
            {
                Debug.WriteLine($"ERROR: no animation for {state}, {dir}");
                return;
            }

            p.last_frame = -1;
            p.state = state;
            p.dir = dir;
            p.sprite.Animation = anim;
            bool flip = state != RoboState.Attack && p.def.sprite_flip[(int)state, (int)dir];
            p.sprite.Effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        // SFX => [ Attack, Footsteps, Jump?, COllision (walls and enemies), ...]
        // Music => Title screen? In-Game theme
        private void PlayStep(int foot)
        {
            _player.def.step_sfx[foot].Play(volume: 1f, pitch: 0f, pan: 0f);
        }

        private void UpdatePlayer(KeyboardState kb, float dt, GameTime game_time)
        {
            Entity p = _player;
            p.attack_cooldown = MathF.Max(0f, p.attack_cooldown - dt);

            Vector3 move = Vector3.Zero;
            if (kb.IsKeyDown(Keys.W)) { move += _camera.Forward; }
            if (kb.IsKeyDown(Keys.S)) { move -= _camera.Forward; }
            if (kb.IsKeyDown(Keys.D)) { move += _camera.Right; }
            if (kb.IsKeyDown(Keys.A)) { move -= _camera.Right; }

            if (p.state != RoboState.Attack && p.attack_cooldown <= 0f && Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                p.state_timer = 0f;
                _spin_hits.Clear();
                SetAnimation(RoboState.Attack, p.dir);
                p.def.attack_sfx[0].Play(0.7f, 0f, 0f);
            }

            bool attacking = p.state == RoboState.Attack;
            if (move != Vector3.Zero)
            {
                move.Normalize();
                float speed = attacking ? p.def.speed * player_spin_move_mult : p.def.speed;
                p.pos += move * speed * dt;
                p.facing = move;
            }

            if (attacking)
            {
                p.state_timer += dt;
                PlayerSpinHits();

                if (p.state_timer >= SpinTime(p))
                {
                    p.attack_cooldown = player_attack_cooldown;
                    attacking = false;
                }
            }

            int frame = p.sprite.CurrFrame;
            if (frame != p.last_frame)
            {
                if (p.state == RoboState.Walk)
                {
                    for (int foot = 0; foot < 2; foot++)
                    {
                        if (p.def.step_frames[(int)p.dir, foot] != frame)
                            continue;

                        PlayStep(foot);

                        if (Vector3.Dot(move, _player_wall_n) < 0f)
                            _wall_collision_sfx.Play(0.1f, 0f, 0f);
                    }
                }

                p.last_frame = frame;
            }

            if (!attacking)
            {
                RoboState state = move != Vector3.Zero ? RoboState.Walk : RoboState.Idle;
                Direction dir = PickDirection(p.facing, _camera);

                if (p.def.sprite_anims[(int)state, (int)dir] == null)
                    state = RoboState.Idle;
                SetAnimation(state, dir);
            }
            p.sprite.Update(game_time);
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Escape)) Exit();

            UpdatePlayer(kb, dt, gameTime);

            foreach (Entity ent in  _entities)
            {
                if (ent.dead || !ent.is_enemy)
                    continue;

                ent.hit_flash = MathF.Max(0f, ent.hit_flash - dt);
                ent.sprite.Color = ent.hit_flash > 0f ? Color.Red : Color.White;

                if (ent.hp <= 0)
                {
                    UpdateDying(ent, dt);
                }
                else if (ent.vel != Vector3.Zero)
                {
                    UpdateKnockback(ent, dt);
                }
                else
                {
                    int damage = UpdateEnemy(ent, _player.pos, dt, gameTime);
                    if (damage > 0)
                    {
                        PlayerHit(ent, damage);
                    }
                }
             }
            SeparateEntities();
            KeepOutOfWalls();
            UpdatePickups(gameTime);

            _player.hit_flash = MathF.Max(0f, _player.hit_flash - dt);
            _player.sprite.Color = _player.hit_flash > 0f ? Color.Red : Color.White;

            foreach (Breakable c in _computers)
            {
                c.hit_flash = MathF.Max(0f, c.hit_flash - dt);
                c.sprite.Color = c.hit_flash > 0f ? Color.Red : Color.White;
            }

            _camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            _camera.Target = ClampCameraTarget(_player.pos);


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

            _floor.Draw(view, projection);
            _wall_renderer.Draw(view, projection);

            _billboards.Begin(view, projection, _player.def.atlas.Texture);
            _billboards.Draw(_player.sprite, _player.pos);
            _billboards.End();

            _billboards.Begin(view, projection, _e_robo_atlas.Texture);
            foreach (Entity ent in _entities)
            {
                if (!ent.dead && !DeathFlickerHidden(ent))
                    _billboards.Draw(ent.sprite, ent.pos);
            }
            _billboards.End();

            _billboards.Begin(view, projection, _scrap_atlas.Texture);
            foreach (Pickup p in _pickups)
                _billboards.Draw(p.sprite, p.pos);
            _billboards.End();

            DrawComputers(view, projection, _computer_atlas.Texture);
            DrawComputers(view, projection, _computer_damaged_atlas.Texture);


            base.Draw(gameTime);
        }

        private void DrawComputers(Matrix view, Matrix projection, Texture2D texture)
        {
            _billboards.Begin(view, projection, texture);
            foreach (Breakable c in _computers)
            {
                if (c.sprite.Region.Texture == texture)
                    _billboards.Draw(c.sprite, c.pos);
            }
            _billboards.End();
        }

        private Color EnemyTint(Entity ent)
        {
            return Color.Aqua;
        }
    }
}