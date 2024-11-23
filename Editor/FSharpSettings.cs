using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public class FSharpSettings : SingletonScriptableObject<FSharpSettings>
    {
        [SerializeField] private List<AssetGuid> _scriptCompileOrder = new();
        [SerializeField] private List<PackageReference> _packageReferences = new();

        public override string DefaultAssetPath => $"Assets/Editor/{nameof(FSharpSettings)}.asset";
        public IEnumerable<string> ScriptGuids => _scriptCompileOrder.Select(assetGuid => assetGuid.Guid);
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

        [DidReloadScripts]
        private static void CreateFSharpSettingsIfNecessary()
        {
            _ = Instance;
        }

        [ContextMenu("Refresh Script Compile Order")]
        public void RefreshScriptCompileOrder()
        {
            HashSet<string> fsScriptGuids = new(AssetDatabase.FindAssets("glob:\"*.fs\""));
            // Remove deleted assets
            _scriptCompileOrder.RemoveAll(assetGuid => !fsScriptGuids.Remove(assetGuid.Guid));
            // Refresh asset paths, in case files were moved
            _scriptCompileOrder.ForEach(assetGuid => assetGuid.RefreshAssetPath());
            // Add all remaining assets
            _scriptCompileOrder.AddRange(fsScriptGuids.Select(guid => new AssetGuid(new GUID(guid))));
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
