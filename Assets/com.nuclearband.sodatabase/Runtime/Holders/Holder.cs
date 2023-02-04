#nullable enable
using Newtonsoft.Json;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Nuclear.SODatabase
{
    [JsonObject(MemberSerialization.OptIn)]
    internal abstract class Holder
    {
        protected Holder()
        {
        }

#if UNITY_EDITOR
        [HideInInspector] internal string Path = string.Empty;
        [HideInInspector] internal string Name = string.Empty;

        [ShowInInspector] [HorizontalGroup("Path")] protected string TempPath = string.Empty;
        [ShowInInspector] [HorizontalGroup("Name")] protected string TempName = string.Empty;

        protected Holder(string path, string name)
        {
            Path = path;
            Name = name;
        }

        internal virtual void Select()
        {
            TempPath = Path;
            TempName = Name;
        }

        [HorizontalGroup("Path")] [ShowIf("@Path != TempPath")] [Button] protected abstract void Move();
        [HorizontalGroup("Name")] [ShowIf("@Name != TempName")] [Button] protected abstract void Rename();
        [Button] [GUIColor(0.7f, 0.7f, 1f)] protected abstract void Clone();
        [Button] [GUIColor(1f, 0.7f, 0.7f)] protected abstract void Remove();
#endif
    }
}
