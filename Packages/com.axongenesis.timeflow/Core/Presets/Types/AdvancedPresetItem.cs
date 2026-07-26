// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System;

namespace AxonGenesis
{
    /// <summary>
    /// Helper class representing a hierarchy entry (either a GameObject or Component)
    /// along with a selectable state and its children. This is not serialized and is 
    /// reconstructed when the hierarchy is loaded.
    /// </summary>
    public class AdvancedPresetItem
    {
        public UnityEngine.Object SourceObject { get; set; } // GameObject or Component

        public string DisplayName { get; set; }

        public Texture Icon { get; set; }

        public int Index { get; set; }

        public int SilbingIndex { get; set; }

        public int Depth { get; set; }

        public bool WasProcessed { get; set; }


        private bool _IsSelected = true;

        public bool IsSelected {
            get {
                return _IsSelected;
            }
            set {
                if (_IsSelected == value) return;
                //Debug.Log($"<color=lime>[{Index}] {DisplayName}.IsSelected:{value}</color>");
                _IsSelected = value;
                if (Children != null) {
                    foreach (var child in Children) {
                        child.IsSelected = value;
                    }
                }
            }
        }

        public void RestoreSelection(bool selected)
        {
            //Debug.Log($"<color=yellow>[{Index}] {DisplayName}.RestoreSelection:{selected}</color>");
            _IsSelected = selected;
        }

        public List<AdvancedPresetItem> Children;
    }
}
#endif
