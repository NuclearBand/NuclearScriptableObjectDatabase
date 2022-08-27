#nullable enable
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace NuclearBand
{
    public class FolderHolder : Holder
    {
        [HideInInspector]
        public Dictionary<string, DataNode> DataNodes = new();
        
        [HideInInspector]
        public Dictionary<string, FolderHolder> FolderHolders = new();

        public FolderHolder() : base()
        {
            
        }
        
#if UNITY_EDITOR
        public FolderHolder(string path, string name) : base(path, name)
        {
        }

        protected override void Move()
        {
            AssetDatabase.Refresh();
            AssetDatabase.MoveAsset(SODatabaseSettings.Path + Path + "/" + Name, SODatabaseSettings.Path + tempPath + "/" + Name);
            AssetDatabase.SaveAssets();
        }

        protected override void Rename()
        {
            AssetDatabase.Refresh();
            AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name, tempName);
            Name = tempName;
            AssetDatabase.SaveAssets();
        }

        protected override void Clone()
        {
            AssetDatabase.Refresh();
            AssetDatabase.CopyAsset(SODatabaseSettings.Path + Path + "/" + Name,
                AssetDatabase.GenerateUniqueAssetPath(SODatabaseSettings.Path + Path + "/" + Name));
            AssetDatabase.SaveAssets();
        }

        protected override void Remove()
        {
            if (!EditorUtility.DisplayDialog("Remove folder",
                    "Are you sure you want to remove folder with DataNodes?", "Yes, remove", "No"))
                return;
            SODatabaseInternal.RemoveFolder(Path + "/" + Name);
        }
#endif
    }
}
