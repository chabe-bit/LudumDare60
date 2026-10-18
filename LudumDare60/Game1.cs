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
        
        enum EnemyState { Idle, Walk, Chase, Attack };
        TextureAtlas _e_robo_atlas;
        Animation[] _e_robo_anims = new Animation[3];
        AnimatedSprite _e_robo;
        Vector3 _e_robo_facing = Vector3.Backward;
        Vector3 _e_robo_pos;
        EnemyState _e_robo_state = EnemyState.Idle;
        Direction _e_robo_dir = Direction.Left;
        int _e_robo_idle = (int)EnemyState.Idle;
        int _e_robo_attack = (int)EnemyState.Attack;


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
            _robo.Scale = new Vector2(0.5f, 0.5f);


            _e_robo_atlas = TextureAtlas.FromFile(Content, "images/enemy-atlas-definition.xml");
            _e_robo_anims[_e_robo_attack] = _e_robo_atlas.GetAnimation("enemy-attack-animation");
            _e_robo = _e_robo_atlas.CreateAnimatedSprite("enemy-walk-animation");
            _e_robo.Scale = new Vector2(0.5f, 0.5f);

            



            Song theme = Content.Load<Song>("audio/music/theme");


            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(theme);
            MediaPlayer.IsRepeating = true;


            _step_sfx = Content.Load<SoundEffect>("audio/sfx/collect");

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


            _billboards.End();

            base.Draw(gameTime);
        }
    }
}