using Sirenix.OdinInspector;
using UnityEngine;

namespace NuclearBand
{
    public abstract class Holder
    {
#if UNITY_EDITOR
        [HideInInspector]
        public string Path;
        [HideInInspector]
        public string Name;

        [ShowInInspector]
        protected string tempPath;
        [ShowInInspector]
        protected string tempName;

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

        [Button]
        protected abstract void Move();
    
        [Button]
        protected abstract void Rename();
        
        [Button]
        protected abstract void Clone();
        [Button]
        protected abstract void Remove();
#endif
    }
}
