#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
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
        public static string SavePath => Application.persistentDataPath + @"/save.txt";

        private static FolderHolder root = null!;

        private static JsonSerializerSettings jsonSerializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ReferenceResolverProvider = () => new DataNodeReferenceResolver()
        };

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
                    CallAction(() => { onProgress?.Invoke(loadHandler.PercentComplete); });
                    await Task.Delay(50);
                }

                CallAction(() => { onProgress?.Invoke(loadHandler.PercentComplete); });
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
            await Task.CompletedTask;
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
                    if (curFolder.FolderHolders.ContainsKey(pathElements[i]))
                        curFolder = curFolder.FolderHolders[pathElements[i]];
                    else
                        break;
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

        public static async void Save()
        {
            await SaveAsync();
        }

        public static async Task SaveAsync()
        {
            var res = SaveFolderHolder(root, string.Empty);
            using var fileStream = new StreamWriter(SavePath);
            await fileStream.WriteAsync(JsonConvert.SerializeObject(res));
        }

        static Dictionary<string, string> SaveFolderHolder(FolderHolder folderHolder, string path)
        {
            var res = new Dictionary<string, string>();
            foreach (var dataNodePair in folderHolder.DataNodes)
            {
                var fullPath = dataNodePair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                DataNodeReferenceResolver.CurrentDataNode = dataNodePair.Value;
                var json = JsonConvert.SerializeObject(dataNodePair.Value, jsonSerializerSettings);
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

        public static async void Load()
        {
            await LoadAsync();
        }

        public static async Task LoadAsync()
        {
            if (!File.Exists(SavePath))
                return;
            
            using var fileStream = new StreamReader(SavePath);
            var serializedDictionary = await fileStream.ReadToEndAsync();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedDictionary);
            LoadFolderHolder(root, string.Empty, dict);
        }


        static void LoadFolderHolder(FolderHolder folderHolder, string path, Dictionary<string, string> data)
        {
            foreach (var dataNodePair in folderHolder.DataNodes)
            {
                var fullPath = dataNodePair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                var json = data.ContainsKey(fullPath) ? data[fullPath] : string.Empty;
                if (string.IsNullOrEmpty(json))
                    continue;
                DataNodeReferenceResolver.CurrentDataNode = dataNodePair.Value; 
                JsonConvert.PopulateObject(json, dataNodePair.Value, jsonSerializerSettings);
            }

            foreach (var folderHolderPair in folderHolder.FolderHolders)
            {
                var fullPath = folderHolderPair.Key;
                if (!string.IsNullOrEmpty(path))
                    fullPath = path + '/' + fullPath;
                LoadFolderHolder(folderHolderPair.Value, fullPath, data);
            }
        }
    }
}