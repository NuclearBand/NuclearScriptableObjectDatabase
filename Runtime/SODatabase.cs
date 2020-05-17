#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace NuclearBand
{
    public static class SODatabase
    {
        private static FolderHolder root; 
        
        public static async Task Init(Action<float> onProgress, Action onComplete)
        {
            var loadHandler = Addressables.LoadResourceLocationsAsync(SODatabaseSettings.Label);
            Task.Run(async () =>
            {
                while (!loadHandler.IsDone)
                {
                    onProgress?.Invoke(loadHandler.PercentComplete);
                    await Task.Delay(50);
                }

                onProgress?.Invoke(loadHandler.PercentComplete);
            });
            var resourceLocations = await loadHandler.Task;
            
            var loadTasks = resourceLocations.ToDictionary(resourceLocation => resourceLocation.PrimaryKey.Substring(SODatabaseSettings.Label.Length + 1), resourceLocation => Addressables.LoadAssetAsync<DataNode>(resourceLocation).Task);
            await Task.WhenAll(loadTasks.Values);
            root = new FolderHolder("", "");
            foreach (var loadTask in loadTasks)
            {
                //SODatabase/Example1Folder/Example1.asset
                var pathElements = loadTask.Key.Split('/');
                var curFolder = root;
                for (var i = 0; i < pathElements.Length - 1; i++)
                {
                    if (!curFolder.FolderHolders.ContainsKey(pathElements[i]))
                        curFolder.FolderHolders.Add(pathElements[i], new FolderHolder("", ""));

                    curFolder = curFolder.FolderHolders[pathElements[i]];
                }

                var dataNodeName = pathElements[pathElements.Length - 1];
                dataNodeName = dataNodeName.Substring(0, dataNodeName.IndexOf(".asset", StringComparison.Ordinal));
                curFolder.DataNodes.Add(dataNodeName, loadTask.Value.Result);
            }

            onComplete?.Invoke();
        }

        public static T GetModel<T>(string path) where T : DataNode
        {
            var pathElements = path.Split('/');
            var curFolder = root;
            for (var i = 0; i < pathElements.Length - 1; i++)
                curFolder = curFolder.FolderHolders[pathElements[i]];

            var dataNodeName = pathElements[pathElements.Length - 1];
            return curFolder.DataNodes[dataNodeName] as T;
        }

        public static List<T> GetModels<T>(string path) where T : DataNode
        {
            var pathElements = path.Split('/');
            var curFolder = root;
            for (var i = 0; i < pathElements.Length; i++)
                curFolder = curFolder.FolderHolders[pathElements[i]];

            return curFolder.DataNodes.Values.OfType<T>().ToList();
        }

#if UNITY_EDITOR
        public static T GetModelForEdit<T>(string path) where T : DataNode => AssetDatabase.LoadAssetAtPath(SODatabaseSettings.Path + path + ".asset", typeof(T)) as T;
#endif

    }
}

