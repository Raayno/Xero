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
    /// Used in the editor for displaying GUI elements.
    /// </summary>
    public class GUIObject
    {
        public static implicit operator GUIRect(GUIObject v) { return v.Rect; }
        public static implicit operator Rect(GUIObject v) { return v.Rect; }
        public static implicit operator string(GUIObject v) { return ""+v.Rect; }

        public GUIRect Rect = new GUIRect(0, 0, 0, 0);
        public bool IsSelected = false;
        public string Name = "GUIObject";
        public GUIObject Container = null;

        #region CONSTRUCTORS

        public GUIObject() { }

        public GUIObject(string name)
        {
            Name = name;
            Rect = new GUIRect(0, 0, 0, 0);
        }

        public GUIObject(string name, int x, int y, int w, int h)
        {
            Name = name;
            Rect = new GUIRect(x, y, w, h);
        }

        public GUIObject(string name, GUIRect rect)
        {
            Name = name;
            Rect = rect;
        }

        #endregion

        #region ACCESSORS

        public Vector2 Position { get => new Vector2(Rect.x, Rect.y); set { Rect.x = (int)value.x; Rect.y = (int)value.y; } }

        public int Left { get => Rect.x; set => Rect.x = value; }

        public int Right { get => Rect.x + Rect.width; set => Rect.width = value - Rect.x; }

        public int Top { get => Rect.y; set => Rect.y = value; }      

        public int Bottom { get => Rect.y + Rect.height; set => Rect.height = value - Rect.y; }

        public int Width { get => Rect.width; set => Rect.width = value; }

        public int Height { get => Rect.height; set => Rect.height = value; }

        #endregion

        public GUIRect WorldRect {
            get {
                if (Container == null) {
                    return Rect;
                }
                GUIRect container = Container.WorldRect;
                return new GUIRect(Left + container.Left, Top + container.Top, Width, Height);
            }
        }

        #region HIT TESTS

        public bool HitTest(Vector2 pos)
        {
            return WorldRect.Contains(pos);
        }

        public bool HitTest(Vector2 pos, int pad)
        {
            int pad2 = pad * 2;
            GUIRect world = WorldRect;
            GUIRect r = new GUIRect(world.x - pad, world.y - pad, world.width + pad2, world.height + pad2);
            return r.Contains(pos);
        }

        public bool HitTest(Vector2 pos, int padX, int padY)
        {
            GUIRect world = WorldRect;
            GUIRect r = new GUIRect(world.x - padX, world.y - padY, world.width + padX + padX, world.height + padY + padY);
            return r.Contains(pos);
        }

        public bool HitTestDebug(Vector2 pos)
        {
            return WorldRect.Contains(pos);
        }

        #endregion
    }

}//AxonGenesis