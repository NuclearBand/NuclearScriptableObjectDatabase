#nullable enable
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NuclearBand
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
                    AssetDatabase.CreateFolder("Assets", "SODatabase");
                    AssetDatabase.CreateFolder("Assets/SODatabase", "Resources");
                    const string destination = "Assets/SODatabase/Resources/";
                    AssetDatabase.CreateAsset(_instance, (destination + "SODatabaseSettings.asset"));
                    AssetDatabase.SaveAssets();
                }
#endif
                return _instance;
            }
        }

        public static string Path => Instance.path;
        public static string Label => Instance.label;

        [SerializeField]
        [ReadOnly]
        [Title("Don't forget to set this folder as Addressable with label")]
        private string path = string.Empty;


        [SerializeField]
        [ReadOnly]
        private string label = "SODatabase";


        [FolderPath(AbsolutePath = false)]
        [NonSerialized, ShowInInspector]
        public string SavePath = string.Empty;

        // ReSharper disable once CollectionNeverUpdated.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public Dictionary<Type, Texture> NodeIcons = new();

        [Button]
        private void Save()
        {
            path = SavePath + "/";
        }
    }
}