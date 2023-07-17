using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nuclear.SODatabase
{
    public interface ISODatabase
    {
        void Init(Action<float>? onProgress, Action? onComplete);
        Task InitAsync(Action<float>? onProgress, Action? onComplete);
        T GetModel<T>(string path) where T : DataNode;
        IReadOnlyList<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode;
        void Save();
        Task SaveAsync();
        void Load();
        Task LoadAsync();
        T GetRuntimeModel<T>(string path, Func<T>? allocator = null) where T : class;
    }
}