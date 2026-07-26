// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Special functions for working with colors. MathUtil also contains operations dealing with color.
    /// </summary>
    public static class ColorUtil
    {
        private static float LastFilteredHue;
        private static float CycleFilteredHue = 0.1234f;

        /// <summary>
        /// Create a new color with 256 color values.
        /// </summary>
        public static Color NewColor(int r, int g, int b)
        {
            return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
        }

        public static Color SetAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Generates a random color
        /// </summary>
        public static Color Random()
        {
            Color color = new Color(MathUtil.Random(), MathUtil.Random(), MathUtil.Random(), 1f);
            return color;
        }

        /// <summary>
        /// Generates a random color
        /// </summary>
        public static Color RandomHue()
        {
            return ColorUtil.HLSColor(UnityEngine.Random.value, 1f, 1f);
        }

        public static Color RandomHue(float saturation, float value)
        {
            return ColorUtil.HLSColor(UnityEngine.Random.value, saturation, value);
        }

        public static float GetNextFilteredHue()
        {
            LastFilteredHue += CycleFilteredHue;
            if (LastFilteredHue > 1f) LastFilteredHue -= 1f;
            LastFilteredHue = FilterHue(LastFilteredHue);
            return LastFilteredHue;
        }

        public static Color RandomHueFiltered()
        {
            return ColorUtil.HLSColor(GetNextFilteredHue(), 1f, 1f);
        }

        public static Color RandomHueFiltered(float saturation, float value)
        {
            return ColorUtil.HLSColor(GetNextFilteredHue(), saturation, value);
        }

        public static float FilterHue(float hue)
        {
            float tolerance = 0.1f;
            float green = 120f / 360f;
            float yellow = 60f / 360f;

            if (Mathf.Abs(hue - yellow) > tolerance) {
                hue += tolerance;
            }
            if (Mathf.Abs(hue - green) > tolerance) {
                hue += tolerance;
            }
            return hue;
        }

        public static float GetHue(Color color)
        {
            float hue, sat, val;
            Color.RGBToHSV(color, out hue, out sat, out val);
            return hue;
        }

        public static float GetLightness(Color color)
        {
            float hue, sat, val;
            Color.RGBToHSV(color, out hue, out sat, out val);
            return val;
        }

        public static float GetSaturation(Color color)
        {
            float hue, sat, val;
            Color.RGBToHSV(color, out hue, out sat, out val);
            return sat;
        }

        public static Color SetHue(Color color, float hue)
        {
            float hue2, sat, val;
            Color.RGBToHSV(color, out hue2, out sat, out val);
            return Color.HSVToRGB(hue, sat, val);
        }

        public static Color SetSaturation(Color color, float saturation)
        {
            float hue, sat, val;
            Color.RGBToHSV(color, out hue, out sat, out val);
            return Color.HSVToRGB(hue, saturation, val);
        }

        public static Color InterpolateHue(Color color1, Color color2, float amount)
        {
            float h1, h2, s1, s2, v1, v2;

            Color.RGBToHSV(color1, out h1, out s1, out v1);
            Color.RGBToHSV(color2, out h2, out s2, out v2);

            h1 = MathUtil.Interpolate(h1, h2, amount);
            s1 = MathUtil.Interpolate(s1, s2, amount);
            v1 = MathUtil.Interpolate(v1, v2, amount);

            color1 = Color.HSVToRGB(h1, s1, v1);

            return color1;
        }

        /// <summary>
        /// Create a color using hue, saturation, and value.
        /// </summary>
        public static Color HLSColor(float hue, float saturation, float value)
        {
            hue = Mathf.Clamp(hue, 0f, 1f);
            saturation = Mathf.Clamp(saturation, 0.001f, 1f);
            value = Mathf.Clamp(value, 0f, 1f);

            Color c = Color.black;
            if (saturation > 0) {
                float h = hue * 6f;

                int hueIndex = (int)h;
                float fract = h - (float)hueIndex;
                float aa = value * (1f - saturation);
                float bb = value * (1f - (saturation * fract));
                float cc = value * (1f - (saturation * (1f - fract)));

                switch (hueIndex) {
                    case 0:
                        c.r = value;
                        c.g = cc;
                        c.b = aa;
                        break;
                    case 1:
                        c.r = bb;
                        c.g = value;
                        c.b = aa;
                        break;
                    case 2:
                        c.r = aa;
                        c.g = value;
                        c.b = cc;
                        break;
                    case 3:
                        c.r = aa;
                        c.g = bb;
                        c.b = value;
                        break;
                    case 4:
                        c.r = cc;
                        c.g = aa;
                        c.b = value;
                        break;
                    case 5:
                        c.r = value;
                        c.g = aa;
                        c.b = bb;
                        break;
                    default:
                        c.r = value;
                        c.g = cc;
                        c.b = aa;
                        break;
                }
            }
            return c;
        }

        /// <summary>
        /// Randomize color by the amount specified by randomizeAmount. Values of 0 mean no randomization,
        /// while 1 is full randomization.
        /// </summary>
        public static Color Randomize(Color color, Color randomizeAmount)
        {
            Color v = color;
            v.r += (UnityEngine.Random.value - 0.5f) * 2.0f * randomizeAmount.r;
            v.g += (UnityEngine.Random.value - 0.5f) * 2.0f * randomizeAmount.g;
            v.b += (UnityEngine.Random.value - 0.5f) * 2.0f * randomizeAmount.b;
            v.a += (UnityEngine.Random.value - 0.5f) * 2.0f * randomizeAmount.a;
            v.r = Mathf.Min(Mathf.Max(v.r, 0.0f), 1.0f);
            v.g = Mathf.Min(Mathf.Max(v.g, 0.0f), 1.0f);
            v.b = Mathf.Min(Mathf.Max(v.b, 0.0f), 1.0f);
            v.a = Mathf.Min(Mathf.Max(v.a, 0.0f), 1.0f);
            return v;
        }

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        public static Color32 HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        /// <summary>
        /// Generates a random color
        /// </summary>
        public static Color Limit(Color color)
        {
            if (color.a < 0f) color.a = 0f;
            else
            if (color.a > 1f) color.a = 1f;

            if (color.r < 0f) color.r = 0f;
            else
            if (color.r > 1f) color.r = 1f;

            if (color.g < 0f) color.g = 0f;
            else
            if (color.g > 1f) color.g = 1f;

            if (color.b < 0f) color.b = 0f;
            else
            if (color.b > 1f) color.b = 1f;

            return color;
        }

        public static Color Gradient(float value, bool interpolateHue, Color[] colors)
        {
            Color color = Color.black;

            if (colors.Length == 1) {
                color = colors[0];
            }
            else
            if (value >= 1f) {
                color = colors[colors.Length - 1];
            }
            else
            if (colors.Length > 1) {
                float v = value * (float)colors.Length;
                int a = Mathf.FloorToInt(v);
                int b = a + 1;
                float t = value - (float)a;

                if (b >= colors.Length) {
                    color = colors[a];
                }
                else
                if (interpolateHue) {
                    color = InterpolateHue(colors[a], colors[b], t);
                }
                else {
                    color = MathUtil.Interpolate(colors[a], colors[b], t);
                }
            }

            return color;
        }

        public static Color Invert(Color color)
        {
            Color i = color;

            i.r = 1f - color.r;
            i.g = 1f - color.g;
            i.b = 1f - color.b;
            i.a = 1f - color.a;

            return i;
        }

        /// <summary>
        /// Returns a color value with ranges between 0 and 1 while preserving the alpha.
        /// </summary>
        public static Color NormalizeVector(Vector4 vec)
        {
            float a = vec.w;
            if (a < 0f) a = 0f;
            else
            if (a > 1f) a = 1f;

            Color c = vec.normalized;
            c.a = a;

            return c;
        }
    }

}//AxonGenesis
