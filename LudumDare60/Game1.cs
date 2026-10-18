using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
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

        World _world = new World();
        int _player;
        float _spawn_timer = 1f;
        const float spawn_interval = 3f;
        const int max_enemies = 10;
        Vector3 move = Vector3.Zero;


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

            Song theme = Content.Load<Song>("audio/music/theme");


            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(theme);
            MediaPlayer.IsRepeating = true;


            _step_sfx = Content.Load<SoundEffect>("audio/sfx/collect");

            _player = SpawnRobot(Vector3.Zero, 5f, Component.Input);
        }

        private int SpawnRobot(Vector3 pos, float speed, Component extra_flags)
        {
            int ent = _world.Create(Component.Position | Component.Move | Component.Sprite | extra_flags);
            if (ent < 0)
                return -1;

            _world._pos[ent] = pos;
            _world._facing[ent] = Vector3.Backward;
            _world._speed[ent] = speed;
            _world._sprite[ent] = _robo_atlas.CreateAnimatedSprite("robo-idle-front-animation");
            _world._anim[ent] = new AnimState { state = RoboState.Idle, dir = Direction.Front, last_frame = -1 };

            return ent;
        }

        // Abstracting away Keyboard 
        private void InputSystem(KeyboardState kb)
        {
            if (kb.IsKeyDown(Keys.Escape)) Exit();

        }

        private void MovementSystem(float dt)
        {
            if (move != Vector3.Zero)
            {
                move.Normalize();
                _robo_pos += move * 5f * dt;
                _robo_facing = move;
            }
        }

        private void AnimationSystem(GameTime game_time)
        {

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
            
            /*        
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
            */

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