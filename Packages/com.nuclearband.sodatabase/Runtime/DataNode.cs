using System.Reflection;
using Sirenix.OdinInspector;

namespace NuclearBand
{
    public class DataNode : SerializedScriptableObject
    {
        protected virtual void OnEnable()
        {
            var typeInfo = GetType().GetTypeInfo();
            var fields = typeInfo.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(ResetOnPlay), false);
                if (attributes.Length > 0)
                    field.SetValue(this, default);
            }
        }
    }
}