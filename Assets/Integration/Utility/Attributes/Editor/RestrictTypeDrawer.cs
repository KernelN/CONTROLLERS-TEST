using UnityEngine;
using UnityEditor;

namespace Universal.Attributes
{
    /// <summary>
    /// Drawer for the RestrictType attribute.
    /// https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/
    /// https://youtu.be/zkr6LU3kHE4 (from OtterKnight)
    /// </summary>
    [CustomPropertyDrawer(typeof(RestrictTypeAttribute))]
    public class RestrictTypeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Overrides GUI drawing for the attribute.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Check if this is reference type property.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Get attribute parameters.
                var requiredAttribute = this.attribute as RestrictTypeAttribute;
                System.Type reqType = requiredAttribute.requiredType;
                
                // Begin drawing property field.
                EditorGUI.BeginProperty(position, label, property);
                
                // Draw property field.
                Object obj = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Object), !EditorUtility.IsPersistent(property.serializedObject.targetObject));

                if (EditorGUI.EndChangeCheck())
                {
                    if (obj == null)
                        property.objectReferenceValue = null;
                    else if (reqType.IsInstanceOfType(obj))
                        property.objectReferenceValue = obj;
                    else if (obj is GameObject)
                    {
                        //This would be risky in another situation, but we know it's a monobehaviour
                        MonoBehaviour c = (MonoBehaviour)((GameObject)obj).GetComponent(reqType);
                        if (c != null)
                            property.objectReferenceValue = c;
                    }
                }

                // Finish drawing property field.
                EditorGUI.EndProperty();
            }
            else
            {
                // If field is not reference, show error message.
                // Save previous color and change GUI to red.
                var previousColor = GUI.color;
                GUI.color = Color.red;

                // Display label with error message.
                EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));

                // Revert color change.
                GUI.color = previousColor;
            }
        }
    }
}