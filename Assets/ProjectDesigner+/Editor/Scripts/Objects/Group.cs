using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Designer
{
    public class Group : GridElement
    {
        public override Type type => Type.Group;

        private static readonly float offset = 12f;
        static Vector4 headerBorderRadius = new Vector4(4f, 4f, 0f, 0f);
        static Vector4 bodyBorderRadius = new Vector4(0f, 0f, 4f, 4f);
        
        static Vector4 headerBorderWidth = new Vector4(1f, 1f, 1f, 0f) * 2f;
        static Vector4 bodyBorderWidth = new Vector4(1f, 1f, 1f, 1f) * 2f;

        private List<Node> _childNodes = new List<Node>();
        public IList<Node> childNodes
        {
            get
            {
                return _childNodes.AsReadOnly();
            }
        }

        public void AddNode(Node node)
        {
            if (!_childNodes.Contains(node))
            {
                _childNodes.Add(node);
            }
        }

        public void RemoveNode(Node node)
        {
            if (_childNodes.Contains(node))
            {
                _childNodes.Remove(node);
            }
        }

        public Rect headerPosition { get; private set; }

        public override void Draw()
        {
            //Debug.Log("Draw Group: " + screenPosition + "|" + position);
            RecalculatePosition();
            headerPosition = new Rect(screenPosition.position - Vector2.up * 25f * EditorData.zoomRatio,new Vector2(screenPosition.width, 25f * EditorData.zoomRatio));

            var headerTextPosition = new Rect(headerPosition);
            headerTextPosition.position += Vector2.right * screenPosition.width * 2f / 5f;
            headerTextPosition.width = screenPosition.width / 5f;

            GUI.DrawTexture(headerPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.groupHeaderColor, Vector4.zero, headerBorderRadius * EditorData.zoomRatio);
            GUI.DrawTexture(headerTextPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.groupHeaderTextFieldColor, 0f, 0f);
            name = GUI.TextField(headerTextPosition, name, StylePresets.groupHeader);
            GUI.DrawTexture(screenPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.groupColor, Vector4.zero, bodyBorderRadius * EditorData.zoomRatio);

            //Borders
            GUI.DrawTexture(headerPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, Color.black, headerBorderWidth * EditorData.zoomRatio, headerBorderRadius * EditorData.zoomRatio);
            GUI.DrawTexture(screenPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, Color.black, bodyBorderWidth * EditorData.zoomRatio, bodyBorderRadius * EditorData.zoomRatio);
        }

        /// <summary>
        /// Group's position is constantly changing due to node sizes and positions are not constant
        /// </summary>
        private void RecalculatePosition()
        {
            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxX = Mathf.NegativeInfinity;
            float maxY = Mathf.NegativeInfinity;

            foreach (var childNode in _childNodes)
            {
                if (minX > childNode.position.x)
                {
                    minX = childNode.position.x;
                }

                if (minY > childNode.position.y)
                {
                    minY = childNode.position.y;
                }

                if (maxX < (childNode.position.x + childNode.position.width))
                {
                    maxX = childNode.position.x + childNode.position.width;
                }

                if (maxY < (childNode.position.y + childNode.position.height))
                {
                    maxY = childNode.position.y + childNode.position.height;
                }
            }

            position = new Rect(minX - offset, minY - offset, maxX - minX + offset * 2, maxY - minY + offset * 2);
        }
    }
}