using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public class FSharpSettings : SingletonScriptableObject<FSharpSettings>
    {
        [SerializeField] private List<FSharpScript> _scriptCompileOrder = new();
        [SerializeField] private List<PackageReference> _packageReferences = new();

        public override string DefaultAssetPath => $"Assets/Editor/{nameof(FSharpSettings)}.asset";
        public IEnumerable<string> ScriptPaths => _scriptCompileOrder.Select(assetGuid => assetGuid.AssetPath);
        public IEnumerable<string> PlayerScriptPaths => ScriptPaths.Where(s => !s.Contains("/Editor/"));
        public IEnumerable<string> EditorScriptPaths => ScriptPaths.Where(s => s.Contains("/Editor/"));
        public List<PackageReference> PackageReferences => _packageReferences;

        protected override async void OnValidate()
        {
            base.OnValidate();
            await Task.Yield();
            if (this)
            {
                RefreshScriptCompileOrder();
            }
        }

        [InitializeOnLoadMethod]
        private static void CreateFSharpSettingsIfNecessary()
        {
            _ = Instance;
        }

        [ContextMenu("Refresh Script Compile Order")]
        public void RefreshScriptCompileOrder()
        {
            IEnumerable<FSharpScript> fsScripts = AssetDatabase.FindAssets($"t:{nameof(FSharpScript)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<FSharpScript>);
            HashSet<FSharpScript> fsScriptSet = new(fsScripts);
            // Remove deleted assets
            _scriptCompileOrder.RemoveAll(assetGuid => !fsScriptSet.Remove(assetGuid));
            // Add all remaining assets
            _scriptCompileOrder.AddRange(fsScriptSet);
            EditorUtility.SetDirty(this);
        }

        [MenuItem("Tools/F#/Select F# Settings Asset")]
        public static void SelectFSharpSettingsAsset()
        {
            EditorGUIUtility.PingObject(Instance);
        }

        [CustomEditor(typeof(FSharpSettings))]
        private class FSharpSettingsEditor : UnityEditor.Editor
        {
            private bool _rebuildOnDisable = false;

            protected void OnEnable()
            {
                _rebuildOnDisable = false;
            }

            protected void OnDisable()
            {
                if (_rebuildOnDisable)
                {
                    FSharpBuilder.BuildAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
                }
            }

            public override void OnInspectorGUI()
            {
                if (DrawDefaultInspector())
                {
                    _rebuildOnDisable = true;
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Build"))
                {
                    FSharpBuilder.BuildAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
                    _rebuildOnDisable = false;
                }
            }
        }
    }
}
