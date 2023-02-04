using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nuclear.SODatabase
{
    public interface ISODatabase
    {
        void Init(Action<float>? onProgress, Action? onComplete);
        Awaitable InitAsync(Action<float>? onProgress, Action? onComplete);
        T GetModel<T>(string path) where T : DataNode;
        IReadOnlyList<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode;
        void Save();
        Awaitable SaveAsync();
        void Load();
        Awaitable LoadAsync();
        T GetRuntimeModel<T>(string path, Func<T>? allocator = null) where T : class;
    }
}