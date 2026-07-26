// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    sealed public class TimeflowDisplayMenuItem
    {
        #region STATIC MENU FUNCTIONS

        public static void Rename()
        {
            Timeflow.Active.View.Display.StartEditingName();
        }

        public static void Save(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.Save(item.Display.Name);
            }
        }

        public static void Remove(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.ObjectMode = TimeflowViewDisplay.ObjectModes.Nothing;
                item.Display.Remove(item.SavedIndex);
            }
        }

        public static void Load(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.Load(item.SavedIndex);
            }
        }

        public static void Clear(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                if (EditorUtility.DisplayDialog("Clear All Lists?", "Are you sure you want to remove all of the saved display lists?", "YES", "NO")) {
                    item.Display.Clear();
                }
            }
        }

        public static void Edit(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.Edit();
            }
        }

        public static void ShowAllAnimatedInWorkArea()
        {
            if (Timeflow.Active == null || Timeflow.Active.Display == null) return;
            Timeflow.Active.Display.ShowAllAnimatedInWorkArea();
        }

        public static void DisplayNothing(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.DisplayNothing();
            }
        }

        public static void DisplayEverything(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.DisplayEverything();
            }
        }

        public static void DisplaySelectedObject(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.DisplaySelectedObject();
            }
        }

        public static void DisplaySelectedGroup(object obj)
        {
            TimeflowDisplayMenuItem item = (TimeflowDisplayMenuItem)obj;
            if (item != null) {
                item.Display.DisplaySelectedGroup();
            }
        }

        #endregion

        public int SavedIndex;
        public readonly TimeflowViewDisplay Display;

        public TimeflowDisplayMenuItem(TimeflowViewDisplay display, int index)
        {
            Display = display;
            SavedIndex = index;
        }

    }

}//AxonGenesis

#endif
