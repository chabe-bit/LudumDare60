using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class GroundGrid
    {
        readonly GraphicsDevice _gd;
        readonly BasicEffect _effect;
        readonly VertexPositionColor[] _lines;

        public GroundGrid(GraphicsDevice gd, int halfSize = 20, float spacing = 1f)
        {
            _gd = gd;
            _effect = new BasicEffect(gd) { VertexColorEnabled = true };

            int count = halfSize * 2 + 1;
            _lines = new VertexPositionColor[count * 4];
            float extent = halfSize * spacing;
            int i = 0;

            for (int n = -halfSize; n <= halfSize; n++)
            {
                float p = n * spacing;
                Color c = n == 0 ? Color.White : Color.Gray;   // highlight the axes

                // line along Z
                _lines[i++] = new VertexPositionColor(new Vector3(p, 0, -extent), c);
                _lines[i++] = new VertexPositionColor(new Vector3(p, 0, extent), c);
                // line along X
                _lines[i++] = new VertexPositionColor(new Vector3(-extent, 0, p), c);
                _lines[i++] = new VertexPositionColor(new Vector3(extent, 0, p), c);
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            _gd.DepthStencilState = DepthStencilState.Default;
            _gd.BlendState = BlendState.Opaque;

            _effect.World = Matrix.Identity;
            _effect.View = view;
            _effect.Projection = projection;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _gd.DrawUserPrimitives(PrimitiveType.LineList, _lines, 0, _lines.Length / 2);
            }
        }
    }
}
