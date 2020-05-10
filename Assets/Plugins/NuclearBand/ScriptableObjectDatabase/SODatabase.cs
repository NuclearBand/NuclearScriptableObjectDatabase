using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NuclearBand
{
    public static class SODatabase
    {
        public static T GetModel<T>(string path) where T : DataNode => Resources.Load<T>(path);

        public static T[] GetModels<T>(string path) where T : DataNode => Resources.LoadAll<T>(path);
    }
}

