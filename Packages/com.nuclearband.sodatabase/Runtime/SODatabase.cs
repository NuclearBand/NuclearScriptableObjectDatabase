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

        private static Dictionary<string, object> runtimeModels = new Dictionary<string, object>();

        private static JsonSerializerSettings jsonSerializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceResolverProvider = () => new DataNodeReferenceResolver(),
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
        
        private static JsonSerializerSettings jsonRuntimeSerializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
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
            var staticNodes = new Dictionary<string, string>();
            foreach (var dataNode in DataNodes(root))
            {
                DataNodeReferenceResolver.CurrentDataNode = dataNode;
                dataNode.BeforeSave();
                var json = JsonConvert.SerializeObject(dataNode, jsonSerializerSettings);
                staticNodes.Add(dataNode.FullPath, json);
            }
            
            var save = new SODatabaseSaveFormat
            {
                StaticNodes = staticNodes
            };
            foreach (var runtimeModelPair in runtimeModels) 
                save.RuntimeNodes.Add(runtimeModelPair.Key, JsonConvert.SerializeObject(runtimeModelPair.Value, jsonRuntimeSerializerSettings));
            
            using var fileStream = new StreamWriter(SavePath);
            await fileStream.WriteAsync(JsonConvert.SerializeObject(save));
        }

        public static async void Load()
        {
            await LoadAsync();
        }

        public static async Task LoadAsync()
        {
            if (!File.Exists(SavePath)) {
                foreach (var dataNode in DataNodes(root))
                    dataNode.AfterLoad();
                return;
            }
            
            using var fileStream = new StreamReader(SavePath);
            var serializedDictionary = await fileStream.ReadToEndAsync();
            var save = JsonConvert.DeserializeObject<SODatabaseSaveFormat>(serializedDictionary);
            
            foreach (var dataNode in DataNodes(root))
            {
                DataNodeReferenceResolver.CurrentDataNode = dataNode;
                var json = save.StaticNodes.ContainsKey(dataNode.FullPath) ? save.StaticNodes[dataNode.FullPath] : string.Empty;
                if (string.IsNullOrEmpty(json))
                    continue;
                DataNodeReferenceResolver.CurrentDataNode = dataNode; 
                JsonConvert.PopulateObject(json, dataNode, jsonSerializerSettings);
            }
            
            runtimeModels.Clear();
            foreach (var runtimeNodePair in save.RuntimeNodes)
            {
                var x = JsonConvert.DeserializeObject(runtimeNodePair.Value, jsonRuntimeSerializerSettings)!;
                runtimeModels.Add(runtimeNodePair.Key, x);
            }

            foreach (var dataNode in DataNodes(root))
                dataNode.AfterLoad();
        }
        private static IEnumerable<DataNode> DataNodes(FolderHolder folderHolder)
        {
            foreach (var dataNodePair in folderHolder.DataNodes)
                yield return dataNodePair.Value;

            foreach (var folderHolderPair in folderHolder.FolderHolders)
                foreach (var dataNode in DataNodes(folderHolderPair.Value))
                    yield return dataNode;
        }
        
        public static T GetRuntimeModel<T>(string path, Func<T>? allocator = null) where T : class
        {
            T model;
            if (runtimeModels.ContainsKey(path))
                model = (T) runtimeModels[path];
            else
            {
                model = allocator!();
                runtimeModels.Add(path, model);
            }
            return model;
        }
    }
}