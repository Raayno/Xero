// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        #region STATIC VARS

        public static GameObject QuickSelect(int index)
        {
            GameObject obj = null;
            if (index < 0 || index > 11) return null;
            if (Timeflow.Active != null) {
                obj = Timeflow.Active.QuickSelectObjects[index];
                SelectionUtil.Select(obj);

                if (Timeflow.Active.EditorViewQuickSelect) {
                    /// Automatically show the selected object in the Timflow view
                    Timeflow.Active.View.Display.DisplaySelectedHierarchy();

                }
            }
            return obj;
        }

        #endregion

        #region PUBLIC

        [TimeflowIgnore]
        public GameObject[] QuickSelectObjects = new GameObject[12];

        [TimeflowIgnore]
        public bool EditorShowQuickSelect;

        [TimeflowIgnore]
        public bool EditorViewQuickSelect;

        #endregion
    }

}//AxonGenesis

#endif