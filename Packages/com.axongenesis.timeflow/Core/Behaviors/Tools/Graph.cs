// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is an editor utility for capturing and displaying data in the Timeflow graph view. It can be
    /// used to help visualize animation data and is useful for debugging complex motions. Graph data can
    /// be routed into this component using Channel Link, or by scripting using the RecordValue method.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Graph")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/graph")]
    sealed public class Graph : TimeflowDataBehavior
    {
        public static List<Graph> AllGraphs;

        public static Graph FindGraph(string name)
        {
            Graph graph = null;
            if (AllGraphs != null) {
                foreach (Graph g in AllGraphs) {
                    if (g.Channel.Name == name) {
                        graph = g;
                        break;
                    }
                }
            }
            if (graph == null) {
                Debug.LogWarning("Could not find a Graph named '" + name + "'");
            }
            return graph;
        }

        public bool IsRecording = true;
        public bool WorkAreaOnly = true;
        public bool ResumeRecording = true;
        public bool ClearOnRecord = true;
        public bool SavePlayModeData = false;

        [SerializeField]
        public SDictionaryFloatFloat Data;

        [NonSerialized]
        public float DataMinTime;

        [NonSerialized]
        public float DataMaxTime;

        [NonSerialized]
        public float DataMinValue;

        [NonSerialized]
        public float DataMaxValue;

        public override Texture2D Icon => AxonUI.Icons.Graph;

        public bool IsLocked {
            get {
                return Channel != null && Channel.IsLocked;
            }
            set {
                if (Channel != null) {
                    Channel.IsLocked = value;
                }
            }
        }

        private string TempDataPath => Application.persistentDataPath + "/Graph_" + Channel.Name + ".json";

        protected override void OnAwake()
        {
            base.OnAwake();
            if (AllGraphs == null) AllGraphs = new List<Graph>();
            if (!AllGraphs.Contains(this)) {
                AllGraphs.Add(this);
            }
            if (!Application.isPlaying && SavePlayModeData) {
                LoadPlayModeData(false);
            }
        }

        protected override void OnDestruct()
        {
            base.OnDestruct();
            if (Application.isPlaying && SavePlayModeData) {
                ExportData(TempDataPath);
            }
            if (AllGraphs != null && AllGraphs.Contains(this)) {
                AllGraphs.Remove(this);
            }
        }

        public override void OnPlay()
        {
            base.OnPlay();
            if (IsRecording && ClearOnRecord) {
                ClearData(false);
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            /// Automatically stop recording on stop
            IsRecording = false;
        }

        public override void OnRewind()
        {
            base.OnRewind();
            PrepareData();
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.IsCombinedValue = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Float;

            Channel.DataParent = this;
            Channel.IsDataOnly = true;
            Channel.IsCombinedValue = true;
            Channel.IsLoopSupported = false;
            Channel.PropertyType = Property.PropertyTypes.Float;

            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Graph";
            }
            PrepareData();
        }

        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            float value = 0f;
            if (IsRecording) {
                if (channel.IsLinkEnabled) {
                    value = channel.Link.GetValue(value, channel.WorldTime(time, true));
                }
                RecordValue(value);
                //if (DebugEnabled) Debug.Log(Channel.Name + ".IsRecording:" + value);
            }
            else
            if (Data != null && Data.Count > 0) {
                if (time <= DataMinTime) {
                    value = Data.ValuesList[0];
                }
                else
                if (time <= DataMinTime) {
                    value = Data.ValuesList[Data.ValuesList.Count - 1];
                }
                else {
                    channel.ToProperty.FloatValue = 0f;
                    bool first = true;
                    KeyValuePair<float, float> last = new KeyValuePair<float, float>(0, 0);
                    foreach (KeyValuePair<float, float> k in Data) {
                        if (first) {
                            first = false;
                        }
                        else {
                            if (time >= last.Key && time < k.Key) {
                                float d = k.Key - last.Key;
                                if (d <= 0) {
                                    value = last.Value;
                                }
                                else {
                                    value = MathUtil.Interpolate(last.Value, k.Value, (time - last.Key) / d);
                                }
                            }
                        }
                        last = k;
                    }
                }
                if (apply) Channel.ToProperty.FloatValue = value;
                //if (DebugEnabled) Debug.Log(Channel.Name + ".InterpolateValue:" + value + " time:" + time);
            }
            return value;
        }

        public void PrepareData()
        {
            DataMinTime = DataMaxTime = 0;
            DataMinValue = DataMaxValue = 0;
            if (Data != null && Data.Count > 0) {
                Data.Sort();

                bool first = true;
                foreach (KeyValuePair<float, float> k in Data) {
                    if (first) {
                        first = false;
                        DataMinTime = DataMaxTime = k.Key;
                        DataMinValue = DataMaxValue = k.Value;
                    }
                    else {
                        if (DataMinTime > k.Key) DataMinTime = k.Key;
                        if (DataMaxTime < k.Key) DataMaxTime = k.Key;
                        if (DataMinValue > k.Value) DataMinValue = k.Value;
                        if (DataMaxValue < k.Value) DataMaxValue = k.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Enables and begins recording for this Graph behavior. If recording data by custom script, you
        /// may also directly set IsRecording to true to arm recording.
        /// </summary>
        public void StartRecording()
        {
            IsRecording = true;
            if (Timeflow != null) {
                if (!ResumeRecording) {
                    float start = 0f;
                    if (WorkAreaOnly && Timeflow.Active.WorkAreaEnabled) {
                        start = Timeflow.Active.WorkAreaStart;
                    }
                    Timeflow.Active.SetTime(start);
                }
                Timeflow.Active.Play();
            }
        }

        public void StopRecording()
        {
            Timeflow.Active.Stop();
        }

        /// <summary>
        /// Use this method to send your own data from another script. Typically one would use ClearData
        /// first and then RecordValue during playback to capture the data to graph in the Timeflow graph
        /// view. Please note that this function only records data when IsRecording is true.
        /// </summary>
        public void RecordValue(float value)
        {
            RecordValue(value, Channel.CurrentTime);
        }

        /// <summary>
        /// Same as RecordValue(value) but at a specific time. Use this method if recording data other than
        /// the current time.
        /// </summary>
        public void RecordValue(float value, float time)
        {
            if (Enabled && !IsLocked && Timeflow.Active.IsPlaying) {
                //if (DebugEnabled) Debug.Log("RecordValue:" + value + " time:" + time);
                if (Data == null) Data = new SDictionaryFloatFloat();

                bool canRecord = true;
                if (WorkAreaOnly && Timeflow.Active.WorkAreaEnabled) {
                    float t = time + Channel.TimeOffsetWorld;
                    canRecord = t >= Timeflow.Active.WorkAreaStart && t < Timeflow.Active.WorkAreaEnd;
                }

                if (canRecord) Data.Add(time, value);
                else StopRecording();
            }
        }

        /// <summary>
        /// Clears the graph data for a clean slate. Call this before recording a new set of data,
        /// otherwise data will be overlayed ontop of existing data.
        /// </summary>
        /// <param name="canUndo">If true, the data can be restored by undoing.</param>
        public void ClearData(bool canUndo)
        {
            if (IsLocked) return;
            //if (DebugEnabled) Debug.Log($"{Name}.Graph.ClearData");
            if (canUndo) UndoUtil.Undo(this, "Clear Data", true);
            if (WorkAreaOnly && Timeflow.Active.WorkAreaEnabled && Data != null && Data.Count > 0) {
                /// Only clear data insde of the work area, preserving everything else
                Data.Sort();

                SDictionaryFloatFloat newData = new SDictionaryFloatFloat();

                foreach (KeyValuePair<float, float> k in Data) {
                    if (k.Key < Timeflow.Active.WorkAreaStart || k.Key > Timeflow.Active.WorkAreaEnd) {
                        newData.Add(k.Key, k.Value);
                    }
                }
                Data = newData;
            }
            else {
                Data = null;
            }
            PrepareData();
        }

        public void LoadPlayModeData(bool deleteFile)
        {
            string path = TempDataPath;
            if (File.Exists(path)) {
                ImportData(path);
                if (deleteFile) File.Delete(path);
            }
            else {
                Debug.LogWarning($"File does not exist:{path}");
            }
        }

        public void ExportData()
        {
            string path = EditorUtility.SaveFilePanel("Save Graph Data", "", Channel.Name, "json");
            ExportData(path);
        }

        public void ExportData(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string json = Data.ToJson(true);
            File.WriteAllText(path, json);
            Debug.Log($"Exported:{path}\n{json}");//--KEEP
        }

        public void ImportData()
        {
            string path = EditorUtility.OpenFilePanel("Import Graph Data", "", "json");
            ImportData(path);
        }

        public void ImportData(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string json = File.ReadAllText(path);
            Debug.Log($"Imported:{path}\n{json}");//--KEEP
            Data.FromJson(json);
        }

        /// <summary>
        /// This adds the behavior to the context menu item to add a new instance.
        /// </summary>
        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Graph"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects == null) {
                return;
            }

            foreach (TimeflowObject obj in objects) {
                obj.BehaviorsEnabled = true;

                Undo.AddComponent<Graph>(obj.gameObject);
            }
            Timeflow.Active.Refresh(true);
        }

        public override void GUIGraphFit(bool init, bool selectedOnly)
        {
            //if (Timeflow == null) return;
            PrepareData();
            base.GUIGraphFit(init, selectedOnly);

            float min = DataMinValue;
            float max = DataMaxValue;
            if (init) {
                Timeflow.Active.View.GraphMinValue = Mathf.Min(min, max);
                Timeflow.Active.View.GraphMaxValue = Mathf.Max(min, max);
            }
            else {
                Timeflow.Active.View.GraphMinValue = Mathf.Min(Timeflow.Active.View.GraphMinValue, Mathf.Min(min, max));
                Timeflow.Active.View.GraphMaxValue = Mathf.Max(Timeflow.Active.View.GraphMaxValue, Mathf.Max(min, max));
            }
        }

        public override void GUIGraph(Rect rect)
        {
            Channel.IsHiddenInGraph = true; // Don't draw channel since this method overrides it
            if (Data == null || Data.Count <= 0) {
                return;
            }

            Timeflow timeflow = Timeflow;
            Vector3[] line = new Vector3[Data.Count];

            GUI.color = Channel.GUIColor;
            int i = 0;
            foreach (KeyValuePair<float, float> k in Data) {
                line[i].x = Timeflow.Active.View.PositionOfTime(k.Key + Channel.TimeOffsetWorld, true);
                line[i].y = Timeflow.Active.View.PositionOfValue(k.Value, true);

                Rect r = new Rect(line[i].x - 5f, line[i].y - 5f, 10, 10);
                GUI.Box(r, GUIContent.none, AxonUI.BezierUnifiedHandleStyle);
                i++;
            }

            Handles.color = Channel.GUIColor;
            Handles.DrawAAPolyLine(TimeflowView.GraphCurveThickness, line);

            GUI.color = Handles.color = Color.white;
        }

    }

}//AxonGenesis

#endif