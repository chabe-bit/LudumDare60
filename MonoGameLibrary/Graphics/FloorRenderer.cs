using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class FloorRenderer
    {
        readonly GraphicsDevice _gd;
        readonly BasicEffect _effect;
        readonly VertexPositionTexture[] _verts;
        readonly short[] _indices;

        public FloorRenderer(GraphicsDevice gd, TextureRegion tile, float pixelsPerUnit, float halfSize = 20f)
        {
            _gd = gd;
            _effect = new BasicEffect(gd) { TextureEnabled = true, Texture = tile.Texture };

            float tileW = tile.Width / pixelsPerUnit;
            float tileD = tile.Height / pixelsPerUnit;
            int countX = (int)MathF.Ceiling(halfSize * 2f / tileW);
            int countZ = (int)MathF.Ceiling(halfSize * 2f / tileD);

            Rectangle src = tile.SrcRectangle;
            float u0 = src.Left / (float)tile.Texture.Width;
            float u1 = src.Right / (float)tile.Texture.Width;
            float v0 = src.Top / (float)tile.Texture.Height;
            float v1 = src.Bottom / (float)tile.Texture.Height;

            _verts = new VertexPositionTexture[countX * countZ * 4];
            _indices = new short[countX * countZ * 6];

            int q = 0;
            for (int z = 0; z < countZ; z++)
            {
                for (int x = 0; x < countX; x++)
                {
                    float x0 = -countX * tileW / 2f + x * tileW;
                    float z0 = -countZ * tileD / 2f + z * tileD;

                    int v = q * 4, n = q * 6;
                    _verts[v + 0] = new VertexPositionTexture(new Vector3(x0, 0, z0), new Vector2(u0, v0));
                    _verts[v + 1] = new VertexPositionTexture(new Vector3(x0 + tileW, 0, z0), new Vector2(u1, v0));
                    _verts[v + 2] = new VertexPositionTexture(new Vector3(x0 + tileW, 0, z0 + tileD), new Vector2(u1, v1));
                    _verts[v + 3] = new VertexPositionTexture(new Vector3(x0, 0, z0 + tileD), new Vector2(u0, v1));

                    _indices[n + 0] = (short)(v + 0);
                    _indices[n + 1] = (short)(v + 1);
                    _indices[n + 2] = (short)(v + 2);
                    _indices[n + 3] = (short)(v + 0);
                    _indices[n + 4] = (short)(v + 2);
                    _indices[n + 5] = (short)(v + 3);
                    q++;
                }
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            _gd.DepthStencilState = DepthStencilState.Default;
            _gd.BlendState = BlendState.Opaque;
            _gd.RasterizerState = RasterizerState.CullNone;
            _gd.SamplerStates[0] = SamplerState.AnisotropicClamp;

            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _verts, 0, _verts.Length,
                    _indices, 0, _indices.Length / 3);
            }
        }
    }
}
