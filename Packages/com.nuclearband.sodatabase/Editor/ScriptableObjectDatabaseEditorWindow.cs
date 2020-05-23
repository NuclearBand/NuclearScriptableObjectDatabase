#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NuclearBand.Editor
{
    public class ScriptableObjectDatabaseEditorWindow : OdinMenuEditorWindow
    {
        private bool inSettings = false;

        [MenuItem("Tools/NuclearBand/ScriptableObjectDatabase")]
        private static void Open()
        {
            var window = GetWindow<ScriptableObjectDatabaseEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
            window.OnClose += () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true)
            {
                DefaultMenuStyle = {IconSize = 28.00f},
                Config = {DrawSearchToolbar = true}
            };

            if (SODatabaseSettings.Path == "")
            {
                inSettings = true;
                tree.AddMenuItemAtPath(new HashSet<OdinMenuItem>(), "", new OdinMenuItem(tree, "Settings", SODatabaseSettings.Instance));
                return tree;
            }

            AddAllAssetsAtPath(tree, SODatabaseSettings.Path, typeof(DataNode));
            Texture folderIcon = (Texture2D) AssetDatabase.LoadAssetAtPath("Packages/com.nuclearband.sodatabase/Editor/folderIcon.png", typeof(Texture2D));
            tree.EnumerateTree().AddIcons<FolderHolder>(x => folderIcon);
            tree.SortMenuItemsByName();
            tree.Selection.SelectionChanged += SelectionChanged;
            return tree;
        }

        private void SelectionChanged(SelectionChangedType obj)
        {
            switch (obj)
            {
                case SelectionChangedType.ItemAdded:
                    (MenuTree.Selection.SelectedValue as Holder).Select();
                    break;
                case SelectionChangedType.ItemRemoved:
                    break;
                case SelectionChangedType.SelectionCleared:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }
        
        void AddAllAssetsAtPath(
            OdinMenuTree tree,
            string assetFolderPath,
            Type type)
        {
            var strings = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase));

            var odinMenuItemSet = new HashSet<OdinMenuItem>();
            foreach (var str1 in strings)
            {
                var @object = AssetDatabase.LoadAssetAtPath(str1, type);
                var path = "";
                var name = "";
                var str2 = "";
                if (@object == null)
                {
                    //it's a directory
                    str2 = str1.Substring(assetFolderPath.Length);
                    int length = str2.LastIndexOf('/');
                    if (length == -1)
                    {
                        path = "";
                        name = str2;
                    }
                    else
                    {
                        path = str2.Substring(0, length);
                        name = str2.Substring(length + 1);
                    }

                    if (name == "")
                        continue;
                    tree.AddMenuItemAtPath(odinMenuItemSet, path, new OdinMenuItem(tree, name, new FolderHolder(path, name)));

                    continue;
                }

                var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(str1);

                str2 = (PathUtilities.GetDirectoryName(str1).TrimEnd('/') + "/").Substring(assetFolderPath.Length);
                if (str2.Length != 0)
                    path = path.Trim('/') + "/" + str2;

                path = path.Trim('/') + "/" + withoutExtension;
                SplitMenuPath(path, out path, out name);
                var menuItem = new OdinMenuItem(tree, name, new DataNodeHolder(path, name, (DataNode) @object));
                tree.AddMenuItemAtPath(odinMenuItemSet, path, menuItem);
                AddDragHandles(menuItem);
            }
        }

        private static void SplitMenuPath(string menuPath, out string path, out string name)
        {
            menuPath = menuPath.Trim('/');

            int length = menuPath.LastIndexOf('/');
            if (length == -1)
            {
                path = "";
                name = menuPath;
            }

            else
            {
                path = menuPath.Substring(0, length);
                name = menuPath.Substring(length + 1);
            }
        }

        protected override void OnBeginDrawEditors()
        {
            if (inSettings)
            {
                base.OnBeginDrawEditors();
                if (SODatabaseSettings.Path != "")
                {
                    Close();
                    Open();
                }

                return;
            }

            var selected = MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    var type = selected.Value is DataNodeHolder ? (selected.Value as DataNodeHolder).DataNode.GetType().ToString() : "Directory";
                    GUILayout.Label(type);
                }

                var path = SODatabaseSettings.Path;
                if (MenuTree.Selection.SelectedValue != null)
                    path += string.IsNullOrEmpty((MenuTree.Selection.SelectedValue as Holder).Path) ? "" : (MenuTree.Selection.SelectedValue as Holder).Path + "/";
                if (MenuTree.Selection.SelectedValue is FolderHolder folderHolder)
                    path += folderHolder.Name + "/";
                path = path.Substring(0, path.Length - 1);
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create DataNode")))
                {
                    DataNodeCreator.ShowDialog<DataNode>(path, obj =>
                    {
                        TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Folder")))
                {
                    var uniqName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + "New");
                    uniqName = uniqName.Substring(uniqName.LastIndexOf('/') + 1);

                    AssetDatabase.CreateFolder(path, uniqName);
                    AssetDatabase.Refresh();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void AddDragHandles(OdinMenuItem menuItem)
        {
            menuItem.OnDrawItem += x => DragAndDropUtilities.DragZone(menuItem.Rect, (menuItem.Value as DataNodeHolder).DataNode, false, false);
        }
    }
}
#endif