#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TriInspector;
using TriInspector.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nuclear.SODatabase.Editor
{
    public class ScriptableObjectDatabaseEditorWindow : EditorWindow
    {
        public static event Action? OnSave;
        
        private MenuTree _menuTree;
        private SearchField _searchField;
        
        private SerializedObject _currentSerializedObject;
        private TriPropertyTree _currentPropertyTree;
        private Vector2 _currentScroll;
        
        private bool _inSettings;
        
        [MenuItem("Tools/NuclearBand/ScriptableObjectDatabase")]
        private static void Open()
        {
            var window = GetWindow<ScriptableObjectDatabaseEditorWindow>();
            window.titleContent = new GUIContent("ScriptableObjectDatabase");
            window.Show();
            //window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
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

        private void OnEnable()
        {
            _menuTree = new MenuTree(new TreeViewState());
            _menuTree.OnSelect += ChangeCurrentSample;

            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _menuTree.SetFocusAndEnsureSelectedItem;
        }
        
        private void OnDisable()
        {
            ChangeCurrentSample(null);
        }
        
        
        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    DrawMenu();
                }

                var separatorRect = GUILayoutUtility.GetLastRect();
                separatorRect.xMin = separatorRect.xMax;
                separatorRect.xMax += 1;
                GUI.Box(separatorRect, "");

                using (new GUILayout.VerticalScope())
                {
                    DrawElement();
                }
            }
        }

        private void DrawMenu()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUILayout.Space(5);
                _menuTree.searchString = _searchField.OnToolbarGUI(_menuTree.searchString, GUILayout.ExpandWidth(true));
                GUILayout.Space(5);
            }

            var menuRect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _menuTree.OnGUI(menuRect);
        }

        private void DrawElement()
        {
            if (_currentPropertyTree == null)
            {
                return;
            }

            using (var scrollScope = new GUILayout.ScrollViewScope(_currentScroll))
            {
                _currentScroll = scrollScope.scrollPosition;

                using (new GUILayout.VerticalScope(SampleWindowStyles.Padding))
                {
                    
                    _currentSerializedObject.UpdateIfRequiredOrScript();
                    _currentPropertyTree.Update();
                    _currentPropertyTree.RunValidationIfRequired();

                    _currentPropertyTree.Draw();
                    

                    if (_currentSerializedObject.ApplyModifiedProperties())
                    {
                        _currentPropertyTree.RequestValidation();
                    }

                    if (_currentPropertyTree.RepaintRequired)
                    {
                        Repaint();
                    }


                   
                }
            }
        }
        
        private void ChangeCurrentSample(Holder holder)
        {
            if (_currentPropertyTree != null)
            {
                _currentPropertyTree.Dispose();
                _currentPropertyTree = null;
            }

            _currentScroll = Vector2.zero;

            if (holder != null)
            {
                _currentSerializedObject = new SerializedObject((holder as DataNodeHolder)?.DataNode);
                _currentPropertyTree = new TriPropertyTreeForSerializedObject(_currentSerializedObject);
            }
        }
        
       

        

        

        

        /*protected override void OnBeginDrawEditors()
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

            if (MenuTree?.Selection == null)
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
                path = path[..^1];
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
        }*/
    }
    
    internal static class SampleWindowStyles
    {
        public static readonly GUIStyle Padding;
        public static readonly GUIStyle BoxWithPadding;
        public static readonly GUIStyle HeaderDisplayNameLabel;

        static SampleWindowStyles()
        {
            Padding = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(5, 5, 5, 5),
            };

            BoxWithPadding = new GUIStyle(TriEditorStyles.Box)
            {
                padding = new RectOffset(5, 5, 5, 5),
            };

            HeaderDisplayNameLabel = new GUIStyle(EditorStyles.largeLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 17,
                margin = new RectOffset(5, 5, 5, 0),
            };
        }
    }
}
