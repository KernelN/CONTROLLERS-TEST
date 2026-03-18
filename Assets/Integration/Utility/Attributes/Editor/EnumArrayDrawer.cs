using UnityEditor;
using UnityEngine;

namespace Universal.Attributes
{
    /// <summary>
    /// https://youtu.be/pCFl1Z-Xf5Y?si=0EXq1ESM3wfQ-hEp
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumArrayAttribute))]
    public class EnumArrayDrawer : PropertyDrawer
    {
        SerializedProperty array;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string path = GetPath(property);
            string newlabel = GetTitle(path);
            if (string.IsNullOrEmpty(newlabel)) newlabel = label.text;
            position.height = GetPropertyHeight(property, label);
            EditorGUI.PropertyField(position, property, new GUIContent(newlabel, label.tooltip), true);
        }
        string GetPath(SerializedProperty property)
        {
            string path = property.propertyPath;
            
            if (array == null)
            {
                array = property.serializedObject.FindProperty
                    (path.Substring(0, path.LastIndexOf('.')));
                if (array == null) return "";
            }

            return path;
        }
        string GetTitle(string path)
        {
            EnumArrayAttribute attr = (EnumArrayAttribute)attribute;
            
            int aSize = attr.names.Length;
            if(attr.names[^1] == "_count") aSize--; //don't take into account _count
            
            if (array.arraySize != aSize)
                array.arraySize = aSize;
        
            // These two lines are edited from the video to fix an issue of nested arrays and garbage from Replace.
            int indexOfNum = path.LastIndexOf('[') + 1;
            int index = System.Convert.ToInt32(path.Substring(indexOfNum, path.LastIndexOf(']') - indexOfNum));
            return attr.names[index];
        }
    }
}