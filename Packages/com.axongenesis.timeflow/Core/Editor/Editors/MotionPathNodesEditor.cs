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
    [CustomEditor(typeof(MotionPathNodes))]
    [CanEditMultipleObjects]
    public class MotionPathNodesEditor : AxonGenesisEditor<MotionPathNodes, MotionPathNodesEdit> { }
    
    sealed public class MotionPathNodesEdit : AxonGenesisBehaviorEdit<MotionPathNodes>
    {
        public MotionPathNodesEdit() { }

        public MotionPathNodesEdit(MotionPathNodes _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/motion-path#motion-path-nodes";
        }

        public override void GUISetup()
        {
            base.GUISetup();
        }

        public override void GUIMenu()
        {
        }

        public override void OnMultiEditGUI()
        {
            OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.SetTooltip("This is a read-only reference to the Motion Path object that owns this node container.");
            AxonGUI.FieldObject(target, "Motion Path", target.MotionPath, typeof(MotionPath), true);

            AxonGUI.HelpBox("This game object and its children are managed by Motion Path. Please do not delete or move this object or its children. " +
                "It is also recommended to avoid adding any other objects as children to this hierarchy. Instead use Follow or Channel Link to relate motions " +
                "to motion path node objects. Please refer to the documentation for more information.", MessageType.Info, true);
        }
    }

}//AxonGenesis

#endif