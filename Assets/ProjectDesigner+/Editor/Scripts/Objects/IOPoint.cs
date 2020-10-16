using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Designer
{
    [Serializable]
    public class IOPoint : GridElement
    {
        public override Type type => Type.IOPoint;

        public bool isInput { get; private set; }
        public bool enabled { get; set; }

        [SerializeReference]public Node parentNode;
        public override Rect position
        {
            get
            {
                Rect pos = new Rect(parentNode.position.position + Vector2.up * (parentNode.position.height / 2f - size.y / 2f) + Vector2.left * size.x, size);
                if (!isInput)
                {
                    pos.position += Vector2.right * (parentNode.position.width + size.x);
                }
                return pos;
            }
            internal set
            {

            }
        }

        private Vector2 size => Vector2.one * (enabled ? 18f : 0f);

        public Vector2 bezierStartPosition => screenPosition.center + (isInput ? new Vector2(-6f * EditorData.zoomRatio, 0f) : new Vector2(6f * EditorData.zoomRatio, 0f));


        public IOPoint(bool isInput, Node parentNode)
        {
            this.isInput = isInput;
            this.parentNode = parentNode;
            enabled = true;
        }

        public bool CanConnectTo(IOPoint other)
        {
            bool bothIsInput = isInput == other.isInput;
            bool thisAlreadyHasParent_And_OtherIsNotInterface_And_ParentIsClass = 
                isInput && parentNode.parentNode != null 
                && other.parentNode.nodeType != Node.NodeType.Interface 
                && parentNode.parentNode.nodeType == Node.NodeType.Class;

            bool otherAlreadyHasParent_And_ThisIsNotInterface_And_ParentIsClass = 
                other.isInput && other.parentNode.parentNode != null 
                && parentNode.nodeType != Node.NodeType.Interface
                && other.parentNode.parentNode.nodeType == Node.NodeType.Class;

            bool childIsInterfaceAndThisIsClass = parentNode.nodeType == Node.NodeType.Class && other.parentNode.nodeType == Node.NodeType.Interface;

            bool alreadyConnected = parentNode.childNodes.Contains(other.parentNode) || other.parentNode.childNodes.Contains(parentNode);

            bool finalResult = 
                !bothIsInput 
                && !thisAlreadyHasParent_And_OtherIsNotInterface_And_ParentIsClass 
                && !otherAlreadyHasParent_And_ThisIsNotInterface_And_ParentIsClass 
                && !childIsInterfaceAndThisIsClass 
                && !alreadyConnected;

            //Debug.Log(bothIsInput + "|" + thisAlreadyHasParent + "|" + otherAlreadyHasParent + "|||" + finalResult);
            return finalResult;
        }

        public override void Draw()
        {
            GUI.DrawTexture(screenPosition, DesignerUtility.EditorSettings.IOPointTexture, ScaleMode.StretchToFill, false, 0f, isHovered ? Color.cyan : DesignerUtility.EditorSettings.IOPointColor, 0f, size.x * EditorData.zoomRatio);
        }
    }
}
