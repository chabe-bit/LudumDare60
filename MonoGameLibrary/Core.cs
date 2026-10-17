using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary
{
    public class Core : Game
    {
        // Variables 
        internal static Core s_instance;
        public static Core Instance => s_instance; // Takes a copy of the real set, such that if "Instance" were to change in a funky way, it'd have a point of reference of what should be 
        public static GraphicsDeviceManager Graphics { get; private set; }
        public static new GraphicsDevice GraphicsDevice { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static new ContentManager Content { get; private set; }


        // Funcs 
        public Core(string title, int width, int height, bool is_full_screen)
        {
            if (s_instance != null)
            {
                throw new InvalidOperationException($"Only one Core!");
            }

            s_instance = this;

            Graphics = new GraphicsDeviceManager(this);

            // Backbuffer is where our pixels are to be rendered, refreshed, then rendered on again
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;

            // No access to the front buffer, it's not allowed since per operating system their use cases are 
            // different. That is abstracted and handled by the lib itself so no one fucks anything up.

            Graphics.IsFullScreen = is_full_screen;

            Graphics.ApplyChanges();

            Window.Title = title;

            Content = base.Content;

            Content.RootDirectory = "Content"; // Looks at the project folder AS our root directory and looks for "Content" folder.

            IsMouseVisible = true; // turn off later 
        }

        // Protected means this function from this class cannot be accessed externally, other than those that 
        // derive from this class. Family calls family. 
        protected override void Initialize()
        {
            base.Initialize();
            GraphicsDevice = base.GraphicsDevice; // Initted this copy with the original 
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

    }
}