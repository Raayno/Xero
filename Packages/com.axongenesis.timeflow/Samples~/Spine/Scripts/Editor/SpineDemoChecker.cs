using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class SpineDemoChecker : MonoBehaviour
    {
#if UNITY_EDITOR
        public SpineConfig Config = null;
        public GameObject Instructions = null;

        private void Awake()
        {
#if USING_SPINE
            // Only show the setup instructions if not installed already
            Instructions.SetActive(!SpineConfig.IsSpineInstalled);
#else
#endif
        }
#endif
    }
}
