#nullable enable
using UnityEngine;

namespace Nuclear.SODatabase
{
    public class SOD_Example1 : MonoBehaviour
    {
        private void Start()
        {
            SODatabaseStatic.Init(null, () =>
            {
                var model = SODatabaseStatic.GetModel<Example1DataNode>(Example1DataNode.Path);
                Debug.Log(model.Description);

                var models = SODatabaseStatic.GetModels<Example1DataNode2>(Example1DataNode2.Path);
                foreach (var model1 in models)
                    Debug.Log(model1.Description2);

                SODatabaseStatic.Load();
                
            });
            
            
        }

        private void OnApplicationQuit()
        {
            SODatabaseStatic.Save();
        }
    }
}

