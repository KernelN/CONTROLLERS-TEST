using UnityEngine;

namespace Universal.Attributes
{
    public class ArrayElementTitleAttribute : PropertyAttribute
    {
        public string varname;
        public ArrayElementTitleAttribute(string elementTitleVar)
        {
            varname = elementTitleVar;
        }
    }
}
