#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NuclearBand
{
    public static class SODatabase
    {
        private static FolderHolder root = null!;

        public static async void Init(Action<float>? onProgress, Action? onComplete)
        {
            await InitAsync(onProgress, onComplete);
        }
        public static async Task InitAsync(Action<float>? onProgress, Action? onComplete)
        {
            var loadHandler = Addressables.LoadResourceLocationsAsync(SODatabaseSettings.Label);
#pragma warning disable 4014
            Task.Run(async () =>
            {
                while (!loadHandler.IsDone)
                {
                    CallAction(() =>
                    {
                        onProgress?.Invoke(loadHandler.PercentComplete);
                    });
                    await Task.Delay(50);
                }
                CallAction(() =>
                {
                    onProgress?.Invoke(loadHandler.PercentComplete);
                });
            });
#pragma warning restore 4014
            var resourceLocations = await loadHandler.Task;

            var loadTasks = resourceLocations.ToDictionary(resourceLocation => resourceLocation.PrimaryKey.Substring(SODatabaseSettings.Path.Length), resourceLocation => Addressables.LoadAssetAsync<DataNode>(resourceLocation).Task);
            await Task.WhenAll(loadTasks.Values);
            root = new FolderHolder();
            foreach (var loadTask in loadTasks)
            {
                //SODatabase/Example1Folder/Example1.asset
                var pathElements = loadTask.Key.Split('/');
                var curFolder = root;
                for (var i = 0; i < pathElements.Length - 1; i++)
                {
                    if (!curFolder.FolderHolders.ContainsKey(pathElements[i]))
                        curFolder.FolderHolders.Add(pathElements[i], new FolderHolder());

                    curFolder = curFolder.FolderHolders[pathElements[i]];
                }

                var dataNodeName = pathElements[pathElements.Length - 1];
                dataNodeName = dataNodeName.Substring(0, dataNodeName.IndexOf(".asset", StringComparison.Ordinal));
                curFolder.DataNodes.Add(dataNodeName, loadTask.Value.Result);
            }

            CallAction(onComplete);
        }

        static async void CallAction(Action? action)
        {
            action?.Invoke();
        }

        public static T GetModel<T>(string path) where T : DataNode
        {
            var pathElements = path.Split('/');
            var curFolder = root;
            for (var i = 0; i < pathElements.Length - 1; i++)
                curFolder = curFolder.FolderHolders[pathElements[i]];

            var dataNodeName = pathElements[pathElements.Length - 1];
            return ((T) curFolder.DataNodes[dataNodeName])!;
        }

        public static List<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode
        {
            var pathElements = path.Split('/');
            var curFolder = root;
            if (path != string.Empty)
                for (var i = 0; i < pathElements.Length; i++)
                {
                    try
                    {
                        curFolder = curFolder.FolderHolders[pathElements[i]];
                    }
                    catch (Exception e)
                    {
                        
                    }
                }

            var res = curFolder.DataNodes.Values.OfType<T>().ToList();
            if (!includeSubFolders)
                return res;


            foreach (var folderName in curFolder.FolderHolders.Keys)
            {
                var newPath = path == string.Empty ? folderName : $"{path}/{folderName}";
                res.AddRange(GetModels<T>(newPath, includeSubFolders));
            }

            return res;
        }

        public static void Save()
        {
            var res = SaveFolderHolder(root, string.Empty);
            foreach (var pair in res)
                PlayerPrefs.SetString(pair.Key, pair.Value);
            PlayerPrefs.Save();
        }

        static Dictionary<string, string> SaveFolderHolder(FolderHolder folderHolder, string path)
        {
            var res = new Dictionary<string, string>();
            foreach (var dataNodePair in folderHolder.DataNodes)
            {
                var fullPath = dataNodePair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                var json = JsonConvert.SerializeObject(dataNodePair.Value);
                res.Add(fullPath, json);
            }

            foreach (var folderHolderPair in folderHolder.FolderHolders)
            {
                var fullPath = folderHolderPair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                var resAdd = SaveFolderHolder(folderHolderPair.Value, fullPath);
                resAdd.ForEach(x => res.Add(x.Key, x.Value));
            }

            return res;
        }

        public static void Load()
        {
            LoadFolderHolder(root, string.Empty);
        }

        static void LoadFolderHolder(FolderHolder folderHolder, string path)
        {
            foreach (var dataNodePair in folderHolder.DataNodes)
            {
                var fullPath = dataNodePair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                var json = PlayerPrefs.GetString(fullPath);
                if (string.IsNullOrEmpty(json))
                    continue;
                JsonConvert.PopulateObject(json, dataNodePair.Value);
            }

            foreach (var folderHolderPair in folderHolder.FolderHolders)
            {
                var fullPath = folderHolderPair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                LoadFolderHolder(folderHolderPair.Value, fullPath);
            }
        }
    }
}