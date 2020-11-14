#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace NuclearBand.Editor
{
    public static class DataNodeCreator
    {
        public static void ShowDialog<T>(string defaultDestinationPath, Action<T>? onScriptableObjectCreated = null)
            where T : ScriptableObject
        {
            var selector = new ScriptableObjectSelector<T>(defaultDestinationPath, onScriptableObjectCreated);

            if (selector.SelectionTree.EnumerateTree().Count() == 1)
            {
                selector.SelectionTree.EnumerateTree().First().Select();
                selector.SelectionTree.Selection.ConfirmSelection();
            }
            else
            {
                selector.ShowInPopup(200);
            }
        }

        private class ScriptableObjectSelector<T> : OdinSelector<Type> where T : ScriptableObject
        {
            private readonly Action<T>? onScriptableObjectCreated;
            private readonly string defaultDestinationPath;

            public ScriptableObjectSelector(string defaultDestinationPath, Action<T>? onScriptableObjectCreated = null)
            {
                this.onScriptableObjectCreated = onScriptableObjectCreated;
                this.defaultDestinationPath = defaultDestinationPath;
                this.SelectionConfirmed += this.Save;
            }

            protected override void BuildSelectionTree(OdinMenuTree tree)
            {
                var scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                    .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)) && typeof(T) != x);

                tree.Selection.SupportsMultiSelect = false;
                tree.Config.DrawSearchToolbar = true;
                tree.AddRange(scriptableObjectTypes, x => x.GetNiceName())
                    .AddThumbnailIcons();
            }

            private void Save(IEnumerable<Type> selection)
            {
                var obj = (ScriptableObject.CreateInstance(selection.FirstOrDefault()) as T)!;

                var dest = this.defaultDestinationPath.TrimEnd('/');

                AssetDatabase.CreateAsset(obj, AssetDatabase.GenerateUniqueAssetPath(dest + "/" + "New.asset"));
                AssetDatabase.Refresh();

                onScriptableObjectCreated?.Invoke(obj);
            }
        }
    }
}

#endif
