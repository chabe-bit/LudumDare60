using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class BillboardRenderer
    {
        const int MaxQuads = 1024;

        readonly GraphicsDevice _gd;
        readonly AlphaTestEffect _effect;
        readonly VertexPositionColorTexture[] _verts = new VertexPositionColorTexture[MaxQuads * 4];
        readonly short[] _indices = new short[MaxQuads * 6];

        int _count;
        Texture2D _texture;
        Vector3 _right, _up;

        public float PixelsPerUnit = 16f;   // 16 sprite pixels = 1 world unit

        public BillboardRenderer(GraphicsDevice gd)
        {
            _gd = gd;
            _effect = new AlphaTestEffect(gd)
            {
                AlphaFunction = CompareFunction.Greater,
                ReferenceAlpha = 128,          // pixels with alpha <= 128 are discarded
                VertexColorEnabled = true,   // ← use per-vertex tint
            };

            // indices never change: two triangles per quad
            for (int i = 0; i < MaxQuads; i++)
            {
                int v = i * 4, n = i * 6;
                _indices[n + 0] = (short)(v + 0);
                _indices[n + 1] = (short)(v + 1);
                _indices[n + 2] = (short)(v + 2);
                _indices[n + 3] = (short)(v + 0);
                _indices[n + 4] = (short)(v + 2);
                _indices[n + 5] = (short)(v + 3);
            }
        }

        public void Begin(Matrix view, Matrix projection, Texture2D atlasTexture)
        {
            _texture = atlasTexture;
            _count = 0;

            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;
            _effect.Texture = atlasTexture;

            // Y-axis (cylindrical) billboard: sprites stay upright, only turn left/right
            Vector3 camRight = Matrix.Invert(view).Right;
            _right = Vector3.Normalize(new Vector3(camRight.X, 0, camRight.Z));
            _up = Vector3.Up;
        }

        public void Draw(Sprite sprite, Vector3 feetPosition)
        {
            bool flipX = (sprite.Effects & SpriteEffects.FlipHorizontally) != 0;
            Draw(sprite.Region.SrcRectangle, feetPosition, sprite.Color, sprite.Scale, flipX);
        }


        public void Draw(Rectangle src, Vector3 feetPosition, Color color, Vector2 scale, bool flipX = false)
        {
            if (_count == MaxQuads) Flush();

            float w = src.Width / PixelsPerUnit;
            float h = src.Height / PixelsPerUnit;

            Vector3 halfRight = _right * (w / 2f);
            Vector3 upH = _up * h;

            // origin is at the sprite's feet, centered horizontally
            Vector3 bl = feetPosition - halfRight;
            Vector3 br = feetPosition + halfRight;
            Vector3 tl = bl + upH;
            Vector3 tr = br + upH;

            // texture coordinates from the atlas rectangle (0..1)
            float u0 = src.Left / (float)_texture.Width;
            float u1 = src.Right / (float)_texture.Width;
            float v0 = src.Top / (float)_texture.Height;
            float v1 = src.Bottom / (float)_texture.Height;
            if (flipX) (u0, u1) = (u1, u0);

            int v = _count * 4;
            _verts[v + 0] = new VertexPositionColorTexture(bl, color, new Vector2(u0, v1));
            _verts[v + 1] = new VertexPositionColorTexture(tl, color, new Vector2(u0, v0));
            _verts[v + 2] = new VertexPositionColorTexture(tr, color, new Vector2(u1, v0));
            _verts[v + 3] = new VertexPositionColorTexture(br, color, new Vector2(u1, v1));
            _count++;
        }

        public void End() => Flush();

        void Flush()
        {
            if (_count == 0) return;

            _gd.BlendState = BlendState.Opaque;
            _gd.DepthStencilState = DepthStencilState.Default;   // depth buffer ON
            _gd.RasterizerState = RasterizerState.CullNone;      // no winding worries
            _gd.SamplerStates[0] = SamplerState.PointClamp;      // crisp pixels

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _verts, 0, _count * 4,
                    _indices, 0, _count * 2);
            }
            _count = 0;
        }
    }
}
