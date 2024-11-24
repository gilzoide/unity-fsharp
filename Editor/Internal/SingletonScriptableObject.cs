using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.FSharp.Editor.Internal
{
    public class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>
    {
        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                if (AssetDatabase.FindAssets($"t:{nameof(T)}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<T>)
                    .FirstOrDefault() is T loadedAsset
                )
                {
                    return _instance = loadedAsset;
                }
                _instance = CreateInstance<T>();
                Directory.CreateDirectory(Path.GetDirectoryName(_instance.DefaultAssetPath));
                AssetDatabase.CreateAsset(_instance, _instance.DefaultAssetPath);
                return _instance;
            }
        }
        protected static T _instance;

        public virtual string DefaultAssetPath => $"Assets/{nameof(T)}.asset";
        public static bool InstanceAssetExists => !AssetDatabase.FindAssets($"t:{nameof(T)}").IsNullOrEmpty();

        protected virtual void OnValidate()
        {
            if (_instance == null)
            {
                _instance = (T) this;
            }
        }

        protected virtual void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
