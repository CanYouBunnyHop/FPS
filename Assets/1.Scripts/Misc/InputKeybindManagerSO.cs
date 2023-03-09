using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.EditorTools;

namespace FPS.Settings.Keybinds
{
[CreateAssetMenu(fileName = "KeybindSettingsSO", menuName = "Settings/KeybindSettingsSO")]
public class InputKeybindManagerSO : ScriptableObject
{
    public InputKeybind fire;
    public InputKeybind AimDownSight;
}

[Serializable]
public sealed class InputKeybind
{
    //public string inputName;
    public KeyCode primaryInput;
    public KeyCode secondaryInput;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InputKeybind))]
public class InputKeybindUIE : PropertyDrawer
{
    Rect foldOutBox;
    SerializedProperty primary;
    SerializedProperty secondary;
    SerializedProperty name;
    private int line = 1;

    //Don't use EditorGUILayout in property drawers but instead EditorGUI or GUI.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
    {
        EditorGUI.BeginProperty(position, label, property);

        foldOutBox = new Rect(position.x, position.y, position.size.x, EditorGUIUtility.singleLineHeight);
        Rect contentPosition = EditorGUI.PrefixLabel(foldOutBox, label); //make a label in front of things ?

        if(primary == null){primary = property.FindPropertyRelative("primaryInput");}
        if(secondary == null){secondary = property.FindPropertyRelative("secondaryInput");}
        //if(name == null){name = property.FindPropertyRelative("inputName");}
            
        Rect tFieldPos = new Rect(60f, position.y, position.width/2, EditorGUIUtility.singleLineHeight);
        Rect lFieldPos = new Rect(position.x, position.y, position.width, position.height);

        //GUIContent inputName = new GUIContent("InputName");
        //EditorGUI.PropertyField(contentPosition, name);
        
        property.isExpanded = EditorGUI.Foldout(foldOutBox, property.isExpanded, label, GUI.skin.textArea);
        contentPosition.y += EditorGUIUtility.singleLineHeight + 2; //this controls how far down the input fields goes below the header

        if(property.isExpanded)
        {
            EditorGUIUtility.labelWidth = 100;    //distance between label and field
            EditorGUI.indentLevel ++;

            Rect contentPosSplit1 = new Rect(position.x, contentPosition.y, position.width / 2 , contentPosition.height);
            EditorGUI.PropertyField(contentPosSplit1, primary);

            Rect contentPosSplit2 = new Rect(position.x + position.width / 2, contentPosition.y, position.width / 2, contentPosition.height);
            EditorGUI.PropertyField(contentPosSplit2, secondary);
            //DrawKeyBindPropertyField(position, contentPosition, out Rect old);
            //position.y += foldOutBox.height + old.height + (EditorGUIUtility.singleLineHeight/2);
        } 
        else
        {
            property.isExpanded = false;
        }
        EditorGUI.EndProperty();
    }
    public Rect AddRect(Rect _rect)
    {
        float y = line * _rect.y;
        return new Rect(_rect.x, y, _rect.width, _rect.height);
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) //the empty background space after expanding each field
    {
        int totalLines = 1;

        if(property.isExpanded)
        {
            totalLines += 1;
        }
        return (EditorGUIUtility.singleLineHeight * (totalLines + property.CountInProperty() - 2)); //the - x at the back is for fine tune the actual space we need
    }
}
#endif

}
