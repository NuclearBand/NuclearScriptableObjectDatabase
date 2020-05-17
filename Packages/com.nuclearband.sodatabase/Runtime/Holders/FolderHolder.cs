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
        public Dictionary<string, DataNode> DataNodes = new Dictionary<string, DataNode>();
        
        [HideInInspector]
        public Dictionary<string, FolderHolder> FolderHolders = new Dictionary<string, FolderHolder>();

        public FolderHolder() : base()
        {
            
        }
        
#if UNITY_EDITOR
        public FolderHolder(string path, string name) : base(path, name)
        {
        }

        protected override void Move()
        {
            AssetDatabase.MoveAsset(SODatabaseSettings.Path + Path + "/" + Name, SODatabaseSettings.Path + tempPath + "/" + Name);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Rename()
        {
            AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name, tempName);
            Name = tempName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Clone()
        {
            AssetDatabase.CopyAsset(SODatabaseSettings.Path + Path + "/" + Name,
                AssetDatabase.GenerateUniqueAssetPath(SODatabaseSettings.Path + Path + "/" + Name));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Remove()
        {
            AssetDatabase.DeleteAsset(SODatabaseSettings.Path + Path + "/" + Name);
            AssetDatabase.Refresh();
        }
#endif

    }
}
