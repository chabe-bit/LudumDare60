using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameLibrary;

namespace LudumDare60
{
    public class Game1 : Core
    {
        private Texture2D _logo;



        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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

            _logo = Content.Load<Texture2D>("images/monogames_logo");

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
            Vector2 pos;
            pos.X = (Window.ClientBounds.Width * 0.5f) - (_logo.Width * 0.5f);
            pos.Y = (Window.ClientBounds.Height * 0.5f) - (_logo.Height * 0.5f);

            SpriteBatch.Begin();
            SpriteBatch.Draw(_logo, 
                            pos, 
                            Color.White);
            SpriteBatch.End();
            

            base.Draw(gameTime);       
        }
    }
}
