#nullable enable
using UnityEngine;

namespace Nuclear.SODatabase
{
    public interface IExample1DataNode2
    {
        bool X { get; }
    }

    public class Example1DataNode2 : DataNode, IExample1DataNode2
    {
        public const string Path = "Example1Folder";
        [ResetOnPlay]
        public string Description2 = string.Empty;

        [SerializeField]
        private bool _x;

        bool IExample1DataNode2.X => _x;
    }
}
