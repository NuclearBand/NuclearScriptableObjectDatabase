#nullable enable
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;

namespace Nuclear.SODatabase
{
    internal class DataNodeHolder : Holder
    {
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)] internal DataNode DataNode;

        internal DataNodeHolder(string path, string name, DataNode dataNode) : base(path, name) => DataNode = dataNode;

        protected override void Move()
        {
            AssetDatabase.Refresh();
            AssetDatabase.MoveAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset",
                SODatabaseSettings.Path + TempPath + "/" + Name + ".asset");
            AssetDatabase.SaveAssets();
        }

        protected override void Rename()
        {
            AssetDatabase.Refresh();
            AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset", TempName + ".asset");
            Name = TempName;
            AssetDatabase.SaveAssets();
        }

        protected override void Clone()
        {
            AssetDatabase.Refresh();
            AssetDatabase.CopyAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset",
                AssetDatabase.GenerateUniqueAssetPath(SODatabaseSettings.Path + Path + "/" + Name + ".asset"));
            AssetDatabase.SaveAssets();
        }

        protected override void Remove()
        {
            if (!EditorUtility.DisplayDialog("Remove node",
                    "Are you sure you want to remove DataNode?", "Yes, remove", "No"))
                return;

            AssetDatabase.Refresh();
            AssetDatabase.DeleteAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif