using UnityEngine;

namespace Nuclear.SODatabase
{
    internal interface ISODatabaseSaver
    {
        Awaitable SaveAsync();
        Awaitable LoadAsync();
    }
}