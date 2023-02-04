#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nuclear.SODatabase
{
    public static class SODatabaseStatic
    {
        private static readonly ISODatabase Instance = new SODatabase();
        
        public static void Init(Action<float>? onProgress, Action? onComplete) => Instance.Init(onProgress, onComplete);
        public static Awaitable InitAsync(Action<float>? onProgress, Action? onComplete) => Instance.InitAsync(onProgress, onComplete);
        public static T GetModel<T>(string path) where T : DataNode => Instance.GetModel<T>(path);
        public static IReadOnlyList<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode => Instance.GetModels<T>(path, includeSubFolders);
        public static void Save() => Instance.Save();
        public static Awaitable SaveAsync() => Instance.SaveAsync();
        public static void Load() => Instance.Load();
        public static Awaitable LoadAsync() => Instance.LoadAsync();
        public static T GetRuntimeModel<T>(string path, Func<T>? allocator = null) where T : class => Instance.GetRuntimeModel(path, allocator);
    }
}