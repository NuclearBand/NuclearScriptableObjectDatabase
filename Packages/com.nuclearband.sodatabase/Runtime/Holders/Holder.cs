using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuclearBand
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Holder
    {
        protected Holder()
        {
            
        }
#if UNITY_EDITOR
        [HideInInspector]
        public string Path = string.Empty;
        [HideInInspector]
        public string Name = string.Empty;

        [ShowInInspector]
        [HorizontalGroup("Path")]
        protected string tempPath = string.Empty;
        [ShowInInspector]
        [HorizontalGroup("Name")]
        protected string tempName = string.Empty;

        public Holder(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public virtual void Select()
        {
            tempPath = Path;
            tempName = Name;
        }

        [HorizontalGroup("Path")]
        [ShowIf("@Path != tempPath")]
        [Button]
        protected abstract void Move();
    
        [HorizontalGroup("Name")]
        [ShowIf("@Name != tempName")]
        [Button]
        protected abstract void Rename();
        
        [Button]
        [GUIColor(0.7f, 0.7f, 1f)]
        protected abstract void Clone();

        [Button]
        [GUIColor(1f, 0.7f, 0.7f)]
        protected abstract void Remove();
#endif
    }
}
