using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(CustomStyles))]
[CanEditMultipleObjects]
public class CustomStylesEditor : Editor
{
    CustomStyles targetObj;
    List<GUIStyle> builtinStyles;
    int newStyleID;
    int removeStyleID;

    private void OnEnable()
    {
        targetObj = (CustomStyles)target;
        builtinStyles = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).customStyles.ToList();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(15f);

        GUILayout.BeginHorizontal();
        {
            newStyleID = EditorGUILayout.IntField(newStyleID);
            if (GUILayout.Button("Add New Style"))
            {
                targetObj.styles.Add(new GUIStyle(builtinStyles[newStyleID]));
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            removeStyleID = EditorGUILayout.IntField(removeStyleID);
            if (GUILayout.Button("Delete Style With ID"))
            {
                if (targetObj.styles.Count > removeStyleID)
                {
                    targetObj.styles.RemoveAt(removeStyleID);
                }
            }
        }
        GUILayout.EndHorizontal();
    }
}