// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if TMPRO_3_OR_NEWER
using TMPro;
#endif

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        public enum TitleModes
        {
            ClearAndRegenerate,
            CreateOrUpdateExisting,
            CreateNewOnly
        }
        [TimeflowIgnore]
        public TitleModes TitleMode = TitleModes.CreateOrUpdateExisting;

        [TimeflowIgnore]
        public GameObject TitlePrefab;

        [TimeflowIgnore]
        public GameObject TitleContainer;

        [TimeflowIgnore]
        public bool TitleRainbow;

        [TimeflowIgnore]
        public bool IsTitleUI = true;

        public void GenerateTitles()
        {
            if (MarkerList == null || MarkerList.Count == 0) {
                Debug.LogError("Please add Markers first to Timeflow to generate titles.");
                return;
            }
            UndoUtil.Undo(this, "Generate Titles", true);
            if (TitleContainer == null) {
                TitleContainer = ObjectUtil.GetChild(gameObject, "Titles", false);
                if (TitleContainer == null) {
                    TitleContainer = ObjectUtil.GetChild(gameObject, "Titles", true);
                    TitleContainer.transform.SetParent(transform);
                    if (IsTitleUI) {
                        ObjectUtil.ResetTransform(TitleContainer);
                        Canvas canvas = TitleContainer.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    }
                    else {
                        ObjectUtil.ResetTransform(TitleContainer);
                    }
                }
                UndoUtil.UndoCreate(TitleContainer, "Generate Titles");
            }
            if (TitleMode == TitleModes.ClearAndRegenerate) {
                ObjectUtil.DestroyChildrenImmediate(TitleContainer);
            }

            Object[] selected = new Object[MarkerList.Count];
            for (int x = 0; x < MarkerList.Count; x++) {
                TimeflowMarker m = MarkerList[x];
                TimeflowMarker next = null;
                if (x < MarkerList.Count - 1) {
                    next = MarkerList[x + 1];
                }

                bool hasPrefab = false;
                bool canChange = true;
                bool titleExists = false;
                GameObject obj = ObjectUtil.GetChild(TitleContainer, m.Name, false);
                if (obj != null) {
                    titleExists = true;
                }
                else {
                    if (TitlePrefab == null) {
                        obj = new GameObject();
                    }
                    else {
                        obj = GameObject.Instantiate(TitlePrefab);
                        hasPrefab = true;
                    }
                    UndoUtil.UndoCreate(obj, "Generate Titles");
                    obj.transform.SetParent(TitleContainer.transform);
                    ObjectUtil.ResetTransform(obj);

                    if (hasPrefab) {
                        obj.transform.localPosition = TitlePrefab.transform.localPosition;
                        obj.transform.localRotation = TitlePrefab.transform.localRotation;
                        obj.transform.localScale = TitlePrefab.transform.localScale;
                    }
                }

                if (TitleMode == TitleModes.CreateNewOnly) {
                    canChange = !titleExists;
                }
                if (canChange) {
                    obj.name = m.Name;// StringUtil.PadNumber2(x+1)+ m.Name;

#if TMPRO_3_OR_NEWER
                    if (IsTitleUI) {
                        if (!obj.TryGetComponent<TextMeshProUGUI>(out var text)) {
                            text = obj.AddComponent<TextMeshProUGUI>();
                        }
                        text.text = m.Name;

                        if (!hasPrefab) {
                            text.alignment = TextAlignmentOptions.Center;
                            text.fontSize = 36;

                            if (obj.TryGetComponent<RectTransform>(out RectTransform rt)) {
                                rt.sizeDelta = new Vector2(600, 100);
                            }
                        }
                    }
                    else {
                        if (!obj.TryGetComponent<TextMeshPro>(out var text)) text = obj.AddComponent<TextMeshPro>();
                        text.text = m.Name;

                        if (!hasPrefab) {
                            text.alignment = TextAlignmentOptions.Center;
                            text.fontSize = 36;
                        }
                    }
#endif
                    obj.SetActive(true);

                    TimeflowObject tobj = Timeflow.SetupTimeflowObject(obj);
                    tobj.Track.AutoFullLength = false;
                    tobj.Track.IsLocked = false;
                    tobj.Track.VisibilityMode = TimeflowTrack.VisibilityModes.Activate;
                    tobj.Track.Keys[0].LockTime = false;
                    tobj.Track.Keys[0].LockValue = false;
                    tobj.Track.Keys[0].KeyTime = m.GlobalTime;
                    tobj.Track.Keys[0].KeyValue = (next != null ? next.GlobalTime : EndTime);
                    tobj.Track.Keys[0].KeyString = m.Name;
                    tobj.Track.Keys[0].LockTime = true;
                    tobj.Track.Keys[0].LockValue = true;
                    tobj.Track.IsLocked = true;

                    Color c = m.LabelColor;
                    if (TitleRainbow) {
                        float hue = (float)x / (float)MarkerList.Count;
                        c = ColorUtil.HLSColor(hue, 1f, 1f);
                    }
                    tobj.Track.GUIColor = c;
                    tobj.Track.Keys[0].KeyColor = c;

                    selected[x] = obj;
                }
            }

            SelectionUtil.Select(selected);
            Timeflow.View.Display.DisplaySelectedObjects(true);
        }
    }

}//AxonGenesis

#endif
