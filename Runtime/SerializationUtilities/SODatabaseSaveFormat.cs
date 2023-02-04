#nullable enable
using System.Collections.Generic;

namespace Nuclear.SODatabase
{
    public class SODatabaseSaveFormat
    {
        public Dictionary<string, string> StaticNodes = new();
        public readonly Dictionary<string, string> RuntimeNodes = new();
    }
}