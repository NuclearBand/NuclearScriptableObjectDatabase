#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Nuclear.SODatabase.Editor
{
    public class ScriptableObjectDatabaseEditorWindow : OdinMenuEditorWindow
    {
        public static event Action? OnSave;
        
        private bool _inSettings;

        [MenuItem("Tools/NuclearBand/ScriptableObjectDatabase")]
        private static void Open()
        {
            var window = GetWindow<ScriptableObjectDatabaseEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        [MenuItem("Tools/NuclearBand/ScriptableObjectDatabase-ClearSave")]
        private static void ClearSave()
        {
            AssetDatabase.Refresh();
            
            File.Delete(SODatabaseSaver.SavePath);
            var models = SODatabaseUtilities.GetModelsForEdit<DataNode>("");
            foreach (var model in models)
            {
                var typeInfo = model.GetType().GetTypeInfo();
                var fields = typeInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var attributes = field.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
                    if (attributes.Length > 0)
                        field.SetValue(model, default);
                }

                var properties =
                    typeInfo.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
                    if (attributes.Length > 0)
                        property.SetValue(model, default);
                }

                EditorUtility.SetDirty(model);
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/NuclearBand/ScriptableObjectDatabase-OpenSaveFolder")]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true)
            {
                DefaultMenuStyle = {IconSize = 28.00f},
                Config = {DrawSearchToolbar = true}
            };

            if (SODatabaseSettings.Path == string.Empty)
            {
                _inSettings = true;
                tree.AddMenuItemAtPath(new HashSet<OdinMenuItem>(), string.Empty, new OdinMenuItem(tree, "Settings", SODatabaseSettings.Instance));
                return tree;
            }

            AddAllAssetsAtPath(tree, SODatabaseSettings.Path, typeof(DataNode));
            Texture folderIcon = (Texture2D) AssetDatabase.LoadAssetAtPath("Packages/com.nuclearband.sodatabase/Editor/folderIcon.png", typeof(Texture2D));
            tree.EnumerateTree().AddIcons(odinMenuItem =>
            {
                if (odinMenuItem.Value is FolderHolder)
                {
                    return folderIcon;
                }

                var dataNodeType = ((DataNodeHolder) odinMenuItem.Value).DataNode.GetType();
                if (SODatabaseSettings.Instance.NodeIcons.ContainsKey(dataNodeType))
                {
                    return SODatabaseSettings.Instance.NodeIcons[dataNodeType];
                }

                return null;
            });
            tree.SortMenuItemsByName();
            tree.Selection.SelectionChanged += SelectionChanged;
            return tree;
        }

        private void SelectionChanged(SelectionChangedType obj)
        {
            switch (obj)
            {
                case SelectionChangedType.ItemAdded:
                    ((Holder) MenuTree.Selection.SelectedValue).Select();
                    break;
                case SelectionChangedType.ItemRemoved:
                    break;
                case SelectionChangedType.SelectionCleared:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        private void AddAllAssetsAtPath(
            OdinMenuTree tree,
            string assetFolderPath,
            Type type)
        {
            var strings = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase));

            var odinMenuItemSet = new HashSet<OdinMenuItem>();
            foreach (var str1 in strings)
            {
                var asset = AssetDatabase.LoadAssetAtPath(str1, type);
                var path = string.Empty;
                string assetName;
                string str2;
                if (asset == null)
                {
                    //it's a directory
                    str2 = str1.Substring(assetFolderPath.Length);
                    int length = str2.LastIndexOf('/');
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
                    tree.AddMenuItemAtPath(odinMenuItemSet, path, new OdinMenuItem(tree, assetName, new FolderHolder(path, assetName)));

                    continue;
                }

                var withoutExtension = Path.GetFileNameWithoutExtension(str1);

                str2 = (PathUtilities.GetDirectoryName(str1).TrimEnd('/') + "/").Substring(assetFolderPath.Length);
                if (str2.Length != 0)
                    path = path.Trim('/') + "/" + str2;

                path = path.Trim('/') + "/" + withoutExtension;
                SplitMenuPath(path, out path, out assetName);
                var menuItem = new OdinMenuItem(tree, assetName, new DataNodeHolder(path, assetName, (DataNode) asset));
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
                path = string.Empty;
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
            if (_inSettings)
            {
                base.OnBeginDrawEditors();
                if (SODatabaseSettings.Path != string.Empty)
                {
                    Close();
                    Open();
                }

                return;
            }

            if (MenuTree == null || MenuTree.Selection == null)
                return;
            var selected = MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    var type = selected.Value switch
                    {
                        DataNodeHolder holder => holder.DataNode.GetType().ToString(),
                        _ => "Directory"
                    };
                    GUILayout.Label(type);
                }

                var path = SODatabaseSettings.Path;
                if (MenuTree.Selection.SelectedValue != null)
                    path += string.IsNullOrEmpty((MenuTree.Selection.SelectedValue as Holder)!.Path) ? string.Empty : (MenuTree.Selection.SelectedValue as Holder)!.Path + "/";
                if (MenuTree.Selection.SelectedValue is FolderHolder folderHolder)
                    path += folderHolder.Name + "/";
                path = path.Substring(0, path.Length - 1);
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create DataNode")))
                    DataNodeCreator.ShowDialog<DataNode>(path, TrySelectMenuItemWithObject);

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Folder")))
                {
                    var uniqName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + "New");
                    uniqName = uniqName.Substring(uniqName.LastIndexOf('/') + 1);

                    AssetDatabase.CreateFolder(path, uniqName);
                    AssetDatabase.Refresh();
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Save")))
                    Save();
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void Save()
        {
            AssetDatabase.Refresh();
            
            Flatten(MenuTree.MenuItems).ForEach(item =>
            {
                if (!(item.Value is DataNodeHolder dataNodeHolder))
                    return;
                var fullPath = $"{(string.IsNullOrEmpty(dataNodeHolder.Path) ? string.Empty : $"{dataNodeHolder.Path}/")}{dataNodeHolder.DataNode.name}";
                if (dataNodeHolder.DataNode.FullPath == fullPath)
                    return;
                dataNodeHolder.DataNode.SetFullPath(dataNodeHolder.Path);
                EditorUtility.SetDirty(dataNodeHolder.DataNode);
            });

            AssetDatabase.SaveAssets();
            OnSave?.Invoke();
        }

        private static IEnumerable<OdinMenuItem> Flatten(IEnumerable<OdinMenuItem> collection)
        {
            foreach (var o in collection)
            {
                foreach (var o1 in Flatten(o.ChildMenuItems))
                    yield return o1;
                
                yield return o;
            }
        }

        private void AddDragHandles(OdinMenuItem menuItem)
        {
            menuItem.OnDrawItem += _ => DragAndDropUtilities.DragZone(menuItem.Rect, (menuItem.Value as DataNodeHolder)!.DataNode, false, false);
        }
    }
}
#endif