#if UNITY_EDITOR
using NuclearBand.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace NuclearBand
{
    public class DataNodeHolder : Holder
    {
        [CustomValueDrawer("ShowDataNode")]
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
            Debug.Log(AssetDatabase.RenameAsset(SODatabaseSettings.Path + Path + "/" + Name + ".asset", tempName + ".asset"));
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


        static PropertyTree myObjectTree;
        private static DataNode prevDataNode;
        static DataNode ShowDataNode(DataNode dataNode, GUIContent label)
        {
            if (myObjectTree == null || prevDataNode != dataNode)
            {
                myObjectTree = PropertyTree.Create(dataNode);
                prevDataNode = dataNode;
            }

            myObjectTree.Draw(false);
            return dataNode;
        }
    }
}
#endif