using UnityEditor.IMGUI.Controls;

namespace Nuclear.SODatabase.Editor
{
    internal class HolderItem : TreeViewItem
    {
        public Holder Holder { get; }
        public HolderItem(int id, int depth, Holder holder) : base (id, depth, holder.Name)
        {
            Holder = holder;
        }
    }
}