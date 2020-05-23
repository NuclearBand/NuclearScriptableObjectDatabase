#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NuclearBand
{
    public static class SODatabaseInternal
    {
        public static T GetModelForEdit<T>(string path) where T : DataNode
        {
            return AssetDatabase.LoadAssetAtPath(SODatabaseSettings.Path + path + ".asset", typeof(T)) as T;
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
            var fullPath = SODatabaseSettings.Path + path + "/" + name;
            
            var model = GetModelForEdit<T>(fullPath);
            if (model != null)
                return model;
            
            CreateFolder(path);
            var obj = ScriptableObject.CreateInstance(typeof(T)) as T;
            AssetDatabase.CreateAsset(obj, fullPath + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return obj;
        }
    }
}
#endif