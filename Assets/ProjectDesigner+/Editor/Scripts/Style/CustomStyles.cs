using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CustomStyles", menuName = "temp/CustomStyles")]
public class CustomStyles : ScriptableObject
{
    private static CustomStyles _instance;
    public static CustomStyles Instance
    {
        get
        {
            if (_instance == null)
            {
                var guid = AssetDatabase.FindAssets("t:CustomStyles")?.ElementAt(0);
                if (guid == null)
                    return null;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CustomStyles>(path);
                _instance = asset;
            }
            return _instance;
        }
    }

    public List<GUIStyle> styles = new List<GUIStyle>();
}