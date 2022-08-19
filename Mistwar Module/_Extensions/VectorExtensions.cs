using System;
using Blish_HUD;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
namespace Nekres.Mistwar
{
    internal static class VectorExtensions
    {
        public static Vector2 XY(this Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }

        public static Vector2 XY(this System.Numerics.Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }

        public static Vector3 ToScreenSpace(this Vector3 position, Matrix view, Matrix projection)
        {
            int screenWidth = GameService.Graphics.SpriteScreen.Width;
            int screenHeight = GameService.Graphics.SpriteScreen.Height;

            position = Vector3.Transform(position, view);
            position = Vector3.Transform(position, projection);

            float x = position.X / position.Z;
            float y = position.Y / -position.Z;

            x = (x + 1) * screenWidth / 2;
            y = (y + 1) * screenHeight / 2;

            return new Vector3(x, y, position.Z);
        }

        public static Vector2 Flatten(this Vector3 v)
        {
            return new Vector2((v.X / v.Z + 1f) / 2f * (float)GameService.Graphics.SpriteScreen.Width, (1f - v.Y / v.Z) / 2f * (float)GameService.Graphics.SpriteScreen.Height);
        }

        public static Vector2 ToXnaVector2(this Coordinates2 coords)
        {
            return new Vector2((float)coords.X, (float)coords.Y);
        }

        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).Length();
        }

        public static double Angle(this Vector3 v, Vector3 u)
        {
            return Math.Acos(Vector3.Dot(v, u) / (v.Length() * u.Length()));
        }
    }
}
