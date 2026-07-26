using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    public static class UndoUtil
    {
        public static void Undo(UnityEngine.Object obj, string name, bool completeObject = false)
        {
#if UNITY_EDITOR
            if (TimeflowViewInput.IsUndoing || obj == null) return;
            UnityEditor.Undo.RecordObject(obj, name);
            if (completeObject) UnityEditor.Undo.RegisterCompleteObjectUndo(obj, name);
#endif
        }

        public static void UndoCreate(UnityEngine.Object obj, string name)
        {
#if UNITY_EDITOR
            if (TimeflowViewInput.IsUndoing) return;
            UnityEditor.Undo.RegisterCreatedObjectUndo(obj, name);
#endif
        }

        public static void UndoDestroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (TimeflowViewInput.IsUndoing) return;
            UnityEditor.Undo.DestroyObjectImmediate(obj);
#else
			UnityEngine.Object.DestroyImmediate(obj);
#endif
        }
    }
}
