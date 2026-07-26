// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    public struct GUIRect
    {
        public static implicit operator Rect(GUIRect v) { return new Rect(v.x, v.y, v.width, v.height); }
        public static implicit operator GUIRect(Rect v) { return new GUIRect(v.x, v.y, v.width, v.height); }
        public static implicit operator string(GUIRect v) { return $"{v.x}, {v.y}, {v.width}, {v.height}"; }

        public int x;
        public int y;
        public int width;
        public int height;

        public GUIRect(int _x, int _y, int _width, int _height)
        {
            x = _x;
            y = _y;
            width = _width;
            height = _height;
        }

        public GUIRect(float _x, float _y, float _width, float _height)
        {
            x = (int)_x;
            y = (int)_y;
            width = (int)_width;
            height = (int)_height;
        }

        public GUIRect(Rect rect)
        {
            x = (int)rect.x;
            y = (int)rect.y;
            width = (int)rect.width;
            height = (int)rect.height;
        }

        public GUIRect(GUIRect rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;
        }

        public int Left { get => x; set => x = value; }

        public int Top { get => y; set => y = value; }

        public int Width { get => width; set => width = value; }

        public int Height { get => height; set => height = value; }

        public int xMin {
            get {
                return x;
            }
            set {
                int num = xMax;
                x = value;
                width = num - x;
            }
        }

        public int xMax {
            get {
                return width + x;
            }
            set {
                width = value - x;
            }
        }

        public int yMin {
            get {
                return y;
            }
            set {
                int num = yMax;
                y = value;
                height = num - y;
            }
        }

        public int yMax {
            get {
                return height + y;
            }
            set {
                height = value - y;
            }
        }

        public Vector2 position {
            get {
                return new Vector2(x, y);
            }
            set {
                x = (int)value.x;
                y = (int)value.y;
            }
        }

        public Vector2 size {
            get {
                return new Vector2(width, height);
            }
            set {
                width = (int)value.x;
                height = (int)value.y;
            }
        }

        public Vector2 center {
            get {
                return new Vector2(x + ((float)width / 2f), y + ((float)height / 2f));
            }
            set {
                x = (int)(value.x - ((float)width / 2f));
                height = (int)(value.y - ((float)height / 2f));
            }
        }

        public void Clear()
        {
            x = y = width = height = 0;
        }

        public bool Contains(Vector2 point)
        {
            return point.x >= x && point.x < x + width && point.y >= y && point.y < y + height;
        }

        public bool Overlaps(Rect other)
        {
            return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
        }

        public override string ToString()
        {
            return $"x:{x} y:{y} w:{width} h:{height}";
        }
    }

}//AxonGenesis
