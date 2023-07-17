using System.Threading.Tasks;

namespace Nuclear.SODatabase
{
    internal interface ISODatabaseSaver
    {
        Task SaveAsync();
        Task LoadAsync();
    }
}