using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nekres.Mistwar
{
    public enum ColorType
    {
        Normal,
        Protanopia,
        Protanomaly,
        Deuteranopia,
        Deuteranomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly
    }

    public static class ColorExtensions
    {
        private static readonly Dictionary<ColorType, float[][]> ColorTypes = new Dictionary<ColorType, float[][]>
        {
            {
                ColorType.Normal,
                new float[][]
                {
                    new float[] {1, 0, 0, 0, 0}, 
                    new float[] {0, 1, 0, 0, 0}, 
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0}, 
                    new float[] {0, 0, 0, 0, 1}
                }
            }, 
            {
                ColorType.Protanopia, 
                new float[][]
                {
                    new float[] {0.567f, 0.433f, 0, 0, 0},
                    new float[] {0.558f, 0.442f, 0, 0, 0},
                    new float[] {0, 0.242f, 0.758f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                }
            },
            { 
                ColorType.Protanomaly, 
                new float[][]
                {
                    new float[]{0.817f,0.183f,0,0,0},
                    new float[]{0.333f,0.667f,0,0,0},
                    new float[]{0,0.125f,0.875f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Deuteranopia,
                new float[][]
                {
                    new float[]{0.625f,0.375f,0,0,0},
                    new float[]{0.7f,0.3f,0,0,0},
                    new float[]{0,0.3f,0.7f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Deuteranomaly,
                new float[][]
                {
                    new float[]{0.8f,0.2f,0,0,0},
                    new float[]{0.258f,0.742f,0,0,0},
                    new float[]{0,0.142f,0.858f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Tritanopia,
                new float[][]
                {
                    new float[]{0.95f,0.05f,0,0,0},
                    new float[]{0,0.433f,0.567f,0,0},
                    new float[]{0,0.475f,0.525f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Tritanomaly,
                new float[][]
                {
                    new float[]{0.967f,0.033f,0,0,0},
                    new float[]{0,0.733f,0.267f,0,0},
                    new float[]{0,0.183f,0.817f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Achromatopsia,
                new float[][]
                {
                    new float[]{0.299f,0.587f,0.114f,0,0},
                    new float[]{0.299f,0.587f,0.114f,0,0},
                    new float[]{0.299f,0.587f,0.114f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            },
            {
                ColorType.Achromatomaly,
                new float[][]
                {
                    new float[]{0.618f,0.320f,0.062f,0,0},
                    new float[]{0.163f,0.775f,0.062f,0,0},
                    new float[]{0.163f,0.320f,0.516f,0,0},
                    new float[]{0,0,0,1,0},
                    new float[]{0,0,0,0,1},
                }
            }
        };

        private static int Clamp(float n) => (int)(n < 0 ? 0 : n < 255 ? n : 255);
        public static Color GetColorBlindType(this Color color, ColorType type, int overwriteAlpha = -1)
        {
            var m = ColorTypes[type];
            var r = color.R * m[0][0] + color.G * m[0][1] + color.B * m[0][2] + color.A * m[0][3] + m[0][4];
            var g = color.R * m[1][0] + color.G * m[1][1] + color.B * m[1][2] + color.A * m[1][3] + m[1][4];
            var b = color.R * m[2][0] + color.G * m[2][1] + color.B * m[2][2] + color.A * m[2][3] + m[2][4];
            var a = color.R * m[3][0] + color.G * m[3][1] + color.B * m[3][2] + color.A * m[3][3] + m[3][4];

            return new Color(Clamp(r), Clamp(g), Clamp(b),  Clamp(overwriteAlpha > -1 ? overwriteAlpha : a));
        }
        /// <summary>
        /// Convert Xna Color (Microsoft.Xna.Framework) to Drawing Color (WinForm)
        /// </summary>
        /// <param name="mediaColor"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Drawing.Color ToDrawingColor(this Microsoft.Xna.Framework.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        /// <summary>
        /// Convert Drawing Color (WinForm) to Xna Color (Microsoft.Xna.Framework)
        /// </summary>
        /// <param name="drawingColor"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return new Microsoft.Xna.Framework.Color(drawingColor.R, drawingColor.G, drawingColor.B, drawingColor.A);
        }
    }
}
