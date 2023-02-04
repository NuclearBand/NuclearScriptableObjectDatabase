#nullable enable
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace Nuclear.SODatabase
{
    internal class FolderHolder : Holder
    {
        internal Dictionary<string, DataNode> DataNodes = new();
        internal Dictionary<string, FolderHolder> FolderHolders = new();

        internal FolderHolder() { }

#if UNITY_EDITOR
        internal FolderHolder(string path, string name) : base(path, name) { }

        protected override void Move()
        {
            AssetDatabase.Refresh();
            AssetDatabase.MoveAsset(SODatabaseSettings.Path + Path + "/" + Name, SODatabaseSettings.Path + TempPath + "/" + Name);
            AssetDatabase.SaveAssets();
        }

        protected override void Rename()
        {
            AssetDatabase.Refresh();
            AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name, TempName);
            Name = TempName;
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
            SODatabaseUtilities.RemoveFolder(Path + "/" + Name);
        }
#endif
    }
}
