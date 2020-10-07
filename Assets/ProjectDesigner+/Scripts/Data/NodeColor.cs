using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "NodeColor", menuName ="Temp")]
public class NodeColor : ScriptableObject
{
    public new string name;
    public Color color;

    public static implicit operator Color(NodeColor d) => d.color;
}
