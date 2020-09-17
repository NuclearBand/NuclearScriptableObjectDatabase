using System.Collections.Generic;

#nullable enable

namespace NuclearBand
{
    public class SODatabaseSaveFormat
    {
        public Dictionary<string, string> StaticNodes = new Dictionary<string, string>();
        public Dictionary<string, string> RuntimeNodes = new Dictionary<string, string>();
    }
}