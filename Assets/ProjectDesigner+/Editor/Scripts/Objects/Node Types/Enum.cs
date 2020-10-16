using UnityEngine;
using System.Collections;

namespace Designer
{
    public partial class Node
    {
        [System.Serializable]
        public class Enum : Node
        {
            public override NodeType nodeType => NodeType.Enum;

            public Enum(Rect _pos,string _name) : base(_pos,_name)
            {
                input.enabled = false;
                output.enabled = false;
            }
        }
    }
}