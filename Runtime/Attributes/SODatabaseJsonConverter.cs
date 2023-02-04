#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nuclear.SODatabase
{
    [AttributeUsage(AttributeTargets.Class)] public class SODatabaseJsonConverter : Attribute { }

    // TODO: test it
    internal static class SODatabaseJsonConverterAttributeUtility
    {
        internal static IList<JsonConverter> GetCustomConverters()
        {
            var converters = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(JsonConverter).IsAssignableFrom(p) &&
                            p.IsDefined(typeof(SODatabaseJsonConverter), false))
                .Select(Activator.CreateInstance)
                .Cast<JsonConverter>()
                .ToList();
            return converters;
        }
    }
}