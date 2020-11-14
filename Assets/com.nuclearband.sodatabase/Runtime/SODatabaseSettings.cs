#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NuclearBand
{
    public class SODatabaseSettings : SerializedScriptableObject
    {
        private static SODatabaseSettings? instance;

        public static SODatabaseSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<SODatabaseSettings>("SODatabaseSettings");
#if UNITY_EDITOR
                if (instance == null)
                {
                    AssetDatabase.Refresh();
                    instance = CreateInstance<SODatabaseSettings>();
                    AssetDatabase.CreateFolder("Assets", "SODatabase");
                    AssetDatabase.CreateFolder("Assets/SODatabase", "Resources");
                    const string destination = "Assets/SODatabase/Resources/";
                    AssetDatabase.CreateAsset(instance, (destination + "SODatabaseSettings.asset"));
                    AssetDatabase.SaveAssets();
                }
#endif
                return instance;
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

        [Button]
        void Save()
        {
            path = SavePath + "/";
        }
    }
}