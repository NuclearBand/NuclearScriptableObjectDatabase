#nullable enable
using System;
using Newtonsoft.Json.Serialization;

namespace Nuclear.SODatabase
{
    public class DataNodeReferenceResolver : IReferenceResolver
    {
        private readonly ISODatabase _soDatabase;
        public static DataNode CurrentDataNode = null!;

        public DataNodeReferenceResolver(ISODatabase soDatabase) => _soDatabase = soDatabase;

        public object ResolveReference(object context, string reference)
        {
            try
            {
                return _soDatabase.GetModel<DataNode>(reference);
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public string GetReference(object context, object value)
        {
            var dataNode = (DataNode) value;
            return dataNode.FullPath;
        }

        public bool IsReferenced(object context, object value) => !ReferenceEquals(value, CurrentDataNode);
        public void AddReference(object context, string reference, object value) { }
    }
}