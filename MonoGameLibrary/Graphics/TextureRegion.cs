using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoGameLibrary.Graphics
{
    public class TextureRegion
    {
        public Texture2D Texture { get; set; }

        public Rectangle SrcRectangle { get; set; }

        public int Width => SrcRectangle.Width;
        public int Height => SrcRectangle.Height;

        
        public TextureRegion(Texture2D texture, int x, int y, int width, int height)
        {
            // Pass in external texture into worker texture
            Texture = texture;

            // Target location within the texture
            SrcRectangle = new Rectangle(x, y, width, height); 
        }


        // Basic rendering of pos and color 
        public void Draw(SpriteBatch sprite_batch, Vector2 pos, Color color)
        {
            Draw(sprite_batch, pos, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
        }

        // Extra paremeters for rendering, offering scaling to be a single float value
        public void Draw(SpriteBatch sprite_batch, Vector2 pos, Color color, float rot, Vector2 origin, float scale, SpriteEffects effects, float layer_depth)
        {
            Draw(
                sprite_batch,
                pos,
                color,
                rot,
                origin,
                new Vector2(scale, scale),
                effects,
                layer_depth);
        }

        // Extra parameters for rendering, offers individual X and Y scale param control 
        public void Draw(SpriteBatch sprite_batch, Vector2 pos, Color color, float rot, Vector2 origin, Vector2 scale, SpriteEffects effects, float layer_depth)
        {
            sprite_batch.Draw(
               Texture,
               pos,
               SrcRectangle,
               color,
               rot,
               origin,
               scale,
               effects,
               layer_depth);
        }
    }
}
