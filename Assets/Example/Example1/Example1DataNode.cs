#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TriInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nuclear.SODatabase
{
    public interface IExample1DataNode
    {
        IExample1DataNode2 Q { get; }
    }

    [DrawWithTriInspector]
    public class Example1DataNode : DataNode, IExample1DataNode
    {
        public const string Path = "Example1Folder/Example1";
        public string Description = string.Empty;
        public DataNode DataNode = null!;

        [SerializeField,SerializeReference]
        private IExample1DataNode2 _interface;

        [SerializeField, SerializeReference]
        private IQ _iq;
        
        [SerializeField]
        private Q1 _q1;

        [SerializeField]
        private List<Q> _iqs;
        

        public IExample1DataNode2 Q => _interface;
        
        [JsonProperty]
        private IExample1DataNode2 _interface2;
        
        [JsonProperty, Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly] private HashSet<string> _npcList = new();

        public override void AfterLoad(ISODatabase soDatabase)
        {
            base.AfterLoad(soDatabase);
            _interface2 = _interface;

            _npcList ??= new();
            _npcList.Add("q");
        }
    }


    public interface IQ
    {
    }
    

    [Serializable]
    public class Q : IQ
    {
        public string Y;
    }

    [Serializable]
    public class Q1 : IQ
    {
        public int X;
    }
}
