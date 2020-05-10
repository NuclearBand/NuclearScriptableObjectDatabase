#if UNITY_EDITOR
using NuclearBand.Editor;
using UnityEditor;

namespace NuclearBand
{
    public class FolderHolder : Holder
    {
        public FolderHolder(string path, string name) : base(path, name)
        {
        }

        protected override void Move()
        {
            AssetDatabase.MoveAsset(ScriptableObjectDatabaseEditorWindow.Path + Path + "/" + Name, ScriptableObjectDatabaseEditorWindow.Path + tempPath + "/" + Name);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Rename()
        {
            AssetDatabase.RenameAsset(ScriptableObjectDatabaseEditorWindow.Path + Path + "/" + Name, tempName);
            Name = tempName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Clone()
        {
            AssetDatabase.CopyAsset(ScriptableObjectDatabaseEditorWindow.Path + Path + "/" + Name,
                AssetDatabase.GenerateUniqueAssetPath(ScriptableObjectDatabaseEditorWindow.Path + Path + "/" + Name));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Remove()
        {
            AssetDatabase.DeleteAsset(ScriptableObjectDatabaseEditorWindow.Path + Path + "/" + Name);
            AssetDatabase.Refresh();
        }
    }
}
#endif