using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Security.Cryptography;

namespace Designer
{
    [System.Serializable]
    public abstract partial class Node
    {
        public bool selected;

        public List<NodeElement> childElements = new List<NodeElement>();

        private List<NodeElement> ChildsIncludingHeader
        {
            get
            {
                List<NodeElement> newList = new List<NodeElement>(childElements);
                newList.Insert(0, NodeElement.Header);
                return newList;
            }
        }

        [SerializeReference] public Node parentNode;
        [SerializeReference] public List<Node> childNodes = new List<Node>();
        [SerializeReference] public List<Connection> connections = new List<Connection>();

        public Group parentGroup { get; private set; }

        /// <summary>
        /// Get the root node. If there is no parent, returns itself.
        /// </summary>
        public Node rootNode
        {
            get
            {
                Node root = this;
                while (root.parentNode != null)
                {
                    root = root.parentNode;
                }
                return root;
            }
        }

        /// <summary>
        /// Scans recursively and returns all children and grandchildren. Includes itself
        /// </summary>
        public List<Node> AllChildren
        {
            get
            {
                var list = new List<Node>();
                list.Add(this);
                if (childNodes.Count < 0)
                    return null;
                foreach (var node in childNodes)
                {
                    list.AddRange(node.AllChildren);
                }
                return list;
            }
        }

        /// <summary>
        /// Returns list of all parenting nodes
        /// </summary>
        public List<Node> AllParents
        {
            get
            {
                var list = new List<Node>();

                var root = parentNode;
                while (root != null)
                {
                    list.Add(root);
                    root = root.parentNode;
                }

                return list;
            }
        }

        /// <summary>
        /// Returns all children of root node.
        /// </summary>
        public List<Node> AllRelatedNodes
        {
            get
            {
                return rootNode.AllChildren;
            }
        }

        /// <summary>
        /// Returns all nodes of the same inheritance (Does not include siblings)
        /// </summary>
        public List<Node> AllConnectedNodes
        {
            get
            {
                var list = new List<Node>();
                list.AddRange(AllChildren);
                list.AddRange(AllParents);
                return list;
            }
        }

        public enum NodeType : byte
        {
            Class,
            Interface,
            Enum
        }

        public string name { get; private set; }
        public abstract NodeType nodeType { get; }
        public Rect position { get; private set; }
        public Rect screenPosition => new Rect(DesignerUtility.GetScreenPositionFromGridPoint(position.position), position.size * EditorData.zoomRatio);

        public IOPoint input;
        public IOPoint output;

        private Rect colorSelectionRect;
        private Rect headerRect;
        private Rect contentRect;
        private Rect contentWithoutLabel;

        public Rect LabelPosition => new Rect(screenPosition.position + contentRect.position + headerRect.position, headerRect.size);

        private Color outlineColor;
        private NodeColor color;

        private static float contentOffset => 5f * EditorData.zoomRatio;

        private static readonly float rawNodeOffset = 3f;
        private static float nodeOffset => rawNodeOffset * EditorData.zoomRatio;

        private static float outlineWidth => 8f * EditorData.zoomRatio;
        private static readonly Vector2 outlineOffset = new Vector2(outlineWidth/2, outlineWidth/2);

        private Vector2 shadowOffset => Vector2.one * 10f * EditorData.zoomRatio;
        private Rect shadowRect => new Rect(screenPosition.position + shadowOffset, screenPosition.size);

        public Node(Rect _pos,string _name)
        {
            position = _pos;
            name = _name;
            input = new IOPoint(true,this);
            output = new IOPoint(false,this);
            color = DesignerUtility.EditorSettings.nodeColors.Where(x => x.name == "Dark").ElementAt(0);
        }

        public void Connect(Node node, Connection connection)
        {
            node.parentNode = this;
            childNodes.Add(node);
            connections.Add(connection);
            node.connections.Add(connection);
        }

        public void AddElement(NodeElement element)
        {
            if (!childElements.Contains(element))
            {
                childElements.Add(element);
            }
        }

        public void RemoveElement(NodeElement element)
        {
            if (childElements.Contains(element))
            {
                childElements.Remove(element);
            }
        }

        public void SetParentGroup(Group group)
        {
            if(parentGroup != null)
            {
                parentGroup.RemoveNode(this);
            }
            parentGroup = group;
            group?.AddNode(this);
        }

        public void SetName(string _name)
        {
            name = _name;
        }

        public void SetPosition(Rect _pos)
        {
            position = _pos;
        }

        public void Move(Vector2 movement)
        {
            position = new Rect(position.position + movement, position.size);
        }

        public void Select()
        {
            selected = true;
        }

        public void Deselect()
        {
            selected = false;
        }

        public string GetHeaderControlName()
        {
            return "node-header-" + Mathf.FloorToInt(position.position.x) + "|" + Mathf.FloorToInt(position.position.y);
        }

        public void Draw()
        {
            RecalculatePosition();

            //Inherit color
            InheritColor();

            //Draw Background
            DrawBox();

            //Draw Contents
            DrawContents();

            DrawIOPoints();
        }

        public void DrawInputActive()
        {
            RecalculatePosition();

            //Inherit color
            InheritColor();

            //Draw Background
            DrawBox();

            //Draw Contents
            DrawContents();

            if (nodeType == NodeType.Enum)
                return;
            input.Draw(Color.cyan);
            output.Draw();
        }

        public void DrawOutputActive()
        {
            RecalculatePosition();

            //Inherit color
            InheritColor();

            //Draw Background
            DrawBox();

            //Draw Contents
            DrawContents();

            if (nodeType == NodeType.Enum)
                return;
            input.Draw();
            output.Draw(Color.cyan);
        }

