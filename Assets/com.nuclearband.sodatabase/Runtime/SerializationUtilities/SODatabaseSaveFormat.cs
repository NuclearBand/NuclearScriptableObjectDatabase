using System.Collections.Generic;

#nullable enable

namespace NuclearBand
{
    public class SODatabaseSaveFormat
    {
        public Dictionary<string, string> StaticNodes = new();
        public readonly Dictionary<string, string> RuntimeNodes = new();
    }
}