using UnityEngine;

namespace Universal.Attributes
{
    
    /// <summary>
    /// Attribute that sets the array size and naming to the names of the enum.
    /// https://youtu.be/pCFl1Z-Xf5Y?si=0EXq1ESM3wfQ-hEp
    /// </summary>
    public class EnumArrayAttribute : PropertyAttribute
    {
        public readonly string[] names;
        
        public EnumArrayAttribute(System.Type enumType)
        {
            names = System.Enum.GetNames(enumType);
        }
    }
}