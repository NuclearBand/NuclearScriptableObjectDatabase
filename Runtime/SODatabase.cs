#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NuclearBand
{
    public static class SODatabase
    {
        public static string SavePath => Application.persistentDataPath + @"/save.txt";
        // ReSharper disable once MemberCanBePrivate.Global
        public static string SaveBakPath => Application.persistentDataPath + @"/save.bak";

        private static FolderHolder _root = null!;

        private static readonly Dictionary<string, object> RuntimeModels = new();

        private static bool _saving;
        
        private static JsonSerializerSettings JsonSerializerSettings => new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
            ReferenceResolverProvider = () => new DataNodeReferenceResolver(),
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };
        
        private static JsonSerializerSettings JsonRuntimeSerializerSettings => new()
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

            var loadTasks = new Dictionary<string, Task<DataNode>>();
            foreach (var resourceLocation in resourceLocations)
            {
                var key = resourceLocation.PrimaryKey[SODatabaseSettings.Path.Length..];
                if (!resourceLocation.ResourceType.IsSubclassOf(typeof(DataNode)) || loadTasks.ContainsKey(key))
                    continue;
                loadTasks.Add(key, Addressables.LoadAssetAsync<DataNode>(resourceLocation).Task);
            }
            await Task.WhenAll(loadTasks.Values);
            _root = new FolderHolder();
            foreach (var loadTask in loadTasks)
            {
                //SODatabase/Example1Folder/Example1.asset
                var pathElements = loadTask.Key.Split('/');
                var curFolder = _root;
                for (var i = 0; i < pathElements.Length - 1; i++)
                {
                    if (!curFolder.FolderHolders.ContainsKey(pathElements[i]))
                        curFolder.FolderHolders.Add(pathElements[i], new FolderHolder());

                    curFolder = curFolder.FolderHolders[pathElements[i]];
                }

                var dataNodeName = pathElements[^1];
                dataNodeName = dataNodeName.Substring(0, dataNodeName.IndexOf(".asset", StringComparison.Ordinal));
                curFolder.DataNodes.Add(dataNodeName, loadTask.Value.Result);
            }

            CallAction(onComplete);
        }

        private static async void CallAction(Action? action)
        {
            action?.Invoke();
            await Task.CompletedTask;
        }

        public static T GetModel<T>(string path) where T : DataNode
        {
            var pathElements = path.Split('/');
            var curFolder = _root;
            for (var i = 0; i < pathElements.Length - 1; i++)
                curFolder = curFolder.FolderHolders[pathElements[i]];

            var dataNodeName = pathElements[^1];
            return (T) curFolder.DataNodes[dataNodeName];
        }

        public static List<T> GetModels<T>(string path, bool includeSubFolders = false) where T : DataNode
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
            if (_saving)
                return;
            
            _saving = true;
            if (File.Exists(SavePath))
                File.Copy(SavePath, SaveBakPath, true);
            
            var staticNodes = new Dictionary<string, string>();
            foreach (var dataNode in DataNodes(_root))
            {
                DataNodeReferenceResolver.CurrentDataNode = dataNode;
                dataNode.BeforeSave();
                var json = JsonConvert.SerializeObject(dataNode, JsonSerializerSettings);
                staticNodes.Add(dataNode.FullPath, json);
            }
            
            var save = new SODatabaseSaveFormat
            {
                StaticNodes = staticNodes
            };
            foreach (var runtimeModelPair in RuntimeModels) 
                save.RuntimeNodes.Add(runtimeModelPair.Key, JsonConvert.SerializeObject(runtimeModelPair.Value, JsonRuntimeSerializerSettings));

            await using var fileStream = new StreamWriter(SavePath);
            await fileStream.WriteAsync(JsonConvert.SerializeObject(save));
            _saving = false;
        }

        public static async void Load()
        {
            await LoadAsync();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static async Task LoadAsync()
        {
            if (!File.Exists(SavePath)) {
                foreach (var dataNode in DataNodes(_root))
                    dataNode.AfterLoad();
                return;
            }

            do
            {
                try
                {
                    using var fileStream = new StreamReader(SavePath);
                    var serializedDictionary = await fileStream.ReadToEndAsync();
                    var save = JsonConvert.DeserializeObject<SODatabaseSaveFormat>(serializedDictionary);
                    if (save == null)
                        throw new Exception();

                    foreach (var dataNode in DataNodes(_root))
                    {
                        DataNodeReferenceResolver.CurrentDataNode = dataNode;
                        var json = save.StaticNodes.ContainsKey(dataNode.FullPath)
                            ? save.StaticNodes[dataNode.FullPath]
                            : string.Empty;
                        if (string.IsNullOrEmpty(json))
                            continue;
                        DataNodeReferenceResolver.CurrentDataNode = dataNode;
                        JsonConvert.PopulateObject(json, dataNode, JsonSerializerSettings);
                    }

                    RuntimeModels.Clear();
                    foreach (var runtimeNodePair in save.RuntimeNodes)
                    {
                        var x = JsonConvert.DeserializeObject(runtimeNodePair.Value, JsonRuntimeSerializerSettings)!;
                        RuntimeModels.Add(runtimeNodePair.Key, x);
                    }

                    break;
                }
                catch (Exception)
                {
                    if (File.Exists(SaveBakPath))
                    {
                        File.Copy(SaveBakPath, SavePath, true);
                        File.Delete(SaveBakPath);
                        continue;
                    }

                    break;
                }
            } while (true);

            foreach (var dataNode in DataNodes(_root))
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
            if (RuntimeModels.ContainsKey(path))
                model = (T) RuntimeModels[path];
            else
            {
                model = allocator!();
                RuntimeModels.Add(path, model);
            }
            return model;
        }
    }
}