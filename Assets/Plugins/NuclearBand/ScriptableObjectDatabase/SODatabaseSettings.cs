using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuclearBand
{
    public class SODatabaseSettings : SerializedScriptableObject
    {
        private static SODatabaseSettings instance;
        public static SODatabaseSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<SODatabaseSettings>("SODatabaseSettings");

                return instance;
            }
        }

        public static string Path => Instance.path;
        public static string Label => Instance.label;
        
        [SerializeField]
        [ReadOnly]
        [Title("Don't forget to set this folder as Addressable with label")]
        private string path = "";


        [SerializeField]
        [ReadOnly]
        private string label = "SODatabase";


        [FolderPath(AbsolutePath = false)]
        [NonSerialized, ShowInInspector]
        public string SavePath;
        
        [Button]
        void Save()
        {
            path = SavePath + "/";
        }
    }
}