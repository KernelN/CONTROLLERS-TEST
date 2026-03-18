using UnityEngine;

namespace Universal.Attributes
{
    /// <summary>
    /// Attribute that require implementation of the provided type.
    /// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
    /// </summary>
    public class RestrictTypeAttribute : PropertyAttribute
    {
        // Object type.
        public readonly System.Type requiredType;

        /// <summary>
        /// Requiring implementation of the <see cref="T:RestrictTypeAttribute"/> type.
        /// </summary>
        /// <param name="type">Interface type.</param>
        public RestrictTypeAttribute(System.Type type)
        {
            this.requiredType = type;
        }
    }
}