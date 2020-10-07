using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Designer
{
    public class Group
    {
        private static readonly float offset = 12f;

        public string name;

        public Rect position { get; private set; }
        public Rect screenPosition => new Rect(DesignerUtility.GetScreenPositionFromGridPoint(position.position), position.size * EditorData.zoomRatio);

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

        public void Draw()
        {
            //Debug.Log("Draw Group: " + screenPosition + "|" + position);
            RecalculatePosition();
            var headerPosition = new Rect(screenPosition.position - Vector2.up * 25f * EditorData.zoomRatio, new Vector2(screenPosition.width, 25f * EditorData.zoomRatio));
            Vector4 headerBorders = new Vector4(4f, 4f, 0f, 0f);
            Vector4 bodyBorders = new Vector4(0f, 0f, 4f, 4f);
            GUI.DrawTexture(headerPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.groupHeaderColor, Vector4.zero, headerBorders * EditorData.zoomRatio);
            name = GUI.TextField(headerPosition, name, CustomStyles.groupHeader);
            GUI.DrawTexture(screenPosition, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.groupColor, Vector4.zero, bodyBorders * EditorData.zoomRatio);
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