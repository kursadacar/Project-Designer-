using UnityEngine;
using System.Collections;

namespace Designer
{
    public partial class Node
    {
        [System.Serializable]
        public class Class : Node
        {
            public override NodeType nodeType { get => NodeType.Class; }

            public Class(Rect _pos,string _name) : base(_pos,_name)
            {

            }

            public override void Draw()
            {
                base.Draw();

                //Inherit color
                InheritColor();

                //Draw IO Points
                DrawIOPoints();
            }
        }
    }
}