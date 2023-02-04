#nullable enable
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nuclear.SODatabase
{
    public interface IExample1DataNode
    {
        IExample1DataNode2 Q { get; }
    }

    public class Example1DataNode : DataNode, IExample1DataNode
    {
        public const string Path = "Example1Folder/Example1";
        public string Description = string.Empty;
        public DataNode DataNode = null!;

        [SerializeField]
        private IExample1DataNode2 _interface;

        public IExample1DataNode2 Q => _interface;
        
        [JsonProperty, ShowInInspector]
        private IExample1DataNode2 _interface2;

        public override void AfterLoad()
        {
            base.AfterLoad();
            _interface2 = _interface;
        }
    }

    public class Q
    {
        
    }
}
