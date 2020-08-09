#nullable enable
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;

namespace NuclearBand
{
    public class DataNodeHolder : Holder
    {
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)] 
        public DataNode DataNode;

        public DataNodeHolder(string path, string name, DataNode dataNode) : base(path, name)
        {
            DataNode = dataNode;
        }


        protected override void Move()
        {
            AssetDatabase.MoveAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset",
                SODatabaseSettings.Path + tempPath + "/" + Name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Rename()
        {
            AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset", tempName + ".asset");
            Name = tempName;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected override void Clone()
        {
            AssetDatabase.CopyAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset",
                AssetDatabase.GenerateUniqueAssetPath(SODatabaseSettings.Path + Path + "/" + Name + ".asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    
        protected override void Remove()
        {
            AssetDatabase.DeleteAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset");
            AssetDatabase.Refresh();
        }
    }
}
#endif
