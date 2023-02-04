#nullable enable
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nuclear.SODatabase
{
    public class SODatabaseSettings : SerializedScriptableObject
    {
        private static SODatabaseSettings? _instance;
        public static SODatabaseSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<SODatabaseSettings>("SODatabaseSettings");
#if UNITY_EDITOR
                if (_instance == null)
                {
                    AssetDatabase.Refresh();
                    _instance = CreateInstance<SODatabaseSettings>();
                    if (!AssetDatabase.IsValidFolder("Assets/SODatabase"))
                        AssetDatabase.CreateFolder("Assets", "SODatabase");
                    if (!AssetDatabase.IsValidFolder("Assets/SODatabase/Resources"))
                        AssetDatabase.CreateFolder("Assets/SODatabase", "Resources");
                    AssetDatabase.CreateAsset(_instance, ("Assets/SODatabase/Resources/SODatabaseSettings.asset"));
                    AssetDatabase.SaveAssets();
                }
#endif
                return _instance;
            }
        }

        public static string Path => Instance._path;
        public static string Label => Instance._label;

        [SerializeField] [ReadOnly] [Title("Don't forget to set this folder as Addressable with label")]
        private string _path = string.Empty;
        
        [SerializeField] [ReadOnly] private string _label = "SODatabase";
        
        [FolderPath(AbsolutePath = false)] [NonSerialized, ShowInInspector] public string SavePath = string.Empty;

        // ReSharper disable once CollectionNeverUpdated.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public Dictionary<Type, Texture> NodeIcons = new();

        [Button] private void Save() => _path = SavePath + "/";
    }
}