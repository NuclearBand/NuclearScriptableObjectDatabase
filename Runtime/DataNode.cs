#nullable enable
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nuclear.SODatabase
{
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public class DataNode : SerializedScriptableObject
    {
        [SerializeField, HideInInspector] private string _fullPath = string.Empty;

        public string FullPath => $"{(string.IsNullOrEmpty(_fullPath) ? string.Empty : $"{_fullPath}/")}{name}";

        public virtual void BeforeSave() { }

        public virtual void AfterLoad(ISODatabase soDatabase) { }

#if UNITY_EDITOR
        public void SetFullPath(string fullPath) => _fullPath = fullPath;
#endif
    }
}