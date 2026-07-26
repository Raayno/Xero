#if UNITY_EDITOR

using UnityEditor;

namespace AxonGenesis
{
    [CustomEditor(typeof(AdvancedPresetsCollection))]
    public class AdvancedPresetsCollectionEditor : Editor
    {
        private AdvancedPresetsCollection _Collection = null;
        private AdvancedPresetsCollectionGUI _GUI = null;
        private bool _hasLoaded = false;

        private AdvancedPresetsWindowContext _context = null;

        private void Setup()
        {
            _Collection = (AdvancedPresetsCollection)target;

            if (!_hasLoaded) {
                _Collection.Load();
                _hasLoaded = true;
            }

            if(_GUI == null && _Collection != null) {
                if(_context == null) _context = new AdvancedPresetsWindowContext(null);
                _GUI = new AdvancedPresetsCollectionGUI(_context, _Collection);
                _context.Load();
            }
            AxonGUI.Setup(70);
        }

        public override void OnInspectorGUI()
        {
            Setup();

            AxonGUI.Setup(70);

            if (_GUI != null) _GUI.MainGUI();
            else {
                AxonGUI.HelpBox("Failed to load the gui object.", MessageType.Warning);
            }
        }
    }
}

#endif
