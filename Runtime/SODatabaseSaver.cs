#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Nuclear.SODatabase
{
    internal class SODatabaseSaver : ISODatabaseSaver
    {
        internal static string SavePath => Application.persistentDataPath + @"/save.txt";
        // ReSharper disable once MemberCanBePrivate.Global
        internal static string SaveBakPath => Application.persistentDataPath + @"/save.bak";

        private readonly FolderHolder _root;
        private readonly Dictionary<string, object> _runtimeModels;

        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly JsonSerializerSettings _jsonRuntimeSerializerSettings;
        
        private bool _saving;

        public SODatabaseSaver(ISODatabase database, FolderHolder root, Dictionary<string, object> runtimeModels)
        {
            _root = root;
            _runtimeModels = runtimeModels;
            
            _jsonSerializerSettings = new()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceResolverProvider = () => new DataNodeReferenceResolver(database),
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Converters = SODatabaseJsonConverterAttributeUtility.GetCustomConverters()
            };
            
            _jsonRuntimeSerializerSettings = new()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceResolverProvider = () => new DataNodeReferenceResolver(database),
                Converters = SODatabaseJsonConverterAttributeUtility.GetCustomConverters()
            };
        }

        async Awaitable ISODatabaseSaver.SaveAsync()
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
                var json = JsonConvert.SerializeObject(dataNode, _jsonSerializerSettings);
                staticNodes.Add(dataNode.FullPath, json);
            }
            
            var save = new SODatabaseSaveFormat
            {
                StaticNodes = staticNodes
            };
            foreach (var runtimeModelPair in _runtimeModels) 
                save.RuntimeNodes.Add(runtimeModelPair.Key, JsonConvert.SerializeObject(runtimeModelPair.Value, _jsonRuntimeSerializerSettings));

            await using var fileStream = new StreamWriter(SavePath);
            await fileStream.WriteAsync(JsonConvert.SerializeObject(save));
            _saving = false;
        }

        async Awaitable ISODatabaseSaver.LoadAsync()
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
                        JsonConvert.PopulateObject(json, dataNode, _jsonSerializerSettings);
                    }

                    _runtimeModels.Clear();
                    foreach (var runtimeNodePair in save.RuntimeNodes)
                    {
                        var x = JsonConvert.DeserializeObject(runtimeNodePair.Value, _jsonRuntimeSerializerSettings)!;
                        _runtimeModels.Add(runtimeNodePair.Key, x);
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
        
        private IEnumerable<DataNode> DataNodes(FolderHolder folderHolder)
        {
            foreach (var dataNodePair in folderHolder.DataNodes)
                yield return dataNodePair.Value;

            foreach (var folderHolderPair in folderHolder.FolderHolders)
                foreach (var dataNode in DataNodes(folderHolderPair.Value))
                    yield return dataNode;
        }
    }
}