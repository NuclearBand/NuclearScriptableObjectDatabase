#nullable enable
using UnityEngine;

namespace NuclearBand
{
    public class SOD_Example1 : MonoBehaviour
    {
        void Start()
        {
            SODatabase.Init(null, () =>
            {
                var model = SODatabase.GetModel<Example1DataNode>(Example1DataNode.Path);
                Debug.Log(model.Description);

                var models = SODatabase.GetModels<Example1DataNode2>(Example1DataNode2.Path);
                foreach (var model1 in models)
                    Debug.Log(model1.Description2);
            });
        }
    }
}
