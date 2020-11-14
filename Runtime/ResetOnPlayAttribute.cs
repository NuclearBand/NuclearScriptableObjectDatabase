#nullable enable
using System;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace NuclearBand
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ResetOnPlay : Attribute
    {
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class PlayStateNotifier
    {
        static PlayStateNotifier()
        {
            EditorApplication.playModeStateChanged -= ModeChanged;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        static void ModeChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode) 
                return;
            var models = SODatabaseInternal.GetModelsForEdit<DataNode>("");
            foreach (var model in models)
            {
                var typeInfo = model.GetType().GetTypeInfo();
                var fields = typeInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var attributes = field.GetCustomAttributes(typeof(ResetOnPlay), false);
                    if (attributes.Length > 0)
                        field.SetValue(model, default);
                }
            }
        }
    }
#endif
}