using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Nuclear.SODatabase.Editor
{
    internal class MenuTree : TreeView
    {
        private bool _inSettings;

        public event Action<Holder> OnSelect;

        public MenuTree(TreeViewState state)  : base(state)
        {
            Reload();
        }
        public class PathHierarchyComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xSegments = x.Split('/');
                var ySegments = y.Split('/');

                int minLength = Math.Min(xSegments.Length, ySegments.Length);

                for (int i = 0; i < minLength; i++)
                {
                    if (xSegments[i] != ySegments[i])
                    {
                        if (IsDirectory(xSegments[i]) && !IsDirectory(ySegments[i]))
                        {
                            return -1; // x is a directory, y is a file
                        }
                        else if (!IsDirectory(xSegments[i]) && IsDirectory(ySegments[i]))
                        {
                            return 1; // x is a file, y is a directory
                        }

                        return string.Compare(xSegments[i], ySegments[i]);
                    }
                }

                return xSegments.Length.CompareTo(ySegments.Length);
            }

            private bool IsDirectory(string xSegment)
            {
                return !xSegment.Contains('.');
            }
        }
        private static List<TreeViewItem> CreateHoldersSet(string assetFolderPath, Type type)
        {
            var strings = AssetDatabase.GetAllAssetPaths()
                .Where(x => x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase)).ToList();
            strings.Sort(new PathHierarchyComparer());

            var menuItemSet = new List<TreeViewItem>();
            int id = 0;
            foreach (var str1 in strings)
            {
                var asset = AssetDatabase.LoadAssetAtPath(str1, type);
                var path = string.Empty;
                string assetName;
                string str2;
                if (asset == null)
                {
                    //it's a directory
                    str2 = str1[assetFolderPath.Length..];
                    var length = str2.LastIndexOf('/');
                    if (length == -1)
                    {
                        path = string.Empty;
                        assetName = str2;
                    }
                    else
                    {
                        path = str2[..length];
                        assetName = str2[(length + 1)..];
                    }

                    if (assetName == string.Empty)
                        continue;
                    var folderDepth = string.IsNullOrEmpty(path) ? 0 : path.Count(p => p == '/') + 1;
                    menuItemSet.Add(new HolderItem(id++, folderDepth, new FolderHolder(path, assetName)));

                    continue;
                }

                var withoutExtension = Path.GetFileNameWithoutExtension(str1);

                str2 = (PathUtilities.GetDirectoryName(str1).TrimEnd('/') + "/").Substring(assetFolderPath.Length);
                if (str2.Length != 0)
                    path = path.Trim('/') + "/" + str2;

                path = path.Trim('/') + "/" + withoutExtension;
                SplitMenuPath(path, out path, out assetName);
                var depth = string.IsNullOrEmpty(path) ? 0 : path.Count(p => p == '/') + 1;
                menuItemSet.Add(new HolderItem(id++, depth, new DataNodeHolder(path, assetName, (DataNode) asset)));
            }

            return menuItemSet;
        }
        
        private static void SplitMenuPath(string menuPath, out string path, out string name)
        {
            menuPath = menuPath.Trim('/');

            var length = menuPath.LastIndexOf('/');
            if (length == -1)
            {
                path = string.Empty;
                name = menuPath;
            }

            else
            {
                path = menuPath[..length];
                name = menuPath[(length + 1)..];
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            var holder = selectedIds.Count > 0 && FindItem(selectedIds[0], rootItem) is HolderItem holderItem
                ? holderItem.Holder
                : null;

            OnSelect?.Invoke(holder);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            if (SODatabaseSettings.Path == string.Empty)
            {
                _inSettings = true;
                root.AddChild(new SettingsItem());
                return root;
            }
            
            var itemsSet = CreateHoldersSet(SODatabaseSettings.Path, typeof(DataNode));
            SetupParentsAndChildrenFromDepths(root, itemsSet);
            
            return root;
        }

       
    }
}