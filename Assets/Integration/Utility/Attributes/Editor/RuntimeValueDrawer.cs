using UnityEditor;
using UnityEngine;

namespace Universal.Attributes
{
    [CustomPropertyDrawer(typeof(RuntimeValueAttribute))]
    public class RuntimeValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //object var = fieldInfo.GetValue(property.serializedObject.targetObject);
            object var = property.floatValue;
            EditorGUI.LabelField(position, label, new GUIContent(var.ToString()));
        }
    }
}