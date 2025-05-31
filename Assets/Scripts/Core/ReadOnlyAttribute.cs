using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// Property attribute to make fields read-only in the inspector.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute() { }
    }
}