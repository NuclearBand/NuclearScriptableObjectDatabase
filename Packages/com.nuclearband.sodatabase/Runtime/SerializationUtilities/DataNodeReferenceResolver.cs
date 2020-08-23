#nullable enable
using System;
using Newtonsoft.Json.Serialization;
using NuclearBand;

public class DataNodeReferenceResolver : IReferenceResolver
{
    public static DataNode CurrentDataNode = null!;

    public object ResolveReference(object context, string reference)
    {
        try
        {
            return SODatabase.GetModel<DataNode>(reference);
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

    public void AddReference(object context, string reference, object value)
    {
    }
}