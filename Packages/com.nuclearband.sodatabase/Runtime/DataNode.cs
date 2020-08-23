#nullable enable
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuclearBand
{
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public class DataNode : SerializedScriptableObject
    {
        [SerializeField, HideInInspector]
        private string fullPath = string.Empty;

        public string FullPath => $"{(string.IsNullOrEmpty(fullPath) ? string.Empty : $"{fullPath}/")}{name}";

#if UNITY_EDITOR
        public void SetFullPath(string fullPath) => this.fullPath = fullPath;
#endif
    }
}