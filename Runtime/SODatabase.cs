#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Nuclear.SODatabase
{
    public class SODatabase : ISODatabase
    {
        private readonly Dictionary<string, object> _runtimeModels = new();
        private ISODatabaseSaver _soDatabaseSaver = null!;
        private FolderHolder _root = null!;

        private ISODatabase This => this;

        async void ISODatabase.Init(Action<float>? onProgress, Action? onComplete) => 
            await This.InitAsync(onProgress, onComplete);

        // ReSharper disable once MemberCanBePrivate.Global
        async Awaitable ISODatabase.InitAsync(Action<float>? onProgress, Action? onComplete)
        {
            var resourceLocations = await LoadResourceLocations(onProgress);
            _root = new();
            var resources = await LoadResources(resourceLocations);
            foreach (var resource in resources)
            {
                // SODatabase/Example1Folder/Example1.asset
                var pathElements = resource.Key.Split('/');
                var curFolder = _root;
                for (var i = 0; i < pathElements.Length - 1; i++)
                {
                    if (!curFolder.FolderHolders.ContainsKey(pathElements[i]))
                        curFolder.FolderHolders.Add(pathElements[i], new FolderHolder());

                    curFolder = curFolder.FolderHolders[pathElements[i]];
                }

                var dataNodeName = pathElements[^1];
                dataNodeName = dataNodeName[..dataNodeName.IndexOf(".asset", StringComparison.Ordinal)];
                curFolder.DataNodes.Add(dataNodeName, resource.Value);
            }

            _soDatabaseSaver = new SODatabaseSaver(this, _root, _runtimeModels);
            onComplete?.Invoke();
        }

        private static async Task<IList<IResourceLocation>> LoadResourceLocations(Action<float>? onProgress)
        {
            var loadHandler = Addressables.LoadResourceLocationsAsync(SODatabaseSettings.Label);
            HandleProgress(onProgress, loadHandler);
            return await loadHandler.Task;
        }

        private static async Task<Dictionary<string, DataNode>> LoadResources(IEnumerable<IResourceLocation> resourceLocations)
        {
            var loadTasks = new Dictionary<string, Task<DataNode>>();
            foreach (var resourceLocation in resourceLocations)
            {
                var key = resourceLocation.PrimaryKey[SODatabaseSettings.Path.Length..];
                if (!resourceLocation.ResourceType.IsSubclassOf(typeof(DataNode)) || loadTasks.ContainsKey(key))
                    continue;
                loadTasks.Add(key, Addressables.LoadAssetAsync<DataNode>(resourceLocation).Task);
            }

            // TODO: Handle progress
            await Task.WhenAll(loadTasks.Values);
            return loadTasks.ToDictionary(loadTask => loadTask.Key, 
                loadTask => loadTask.Value.Result);
        }

        private static void HandleProgress(Action<float>? onProgress, AsyncOperationHandle<IList<IResourceLocation>> loadHandler)
        {
            Task.Run(async () =>
            {
                while (!loadHandler.IsDone)
                {
                    onProgress?.Invoke(loadHandler.PercentComplete);
                    await Task.Delay(50);
                }

                onProgress?.Invoke(loadHandler.PercentComplete);
            });
        }

        T ISODatabase.GetModel<T>(string path)
        {
            var pathElements = path.Split('/');
            var curFolder = _root;
            for (var i = 0; i < pathElements.Length - 1; i++)
                curFolder = curFolder.FolderHolders[pathElements[i]];

            var dataNodeName = pathElements[^1];
            return (T) curFolder.DataNodes[dataNodeName];
        }

        IReadOnlyList<T> ISODatabase.GetModels<T>(string path, bool includeSubFolders)
        {
            var pathElements = path.Split('/');
            var curFolder = _root;
            if (path != string.Empty)
                foreach (var pathElement in pathElements)
                {
                    if (curFolder.FolderHolders.ContainsKey(pathElement))
                        curFolder = curFolder.FolderHolders[pathElement];
                    else
                        break;
                }

            var res = curFolder.DataNodes.Values.OfType<T>().ToList();
            if (!includeSubFolders)
                return res;

            foreach (var folderName in curFolder.FolderHolders.Keys)
            {
                var newPath = path == string.Empty ? folderName : $"{path}/{folderName}";
                res.AddRange(This.GetModels<T>(newPath, includeSubFolders));
            }

            return res;
        }

        async void ISODatabase.Save() => await This.SaveAsync();

        // ReSharper disable once MemberCanBePrivate.Global
        async Awaitable ISODatabase.SaveAsync() => await _soDatabaseSaver.SaveAsync();

        async void ISODatabase.Load() => await This.LoadAsync();

        // ReSharper disable once MemberCanBePrivate.Global
        async Awaitable ISODatabase.LoadAsync() => await _soDatabaseSaver.LoadAsync();

        T ISODatabase.GetRuntimeModel<T>(string path, Func<T>? allocator)
        {
            T model;
            if (_runtimeModels.ContainsKey(path))
                model = (T) _runtimeModels[path];
            else
            {
                model = allocator!();
                _runtimeModels.Add(path, model);
            }
            return model;
        }
    }
}