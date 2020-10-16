using UnityEngine;
using System.Collections;

namespace Designer
{
    public partial class Node
    {
        [System.Serializable]
        public class Interface : Node
        {
            public override NodeType nodeType => NodeType.Interface;

            public Interface(Rect _pos, string _name) : base(_pos, _name)
            {

            }

            public override void Draw()
            {
                base.Draw();

                //Draw IO Points
                DrawIOPoints();
            }
        }
    }
}