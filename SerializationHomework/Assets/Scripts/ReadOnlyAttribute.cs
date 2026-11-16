using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
    {
        var element = base.CreatePropertyGUI(property) ?? new UnityEditor.UIElements.PropertyField(property);
        element.SetEnabled(false);
        return element;
    }
}

#endif