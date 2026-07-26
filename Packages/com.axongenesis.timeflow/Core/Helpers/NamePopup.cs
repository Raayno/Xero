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
    public class NameData
    {
        public string Name = "Name";
        public object Data;
    }

    public class NamePopup : EditorWindow
    {
        public delegate bool SaveFunction(NameData userData);

        private string _title = "Title";
        private SaveFunction _onSave;
        private NameData _data;
        private bool _isPlaced;

        public static void Show(string title, string name, SaveFunction callback, object userData)
        {
            NamePopup window = ScriptableObject.CreateInstance(typeof(NamePopup)) as NamePopup;
            window._title = title;
            window._onSave = callback;
            window._data = new NameData();
            window._data.Name = name;
            window._data.Data = userData;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            if (_data == null) Cancel();
            if (!_isPlaced) {
                _isPlaced = true;
                position = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 300, 80);
            }
            AxonGUI.BeginBoxPadded();
            AxonGUI.Heading(_title);
            _data.Name = AxonGUI.FieldTextInline(null, _data.Name, GUILayout.ExpandWidth(true));
            AxonGUI.BeginHorizontal();
            if (AxonGUI.ButtonInline("Cancel")) {
                Cancel();
            }
            if (AxonGUI.Button("Save")) {
                Save();
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBoxPadded();
        }

        private void Cancel()
        {
            Close();
            EditorGUIUtility.ExitGUI();
        }

        private void Save()
        {
            if (_onSave != null) {
                if (_onSave(_data)) {
                    Cancel();
                }
            }
            else {
                Debug.LogWarning("No callback method provided for the NameWindow");
                Cancel();
            }
        }
    }

}//AxonGenesis
#endif