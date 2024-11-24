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
        public string AssetPath => AssetDatabase.GetAssetPath(this);

        public FSharpScript(string contents)
        {
            _contents = contents;
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
    }
}
