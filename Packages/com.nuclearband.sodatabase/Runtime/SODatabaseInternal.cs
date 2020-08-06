#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NuclearBand
{
    public static class SODatabaseInternal
    {
        public static T GetModelForEdit<T>(string path) where T : DataNode
        {
            return AssetDatabase.LoadAssetAtPath<T>(SODatabaseSettings.Path + path + ".asset") ??
                throw new ArgumentException($"Could not get model at path {path}");
        }

        public static List<T> GetModelsForEdit<T>(string path) where T : DataNode
        {
            var modelGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).Name}",new [] {SODatabaseSettings.Path + path});

            return modelGUIDs.Select(model => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(model))).ToList();
        }

        public static void CreateFolder(string path)
        {
            var folders = path.Split('/');
            var fullPath = SODatabaseSettings.Path.TrimEnd('/');
            foreach (var folder in folders)
            {
                var prevFullPath = fullPath;
                fullPath += "/" + folder;

                if (AssetDatabase.IsValidFolder(fullPath))
                    continue;
                
                AssetDatabase.CreateFolder(prevFullPath, folder);
                
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static T CreateModel<T>(string path, string name) where T : DataNode
        {
            try
            {
                var model = GetModelForEdit<T>(path + "/" + name);
                return model;
            }
            catch{}

            CreateFolder(path);
            var obj = (ScriptableObject.CreateInstance(typeof(T)) as T)!;
            var fullPath = SODatabaseSettings.Path + path + "/" + name;
            AssetDatabase.CreateAsset(obj, fullPath + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return obj;
        }

        public static void RemoveFolder(string path)
        {
            AssetDatabase.DeleteAsset(SODatabaseSettings.Path + "/" + path);
            AssetDatabase.Refresh();
        }
    }
}
#endif