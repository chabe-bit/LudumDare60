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
        TextureAtlas _atlas;
        AnimatedSprite _slime;
        TextureRegion _bat;

        Camera3D _camera;
        GroundGrid _grid;
        BillboardRenderer _billboards;

        Vector3 _slimePos = Vector3.Zero;            // world position (feet), Y = 0 is the ground
        Vector3 _batPos = new Vector3(3, 1.5f, -2);  // floating a bit above the ground
        bool _slimeFacingLeft;

        TextureAtlas _robo_atlas;
        enum Direction { Front, Back, Left, Right };
        enum RoboState { Idle, Walk };
        Animation[,] _robo_anims = new Animation[2, 4];
        AnimatedSprite _robo;
        RoboState _robo_state = RoboState.Idle;
        Direction _robo_dir = Direction.Front;
        Vector3 _robo_facing = Vector3.Backward;
        Vector3 _robo_pos;
        int _robo_idle = (int)RoboState.Idle;
        int _robo_walk = (int)RoboState.Walk;

        // Audio -> SFX and Music

        
        public Game1() : base("Dungeon Slime", 1280, 720, false) { }

        protected override void Initialize()
        {
            base.Initialize();   // in this template, Initialize calls LoadContent

            _camera = new Camera3D();
            _grid = new GroundGrid(GraphicsDevice);
            _billboards = new BillboardRenderer(GraphicsDevice) { PixelsPerUnit = 250f };
        }

        protected override void LoadContent()
        {
            _atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _slime = _atlas.CreateAnimatedSprite("slime-animation");

            _robo_atlas = TextureAtlas.FromFile(Content, "images/robo-atlas-definition.xml");
            _robo_anims[_robo_idle, (int)Direction.Front]  = _robo_atlas.GetAnimation("robo-idle-front-animation");
            _robo_anims[_robo_idle, (int)Direction.Back]   = _robo_atlas.GetAnimation("robo-idle-back-animation");
            _robo_anims[_robo_idle, (int)Direction.Right]  = _robo_atlas.GetAnimation("robo-idle-right-animation");
            _robo_anims[_robo_idle, (int)Direction.Left]   = _robo_anims[_robo_idle, (int)Direction.Right];

            _robo_anims[_robo_walk, (int)Direction.Front]  = _robo_atlas.GetAnimation("robo-walk-front-animation");
            _robo_anims[_robo_walk, (int)Direction.Back]   = _robo_atlas.GetAnimation("robo-walk-back-animation");
            _robo_anims[_robo_walk, (int)Direction.Right]  = _robo_atlas.GetAnimation("robo-walk-right-animation");
            _robo_anims[_robo_walk, (int)Direction.Left]   = _robo_anims[_robo_walk, (int)Direction.Right];


            _robo = _robo_atlas.CreateAnimatedSprite("robo-idle-front-animation");
            _robo.Scale = new Vector2(0.5f, 0.5f);
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

            _robo_state = state;
            _robo_dir = dir;
            _robo.Animation = anim;
            _robo.Effects = dir == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kb.IsKeyDown(Keys.Escape)) Exit();

            // --- camera orbit: Q/E rotate, R/F tilt, Z/X zoom ---
            float rotSpeed = MathHelper.ToRadians(90f);
            if (kb.IsKeyDown(Keys.Q)) _camera.Yaw -= rotSpeed * dt;
            if (kb.IsKeyDown(Keys.E)) _camera.Yaw += rotSpeed * dt;
            if (kb.IsKeyDown(Keys.R)) _camera.Pitch += rotSpeed * dt;
            if (kb.IsKeyDown(Keys.F)) _camera.Pitch -= rotSpeed * dt;
            if (kb.IsKeyDown(Keys.Z)) _camera.Distance -= 10f * dt;
            if (kb.IsKeyDown(Keys.X)) _camera.Distance += 10f * dt;
            _camera.ClampPitch();
            _camera.Distance = MathHelper.Clamp(_camera.Distance, 3f, 40f);

            // --- slime movement: WASD relative to where the camera faces ---
            Vector3 move = Vector3.Zero;
            if (kb.IsKeyDown(Keys.W)) move += _camera.Forward;
            if (kb.IsKeyDown(Keys.S)) move -= _camera.Forward;
            if (kb.IsKeyDown(Keys.D)) move += _camera.Right;
            if (kb.IsKeyDown(Keys.A)) move -= _camera.Right;

            if (move != Vector3.Zero)
            {
                move.Normalize();
                _robo_pos += move * 5f * dt;
                _robo_facing = move;
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
            _billboards.Draw(_robo, _robo_pos);
            _billboards.End();

            base.Draw(gameTime);
        }
    }
}