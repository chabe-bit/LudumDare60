using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class WallRenderer
    {
        readonly GraphicsDevice _gd;
        readonly BasicEffect _effect;
        readonly VertexPositionColorTexture[] _verts;

        static readonly Color s_side = new Color(200, 200, 200);

        public WallRenderer(GraphicsDevice gd, TextureRegion art, float height, IEnumerable<(Vector3 start, Vector3 end)> runs)
        {
            _gd = gd;
            _effect = new BasicEffect(gd) { TextureEnabled = true, VertexColorEnabled = true, Texture = art.Texture };

            Rectangle src = art.SrcRectangle;
            float tw = art.Texture.Width, th = art.Texture.Height;
            float u0 = src.Left / tw, u1 = src.Right / tw;
            float v0 = src.Top / th, v1 = src.Bottom / th;

            var verts = new List<VertexPositionColorTexture>();
            foreach (var (start, end) in runs)
            {
                Vector3 hi = Vector3.Up * height;

                Vector3 run = end - start;
                Color col = MathF.Abs(run.Z) > MathF.Abs(run.X) ? s_side : Color.White;

                verts.Add(new VertexPositionColorTexture(start, col, new Vector2(u0, v1)));
                verts.Add(new VertexPositionColorTexture(end, col, new Vector2(u1, v1)));
                verts.Add(new VertexPositionColorTexture(end + hi, col, new Vector2(u1, v0)));
                verts.Add(new VertexPositionColorTexture(start, col, new Vector2(u0, v1)));
                verts.Add(new VertexPositionColorTexture(end + hi, col, new Vector2(u1, v0)));
                verts.Add(new VertexPositionColorTexture(start + hi, col, new Vector2(u0, v0)));
            }
            _verts = verts.ToArray();
        }

        public void Draw(Matrix view, Matrix projection)
        {
            if (_verts.Length == 0) return;

            _gd.BlendState = BlendState.Opaque;
            _gd.DepthStencilState = DepthStencilState.DepthRead;
            _gd.RasterizerState = RasterizerState.CullNone;
            _gd.SamplerStates[0] = SamplerState.AnisotropicClamp;

            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _verts, 0, _verts.Length / 3);
            }
        }
    }
}