        private void RecalculatePosition()
        {
            var targetSize = new Vector2(DesignerUtility.EditorSettings.nodeWidth, (DesignerUtility.EditorSettings.GetRawNodeElementHeight() + rawNodeOffset) * (ChildsIncludingHeader.Count + 2) + 10f);
            if (selected)
                targetSize = targetSize * 1.05f;
            position = new Rect(position.position, Vector2.Lerp(position.size, targetSize, EditorData.deltaTime * 10f));
        }

        private void InheritColor()
        {
            if (rootNode != null)
            {
                color = rootNode.color;
            }
        }

        private void ChangeColor(object value)
        {
            var newColor = (NodeColor)value;
            color = newColor;
        }

        private void DrawContents()
        {
            GUI.BeginGroup(screenPosition);
            {
                contentRect = new Rect(new Vector2(contentOffset, contentOffset), screenPosition.size - new Vector2(contentOffset * 2, contentOffset * 2));

                //color = EditorGUILayout.ColorField(color);

                GUI.BeginGroup(contentRect);
                {
                    colorSelectionRect = new Rect(3f * EditorData.zoomRatio, 0f, contentRect.width / 5f - (3f * EditorData.zoomRatio * 2f), DesignerUtility.EditorSettings.NodeElementHeight);
                    headerRect = new Rect(contentRect.width / 5f, 0f, contentRect.width / 5f * 3f, DesignerUtility.EditorSettings.NodeElementHeight);
                    contentWithoutLabel = new Rect(2f, DesignerUtility.EditorSettings.NodeElementHeight + 2f, contentRect.width - 4f, contentRect.height - DesignerUtility.EditorSettings.NodeElementHeight - 4f);

                    GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, Color.gray, 0f, 3f * EditorData.zoomRatio);
                    //GUI.SetNextControlName(GetHeaderControlName());
                    name = GUI.TextField(headerRect, name, CustomStyles.nodeHeader);

                    if (parentNode == null)
                    {
                        if (GUI.Button(colorSelectionRect, ""))
                        {

                            GenericMenu menu = new GenericMenu();
                            foreach(var color in DesignerUtility.EditorSettings.nodeColors)
                            {
                                object data = color;
                                menu.AddItem(new GUIContent(color.name), this.color == color, this.ChangeColor, data);
                            }
                            menu.ShowAsContext();
                            Event.current.Use();
                        }
                        GUI.DrawTexture(colorSelectionRect, DesignerUtility.EditorSettings.colorWheelTexture, ScaleMode.ScaleToFit);
                    }

                    if (EditorData.zoomRatio >= 1f)
                    {
                        DrawElements(contentWithoutLabel);

                        Rect buttonRect = new Rect(0f, (DesignerUtility.EditorSettings.NodeElementHeight + nodeOffset) * (ChildsIncludingHeader.Count + 1), contentRect.width, DesignerUtility.EditorSettings.NodeElementHeight);
                        if (GUI.Button(buttonRect, "+", CustomStyles.button))
                        {
                            var element = new NodeElement.Field();
                            AddElement(element);
                        }
                    }
                    else
                    {
                        var rect = new Rect(0f, DesignerUtility.EditorSettings.NodeElementHeight, contentRect.width, contentRect.height);
                        GUI.Label(rect, ". . .", CustomStyles.GetCustomLabel(Color.white, 24, TextAnchor.MiddleCenter));
                    }
                }
                //content rect
                GUI.EndGroup();
            }
            //screenposition
            GUI.EndGroup();
        }

        private void DrawIOPoints()
        {
            if (nodeType == NodeType.Enum)
                return;
            input.Draw();
            output.Draw();
        }

        private void DrawBox()
        {
            var outlineRect = new Rect(screenPosition.position - outlineOffset, screenPosition.size + outlineOffset * 2f);
            outlineColor = Color.Lerp(outlineColor, selected ? Color.cyan : Color.clear, EditorData.deltaTime * 10f);
            GUI.DrawTexture(outlineRect, DesignerUtility.EditorSettings.nodeTexture, ScaleMode.StretchToFill, false, 0f, outlineColor, outlineWidth, 5f * EditorData.zoomRatio);

            GUI.DrawTexture(shadowRect, DesignerUtility.EditorSettings.nodeTexture, ScaleMode.StretchToFill, false, 0f, DesignerUtility.EditorSettings.nodeShadowColor, 0f, 3f);
            GUI.DrawTexture(screenPosition, DesignerUtility.EditorSettings.nodeTexture,ScaleMode.StretchToFill,false,0f, color, 0f,5f * EditorData.zoomRatio);
        }

        private void DrawElements(Rect position)
        {
            Rect curContentPos;

            curContentPos = new Rect(position.x, position.y, position.width, DesignerUtility.EditorSettings.NodeElementHeight - 2f);
            GUI.DrawTexture(curContentPos, DesignerUtility.EditorSettings.nodeTexture, ScaleMode.StretchToFill, false, 0f, new Color(0.8f,0.8f,0.8f,1f), 0f, 3f * EditorData.zoomRatio);
            NodeElement.Header.Draw(curContentPos);

            var curIndex = 1;
            foreach(var content in childElements)
            {
                curContentPos = new Rect(position.x, position.y + curIndex * (DesignerUtility.EditorSettings.NodeElementHeight + nodeOffset), position.width, DesignerUtility.EditorSettings.NodeElementHeight - 2f);
                content.Draw(curContentPos);
                curIndex++;
            }
        }
    }
}