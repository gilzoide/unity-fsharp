using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public class FSharpSettings : SingletonScriptableObject<FSharpSettings>
    {
        [SerializeField] private List<AssetGuid> _scriptCompileOrder = new();

        public override string DefaultAssetPath => $"Assets/Editor/{nameof(FSharpSettings)}.asset";
        public List<AssetGuid> ScriptCompileOrder => _scriptCompileOrder;

        protected override async void OnValidate()
        {
            base.OnValidate();
            if (_scriptCompileOrder.Count == 0)
            {
                RefreshScriptCompileOrder();
            }
            // yield to run on main thread
            await Task.Yield();
            FSharpProjectGenerator.GenerateFsproj();
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
    }
}
