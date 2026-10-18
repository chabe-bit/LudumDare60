/*
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace LudumDare60
{
    public class Game1 : Core
    {
        TextureAtlas _atlas;
        TextureRegion _slime;
        TextureRegion _bat;

        Camera3D _camera;
        GroundGrid _grid;
        BillboardRenderer _billboards;

        Vector3 _slimePos = Vector3.Zero;            // world position (feet), Y = 0 is the ground
        Vector3 _batPos = new Vector3(3, 1.5f, -2);  // floating a bit above the ground
        bool _slimeFacingLeft;

        public Game1() : base("Dungeon Slime", 1280, 720, false) { }

        protected override void Initialize()
        {
            base.Initialize();   // in this template, Initialize calls LoadContent

            _camera = new Camera3D();
            _grid = new GroundGrid(GraphicsDevice);
            _billboards = new BillboardRenderer(GraphicsDevice) { PixelsPerUnit = 16f };
        }

        protected override void LoadContent()
        {
            _atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _slime = _atlas.GetRegion("slime");
            _bat = _atlas.GetRegion("bat");
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
                _slimePos += move * 5f * dt;

                // flip sprite based on screen-relative left/right
                float sideways = Vector3.Dot(move, _camera.Right);
                if (sideways < -0.1f) _slimeFacingLeft = true;
                if (sideways > 0.1f) _slimeFacingLeft = false;
            }

            // camera follows the slime
            _camera.Target = _slimePos;
            _camera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);   // also clears the depth buffer

            Matrix view = _camera.View;
            Matrix projection = _camera.Projection;

            _grid.Draw(view, projection);

            _billboards.Begin(view, projection, _atlas.Texture);
            _billboards.Draw(_slime.SrcRectangle, _slimePos, _slimeFacingLeft);
            _billboards.Draw(_bat.SrcRectangle, _batPos);
            _billboards.End();

            base.Draw(gameTime);
        }
    }
}
*/


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameLibrary;
using MonoGameLibrary.Graphics; // We now have access to the Texture methods from MGL

namespace LudumDare60
{
    public class Game1 : Core
    {
        private AnimatedSprite _slime;
        private Vector2 _slime_pos;

        // Robo idle anim 
        private AnimatedSprite _robo_idle_front;
        private Vector2 robo_pos;


        private const float MOVEMENT_SPEED = 5.0f;

        public Game1() : base("LudumDare60", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

          

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _slime = atlas.CreateAnimatedSprite("slime-animation");
            _slime.Scale = new Vector2(4.0f, 4.0f);

            TextureAtlas _robo_atlas = TextureAtlas.FromFile(Content, "images/robo-atlas-definition.xml");

            _robo_idle_front = _robo_atlas.CreateAnimatedSprite("robo-idle-front-animation");
            _robo_idle_front.Scale = new Vector2(0.5f, 0.5f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _slime.Update(gameTime);
            _robo_idle_front.Update(gameTime);

            CheckKeyboardInput();
            CheckGamePadInput();

            base.Update(gameTime);
        }

        private void CheckKeyboardInput()
        {
            KeyboardState keyboard_state = Keyboard.GetState();
            float speed = MOVEMENT_SPEED;

            if (keyboard_state.IsKeyDown(Keys.Space))
            {
                
            }

            if (keyboard_state.IsKeyDown(Keys.W))
            {
                _slime_pos.Y -= speed;
            }

            if (keyboard_state.IsKeyDown(Keys.S))
            {
                _slime_pos.Y += speed;
            }

            if (keyboard_state.IsKeyDown(Keys.A))
            {
                _slime_pos.X -= speed;
            }

            if (keyboard_state.IsKeyDown(Keys.D))
            {
                _slime_pos.X += speed;
            }
        }




        private void CheckGamePadInput()
        {

        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _slime.Draw(SpriteBatch, _slime_pos);

            _robo_idle_front.Draw(SpriteBatch, _slime_pos);
                
            SpriteBatch.End();

            base.Draw(gameTime);       
        }
    }
}
