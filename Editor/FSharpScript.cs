using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpScript : ScriptableObject
    {
        [SerializeField] private string _contents;

        public string Contents
        {
            get => _contents;
            set => _contents = value;
        }
        public string AssetPath => Path.GetRelativePath(".", AssetDatabase.GetAssetPath(this));

        public FSharpScript(string contents)
        {
            _contents = contents;
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Scripting/Empty F# Script")]
        private static void CreateEmptyFSharpScript()
        {
            string baseDir = "Assets";
            foreach (Object obj in Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets))
            {
                baseDir = AssetDatabase.GetAssetPath(obj);
            }

            string path = EditorUtility.SaveFilePanelInProject("New F# script", "NewFSharpEmptyScript", "fs", "", baseDir);
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, "");
                AssetDatabase.ImportAsset(path);
            }
        }

        [CustomPropertyDrawer(typeof(FSharpScript))]
        private class FSharpScriptPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(position, property, label);
                }
            }
        }

        [CustomEditor(typeof(FSharpScript))]
        private class FSharpScriptEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(serializedObject.FindProperty(nameof(_contents)).stringValue);
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
