using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Triangulation;

namespace Nekres.Mistwar
{
    public static class SpriteBatchExtensions
    {
        public static void DrawPolygonFill(this SpriteBatch spriteBatch, Vector2 position, Vector2[] vertices, Color color, bool outline = true)
        { 
            int count = vertices.Length;

            if (count == 2)
            {
                spriteBatch.DrawPolygon(position, vertices, color);
                return;
            }

            Color colorFill = color * (outline ? 0.5f : 1.0f);

            Vector2[] outVertices;
            int[] outIndices;
            Triangulator.Triangulate(vertices, WindingOrder.CounterClockwise, out outVertices, out outIndices);

            var triangleVertices = new List<VertexPositionColor>();

            for (int i = 0; i < outIndices.Length - 2; i += 3)
            {
                triangleVertices.Add(GetVertexPositionColor(new Vector2(outVertices[outIndices[i]].X + position.X, outVertices[outIndices[i]].Y + position.Y), colorFill));
                triangleVertices.Add(GetVertexPositionColor(new Vector2(outVertices[outIndices[i + 1]].X + position.X, outVertices[outIndices[i + 1]].Y + position.Y), colorFill));
                triangleVertices.Add(GetVertexPositionColor(new Vector2(outVertices[outIndices[i + 2]].X + position.X, outVertices[outIndices[i + 2]].Y + position.Y), colorFill));
            }

            spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleVertices.ToArray(), 0, triangleVertices.Count / 3);

            if (outline)
                spriteBatch.DrawPolygon(position, vertices, color);
        }

        private static VertexPositionColor GetVertexPositionColor(Vector2 vertex, Color color)
        {
            return new VertexPositionColor(new Vector3(vertex, -0.1f), color);
        }
    }
}
