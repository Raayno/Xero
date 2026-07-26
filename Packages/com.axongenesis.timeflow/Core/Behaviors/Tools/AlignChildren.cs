// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is a simple tool for sorting and aligning child objects in a series with the ability to
    /// incrementally offset position, rotation, and scale. This is a fast way to arrange objects in a row
    /// or incremental step. Apply this to any object with children. Note also that this behavior
    /// implements time behavior but does not have any channels.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Timeflow/Align Children")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/tools/align-children")]
    sealed public class AlignChildren : TimeflowBehavior, ITimeflowBehaviorMenu
    {
        #region PUBLIC

        public bool AutoLayout = true;
        public int RandomSeed;

        // POSITION
        public bool PositionEnabled;
        public bool PositionRelative = true;
        public Vector3 Position = Vector3.zero;
        public Vector3 PositionEach = Vector3.zero;
        public bool PositionReverse;
        public bool PositionCenter;
        public bool PositionAbs;
        public Vector3 PositionRandomize = Vector3.zero;

        public bool PositionLockX;
        public bool PositionLockY;
        public bool PositionLockZ;

        // ROTATION
        public bool RotationEnabled;
        public bool RotationRelative = true;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 RotationEach = Vector3.zero;
        public bool RotationReverse;
        public bool RotationCenter;
        public bool RotationAbs;
        public Vector3 RotationRandomize = Vector3.zero;

        public bool RotationLockX;
        public bool RotationLockY;
        public bool RotationLockZ;

        // SCALE
        public bool ScaleEnabled;
        public bool ScaleRelative = true;
        public bool ScaleUniform;
        public Vector3 Scale = Vector3.one;
        public Vector3 ScaleEach = Vector3.zero;
        public bool ScaleReverse;
        public bool ScaleCenter;
        public bool ScaleAbs;
        public Vector3 ScaleRandomize = Vector3.zero;

        public bool ScaleLockX;
        public bool ScaleLockY;
        public bool ScaleLockZ;

        public List<AlignChild> Children;

        #endregion

        public int Count {
            get {
                return transform.childCount;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            GatherChildren(false);
        }

        public void GatherChildren(bool force)
        {
            if (force || Children == null || Children.Count == 0) {
#if UNITY_EDITOR
                UndoUtil.Undo(this, "Rebuild Matrices", true);
#endif
                Random.InitState(RandomSeed);
                if (gameObject.transform.childCount == 0) {
                    Debug.LogWarning("Please assign child objects to this game object to use Align Children");
                }
                else {
                    Children = new List<AlignChild>();
                    int i = 0;
                    foreach (Transform child in transform) {
                        AlignChild c = new AlignChild(this, child, i);
                        c.Transform = child;
                        Children.Add(c);
                        i++;
                    }
                }
            }
        }

        public void Randomize()
        {
            RandomSeed = (int)(Random.value * 99999);
            if (Children == null || Children.Count == 0) {
                GatherChildren(true);
            }
            else {
                Random.InitState(RandomSeed);
                foreach (AlignChild child in Children) {
                    child.Randomize();
                }
            }
        }

        /// <summary>
        /// Sorts the children by their bounding box size, either ascending or descending. This is used to
        /// arrange child objects from large to small, or vice versa.
        /// </summary>
        /// <param name="ascending">If true, children are arranged from smallest to largest</param>
        public void SortSize(bool ascending)
        {
            if (ascending) {
                Children.Sort(new SortChildrenAsc());
            }
            else {
                Children.Sort(new SortChildrenDesc());
            }
            int i = 0;
            foreach (AlignChild child in Children) {
                //if (DebugEnabled) Debug.Log(child.Transform.name + ":" + child.Bounds.size.x);
                child.Index = i;
                child.Transform.SetSiblingIndex(i);
                i++;
            }
        }

        public override void UpdateTime()
        {
            //if (DebugEnabled) Debug.Log("AlignChildren[" + name + "].UpdateTime: " + CurrentFrame + " pos:" + PositionEach);
            base.UpdateTime();
            if (AutoLayout) UpdateLayout();
        }

        public void UpdateLayout()
        {
            if (Children == null || Children.Count == 0) return;

            int x = 0;
            foreach (AlignChild child in Children) {
                if (PositionEnabled && (!PositionLockX || !PositionLockY || !PositionLockZ)) {
                    child.UpdatePosition();
                }

                if (RotationEnabled && (!RotationLockX || !RotationLockY || !RotationLockZ)) {
                    child.UpdateRotation();
                }

                if (ScaleEnabled && (!ScaleLockX || !ScaleLockY || !ScaleLockZ)) {
                    child.UpdateScale();
                }

                x++;
            }
        }

        #region EDITOR
#if UNITY_EDITOR

        public bool EditorShowNaming;
        public string Basename;
        public int BaseamePad = 2;
        public bool AutoLayoutOnChange = true;

        public override Texture2D Icon => AxonUI.Icons.AlignChildren;

        public void RenameChildren()
        {
            if (string.IsNullOrEmpty(Basename)) {
                Basename = gameObject.name;
            }
            int i = 1;
            foreach (Transform child in transform) {
                UndoUtil.Undo(child, "Rename Children");
                child.gameObject.name = Basename + StringUtil.PadNumber(i, BaseamePad);
                i++;
            }
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Tool/Align Children"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Undo.AddComponent<AlignChildren>(obj.gameObject);
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif
        #endregion
    }

    public class SortChildrenAsc : IComparer<AlignChild>
    {
        public int Compare(AlignChild a, AlignChild b)
        {
            int c = 0;

            float lengthA = a.Bounds.size.x * a.Bounds.size.z;
            float lengthB = b.Bounds.size.x * b.Bounds.size.z;
            if (lengthA < lengthB) {
                c = -1;
            }
            else
            if (lengthA > lengthB) {
                c = 1;
            }

            return c;
        }
    }

    public class SortChildrenDesc : IComparer<AlignChild>
    {
        public int Compare(AlignChild a, AlignChild b)
        {
            int c = 0;

            float lengthA = a.Bounds.size.x * a.Bounds.size.z;
            float lengthB = b.Bounds.size.x * b.Bounds.size.z;
            if (lengthA > lengthB) {
                c = -1;
            }
            else
            if (lengthA < lengthB) {
                c = 1;
            }

            return c;
        }
    }

}//AxonGenesis
