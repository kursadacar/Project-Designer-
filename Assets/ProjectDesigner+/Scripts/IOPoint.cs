using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Designer
{
    [Serializable]
    public class IOPoint
    {
        public bool isInput { get; private set; }
        [SerializeReference]public Node parentNode;
        public Rect rect 
        { 
            get
            {
                if (isInput)
                {
                    return new 
                        Rect(parentNode.screenPosition.position + new Vector2(-size.x * 1.5f * EditorData.zoomRatio,parentNode.screenPosition.height / 2 - size.y * EditorData.zoomRatio),
                        size * 2f * EditorData.zoomRatio);
                }
                else
                {
                    return new Rect(parentNode.screenPosition.position + new Vector2(-size.x * 0.5f * EditorData.zoomRatio + parentNode.screenPosition.width, parentNode.screenPosition.height / 2 - size.y* EditorData.zoomRatio), size * 2f * EditorData.zoomRatio);
                }
            }
        }

        private Vector2 size = new Vector2(8f, 8f);

        public Vector2 bezierStartPosition => rect.center + (isInput ? new Vector2(-6f * EditorData.zoomRatio, 0f) : new Vector2(6f * EditorData.zoomRatio, 0f));

        public IOPoint(bool isInput, Node parentNode)
        {
            this.isInput = isInput;
            this.parentNode = parentNode;
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

        public void Draw()
        {
            GUI.DrawTexture(rect, DesignerUtility.EditorSettings.IOPointTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.IOPointColor, 0f, size.x * EditorData.zoomRatio);
        }

        public void Draw(Color customColor)
        {
            GUI.DrawTexture(rect, DesignerUtility.EditorSettings.IOPointTexture, ScaleMode.StretchToFill, false, 0f, customColor, 0f, size.x * EditorData.zoomRatio);
        }
    }
}
