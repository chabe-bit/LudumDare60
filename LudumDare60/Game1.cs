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
using MonoGameLibrary.Graphics;
using System.Data; // We now have access to the Texture methods from MGL

namespace LudumDare60
{
    public class Game1 : Core
    {
        private Sprite _slime;
        private Sprite _bat;

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

            /* 
            Texture2D atlas_texture = Content.Load<Texture2D>("images/atlas");
            TextureAtlas atlas = new TextureAtlas(atlas_texture);

            // Created the regions and ID'd by name  
            atlas.AddRegion("slime", 0, 0, 20, 20);
            atlas.AddRegion("bat", 20, 0, 20, 20);
            */

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _slime = atlas.CreateSprite("slime");
            _bat = atlas.CreateSprite("bat");

            _bat = atlas.CreateSprite("bat");
            _bat.Scale = new Vector2(4.0f, 4.0f);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            SpriteBatch.Begin();
            _slime.Draw(SpriteBatch, Vector2.Zero);
            _bat.Draw(SpriteBatch, new Vector2(_slime.Width + 10, 0));

            SpriteBatch.End();

            base.Draw(gameTime);       
        }
    }
}


