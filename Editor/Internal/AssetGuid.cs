using System;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    [Serializable]
    public class AssetGuid
    {
        public string Guid;
        public string AssetPath;

        public AssetGuid(GUID guid)
        {
            Guid = guid.ToString();
            AssetPath = AssetDatabase.GUIDToAssetPath(guid);
        }

        public void RefreshAssetPath()
        {
            AssetPath = AssetDatabase.GUIDToAssetPath(Guid);
        }

        [CustomPropertyDrawer(typeof(AssetGuid))]
        private class AssetGuidPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(AssetPath)), GUIContent.none);
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(AssetPath)));
            }
        }
    }

}
