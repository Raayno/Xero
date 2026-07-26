// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.


using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    /// <summary>
    /// Container for Motion Path nodes. This simply makes it easier to identify.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/motion-path#motion-path-nodes")]
    sealed public class MotionPathNodes : AxonGenesisBehavior
    {
        [SerializeField]
        [FormerlySerializedAs("MotionPath")]
        private MotionPath _MotionPath;

        [SerializeField]
        private bool IsInitialized = false;

        public MotionPath MotionPath {
            get {
                return _MotionPath;
            }
            set {
                if (_MotionPath != value) {
                    IsInitialized = true;
                    _MotionPath = value;
                }
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if(_MotionPath != null) IsInitialized = true;
        }

        private void Update()
        {
            if (!IsInitialized) return;
            if (MotionPath == null || MotionPath.NodeContainer != this) {
                // Automatically remove nodes when the motion path has been removed
                Remove();
            }
        }

        private void Remove()
        {
            ObjectUtil.Destroy(gameObject);
        }
    }

}//AxonGenesis