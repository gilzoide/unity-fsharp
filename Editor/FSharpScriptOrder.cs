using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpScriptOrder : ScriptableObject
    {
        public const string ProjectSettingsPath = "ProjectSettings/FSharpScriptOrder.txt";
        public static readonly int GuidLength = GUID.Generate().ToString().Length;

        [SerializeField] private List<AssetGuid> _scriptCompileOrder;

        public static IEnumerable<GUID> LoadScriptGuids()
        {
            if (!File.Exists(ProjectSettingsPath))
            {
                return Enumerable.Empty<GUID>();
            }
            return File.ReadLines(ProjectSettingsPath)
                .Select(s => s[..GuidLength])
                .Select(s => (isValid: GUID.TryParse(s, out GUID guid), guid))
                .Where(tuple => tuple.isValid)
                .Select(tuple => tuple.guid);
        }
        public static IEnumerable<string> LoadScriptPaths()
        {
            if (!File.Exists(ProjectSettingsPath))
            {
                return Enumerable.Empty<string>();
            }
            return File.ReadLines(ProjectSettingsPath).Select(s => s[(GuidLength + 1)..]);
        }
        public static void SaveScriptGuids(IEnumerable<GUID> guids) => File.WriteAllLines(ProjectSettingsPath, guids.Select(guid => $"{guid}\t{AssetDatabase.GUIDToAssetPath(guid)}"));
        public static void SaveScriptPaths(IEnumerable<string> paths) => SaveScriptGuids(paths.Select(AssetDatabase.GUIDFromAssetPath));

        [InitializeOnLoadMethod]
        private static void SyncMissingScriptIfNecessary()
        {
            if (!File.Exists(ProjectSettingsPath))
            {
                SyncMissingScripts();
            }
        }

        [MenuItem("Tools/F#/Update Script Compilation Order")]
        public static void SyncMissingScripts()
        {
            var orderedGuids = new List<GUID>();
            var fsScriptGuids = new HashSet<string>(AssetDatabase.FindAssets("glob:\"*.fs\""));

            // Maintain existing scripts without messing with the existing order
            foreach (GUID guid in LoadScriptGuids())
            {
                if (fsScriptGuids.Remove(guid.ToString()))
                {
                    orderedGuids.Add(guid);
                }
            }
            // Now add all missing scripts without messing with the existing order
            orderedGuids.AddRange(fsScriptGuids.Select(s => new GUID(s)));

            SaveScriptGuids(orderedGuids);
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            FSharpScriptOrder fsScriptOrder = null;
            bool somethingChanged = false;
            return new SettingsProvider("Project/F#/ScriptOrder", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Script Compile Order",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var serializedObject = new SerializedObject(fsScriptOrder);
                    SerializedProperty prop = serializedObject.FindProperty(nameof(_scriptCompileOrder));
                    prop.isExpanded = true;
                    EditorGUILayout.PropertyField(prop);
                    if (serializedObject.ApplyModifiedProperties())
                    {
                        somethingChanged = true;
                        SaveScriptGuids(fsScriptOrder._scriptCompileOrder.Select(assetGuid => new GUID(assetGuid.Guid)));
                        FSharpProjectGenerator.GenerateFsproj();
                    }
                },
                activateHandler = (searchContext, rootElement) =>
                {
                    SyncMissingScripts();
                    fsScriptOrder = CreateInstance<FSharpScriptOrder>();
                    fsScriptOrder._scriptCompileOrder = new List<AssetGuid>(LoadScriptGuids().Select(guid => new AssetGuid(guid)));
                    somethingChanged = false;
                },
                deactivateHandler = () =>
                {
                    if (somethingChanged)
                    {
                        FSharpBuilder.BuildAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
                    }
                    DestroyImmediate(fsScriptOrder);
                },
            };
        }

        [Serializable]
        private struct AssetGuid
        {
            public AssetGuid(GUID guid)
            {
                Guid = guid.ToString();
            }

            public string Guid;
        }

        [CustomPropertyDrawer(typeof(AssetGuid))]
        private class AssetGuidPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    string guid = property.FindPropertyRelative(nameof(AssetGuid.Guid)).stringValue;
                    EditorGUI.TextField(position, GUIContent.none, AssetDatabase.GUIDToAssetPath(guid));
                }
            }
        }
    }
}
