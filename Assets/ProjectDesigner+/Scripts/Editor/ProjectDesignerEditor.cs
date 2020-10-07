﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Runtime.InteropServices;

namespace Designer
{
    [Serializable]
    public class ProjectDesignerEditor : EditorWindow
    {
        private List<Node> activeNodes = new List<Node>();
        private List<Node> selectedNodes = new List<Node>();
        private List<Node> grabbedNodes = new List<Node>();
        private List<Node> visibleNodes = new List<Node>();

        private List<Connection> connections = new List<Connection>();

        private List<Group> activeGroups = new List<Group>();

        private Rect selectionRect;

        float gridSize = 100;

        //private Vector2 gridOffset;

        int verticalWhiteLineBegin;
        int horizontalWhiteLineBegin;

        float optionsPanelWidth = 250f;

        bool mouseDragged = false;
        bool grabbedOptionsPanel;
        bool optionsPanelEnabled = true;
        OptionsPanelData optionsPanelData;
        EditorSettings settings;
        Rect viewport;

        IOPoint selectedIOPoint;

        private Connection hoveredConnection;
        private IOPoint hoveredIOPoint;

        private Node activeClickedNode;
        private List<Vector2> grabbedNodesInitialPosition = new List<Vector2>();
        private List<Vector2> gridPoints = new List<Vector2>();

        #region Obligatory
        [MenuItem("ProjectDesigner+/ProjectDesigner+")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ProjectDesignerEditor));
            EditorData.SetZoomRatio(1f);
        }
        #endregion

        private void OnEnable()
        {
            optionsPanelData = new OptionsPanelData(true);
            settings = DesignerUtility.EditorSettings;
        }

        void OnGUI()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            wantsMouseMove = true;
            viewport = new Rect(DesignerUtility.GetGridPositionFromScreenPoint(new Vector2(0f,0f)), position.size / EditorData.zoomRatio);
            EditorData.windowRect = position;

            DrawGrid();
            DrawGroups();
            DrawConnections();
            DrawHoveringBezier();
            DrawNodes();
            DrawSelectionRect();
            DrawOptionsPanel(optionsPanelWidth, ref optionsPanelData);

            HandleEvents(Event.current);

            Repaint();

            stopWatch.Stop();
            //Set EditorData values
            EditorData.SetDeltaTime((float)(stopWatch.ElapsedMilliseconds) / 100f);
            EditorData.SetMousePosition(Event.current.mousePosition);
            //Debug.Log(EditorData.deltaTime);
        }

        #region Utilities

        #region Connections
        private void ConnectNodes(Node from, Node to)
        {
            //Enums can not connect with other nodes
            if (from.nodeType == Node.NodeType.Enum || to.nodeType == Node.NodeType.Enum)
                return;

            Connection connection = null;

            //Interfaces can only make interface type connections
            if (from.nodeType == Node.NodeType.Interface)
            {
                connection = new Connection(from, to, Connection.ConnectionType.Interface);
            }

            if (from.nodeType == Node.NodeType.Class)
            {
                //Classes cant parent to interfaces
                if (to.nodeType == Node.NodeType.Interface)
                {
                    return;
                }

                //If target node already has parent class, can not connect. Also cant connect to any related nodes.
                if (to.parentNode != null && to.parentNode.nodeType == Node.NodeType.Class || from.AllRelatedNodes.Contains(to))
                {
                    return;
                }
                else
                {
                    connection = new Connection(from, to, Connection.ConnectionType.Inheritance);
                }
            }


            connections.Add(connection);
            from.Connect(to, connection);

        }

        private void DrawConnections()
        {
            foreach (var connection in connections)
            {
                if(hoveredConnection!= null && hoveredConnection == connection)
                {
                    connection.Draw(Color.cyan);
                    continue;
                }
                connection.Draw();
            }
        }

        private void DeleteConnection(Connection con)
        {
            if (connections.Contains(con))
            {
                connections.Remove(con);
                con.fromNode.connections.Remove(con);
                con.fromNode.childNodes.Remove(con.toNode);

                con.toNode.parentNode = null;
                con.toNode.connections.Remove(con);
            }
        }
        #endregion


        #region Events

        private void HandleEvents(Event currentEvent)
        {
            #region Utility
            void ClearSelectionRect()
            {
                selectionRect.position = new Vector2(0f, 0f);
                selectionRect.size = new Vector2(0f, 0f);
            }

            void LoseFocus()
            {
                //if(currentlyEditedNode != null)
                //{
                //    Debug.Log("Change name of : " + currentlyEditedNode);
                //    currentlyEditedNode.SetName(Regex.Replace(currentlyEditedNode.name, @"\s+", ""));
                //    currentlyEditedNode = null;
                //}
                GUI.FocusControl(null);
            }
            #endregion

            void HandleGridEvents(Vector2 mousePosition, Event _event)
            {
                #region Utility
                void ReleaseBezier()
                {
                    _event.Use();
                    selectedIOPoint = null;
                }

                void RightClickOnNode(Node node)
                {
                    //Selected more than one node
                    if (selectedNodes.Count > 1)
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete Nodes"), false, DeleteSelectedNodes);

                        menu.AddItem(new GUIContent("Group Nodes"), false, GroupSelectedNodes);

                        if(selectedNodes.Where(x=> x.parentGroup != null).Count() > 0)
                        {
                            menu.AddItem(new GUIContent("Remove From Group"), false, UngroupSelectedNodes);
                        }

                        menu.ShowAsContext();
                    }
                    else
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete Node"), false, DeleteNode, node);

                        if(node.parentGroup != null)
                        {
                            menu.AddItem(new GUIContent("Remove Node From Group"), false, UngroupNode, node);
                        }

                        menu.ShowAsContext();
                    }
                }

                void RightClickOnGroup(Group group)
                {
                    GenericMenu menu = new GenericMenu();

                    object data = group;
                    menu.AddItem(new GUIContent("Delete Group"), false, DeleteGroup, data);

                    menu.ShowAsContext();
                    _event.Use();
                }

                void SelectNode(Node node)
                {
                    node.Select();
                    if (!selectedNodes.Contains(node))
                    {
                        selectedNodes.Add(node);
                    }
                }

                void DeselectNode(Node node)
                {
                    node.Deselect();
                    if (selectedNodes.Contains(node))
                    {
                        selectedNodes.Remove(node);
                    }
                }

                void DeselectAllNodes()
                {
                    foreach (var node in selectedNodes)
                    {
                        node.Deselect();
                    }
                    selectedNodes.Clear();
                }

                void RightClickOnGrid()
                {
                    var menu = new GenericMenu();

                    object[] data = new object[2];
                    data[0] = mousePosition;
                    data[1] = Node.NodeType.Class;

                    menu.AddItem(new GUIContent("Create Class"), false, CreateNewNode, data);

                    data = new object[2];
                    data[0] = mousePosition;
                    data[1] = Node.NodeType.Interface;
                    menu.AddItem(new GUIContent("Create Interface"), false, CreateNewNode, data);

                    data = new object[2];
                    data[0] = mousePosition;
                    data[1] = Node.NodeType.Enum;
                    menu.AddItem(new GUIContent("Create Enum"), false, CreateNewNode, data);

                    menu.ShowAsContext();
                    _event.Use();
                }

                void LeftClickOnGrid()
                {
                    selectionRect.position = mousePosition;
                    DeselectAllNodes();
                    LoseFocus();
                }

                void DrawSelectionRect()
                {
                    selectionRect.size = mousePosition - selectionRect.position;
                    foreach (var node in visibleNodes)
                    {
                        if (selectionRect.ContainsArea(node.screenPosition))
                        {
                            SelectNode(node);
                        }
                        else
                        {
                            DeselectNode(node);
                        }
                    }
                }

                void DropBezierOnGrid()
                {
                    var menu = new GenericMenu();

                    var data = new object[2];
                    data[0] = mousePosition;
                    data[1] = Node.NodeType.Class;
                    menu.AddItem(new GUIContent("Create New Class"), false, CreateNodeAndConnect, data);

                    //data = new object[2];
                    //data[0] = mousePosition;
                    //data[1] = Node.NodeType.Enum;
                    //menu.AddItem(new GUIContent("Create New Enum"), false, CreateNodeAndConnect, data);

                    //data = new object[2];
                    //data[0] = mousePosition;
                    //data[1] = Node.NodeType.Interface;
                    //menu.AddItem(new GUIContent("Create New Interface"), false, CreateNodeAndConnect, data);


                    menu.AddItem(new GUIContent("Cancel"), false, ReleaseBezier);

                    menu.ShowAsContext();
                    _event.Use();
                }

                void CheckHoveredIOPoints()
                {
                    hoveredIOPoint = null;
                    foreach (var node in visibleNodes)
                    {
                        if (node.input.rect.Contains(mousePosition))
                        {
                            hoveredIOPoint = node.input;
                            break;
                        }
                        if (node.output.rect.Contains(mousePosition))
                        {
                            hoveredIOPoint = node.output;
                            break;
                        }
                    }
                }

                void CheckHoveredConnections()
                {
                    hoveredConnection = null;
                    foreach (var con in connections)
                    {
                        if (con.boundingRect.Contains(mousePosition, true))
                        {
                            var dist = HandleUtility.DistancePointBezier(mousePosition, con.fromPosition, con.toPosition, con.fromTangent, con.toTangent);
                            if (dist < DesignerUtility.EditorSettings.connectionWidth * EditorData.zoomRatio * 1.3f)
                            {
                                hoveredConnection = con;
                                break;
                            }
                        }
                    }
                }

                Vector2 GetNearestGridPoint(Vector2 point)
                {
                    Vector2 nearestGridPoint = new Vector2();
                    var minDist = Mathf.Infinity;
                    foreach (var gridPoint in gridPoints)
                    {
                        var dist = Vector2.Distance(gridPoint, point);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearestGridPoint = gridPoint;
                        }
                    }
                    return nearestGridPoint;
                }
                #endregion

                //Debug.Log(_event.type);

                //Mouse Move
                if(_event.type == EventType.MouseMove)
                {
                    //Check if hovering connections
                    CheckHoveredConnections();

                    //Check if hovering io points
                    CheckHoveredIOPoints();
                }

                //Mouse Down
                if (_event.type == EventType.MouseDown)
                {
                    //Left click
                    if (_event.button == 0)
                    {
                        //Clicked on a node or IO Point?
                        foreach (var node in visibleNodes)
                        {
                            if (node.input.rect.Contains(mousePosition))
                            {
                                selectedIOPoint = node.input;
                                return;
                            }
                            if (node.output.rect.Contains(mousePosition))
                            {
                                selectedIOPoint = node.output;
                                return;
                            }

                            if (node.screenPosition.Contains(mousePosition))
                            {
                                if (!_event.control)
                                {
                                    DeselectAllNodes();
                                    if (!node.selected)
                                    {
                                        SelectNode(node);
                                    }
                                }
                                else
                                {
                                    SelectNode(node);
                                }

                                LoseFocus();

                                //grab all selected nodes
                                grabbedNodes.AddRange(selectedNodes);

                                //make node go to end of list, to draw it latest, so it appears on top.
                                activeNodes.Remove(node);
                                activeNodes.Add(node);

                                //set active clicked node
                                activeClickedNode = node;
                                //record grabbed nodes positions
                                grabbedNodesInitialPosition.Clear();
                                foreach (var grabbedNode in grabbedNodes)
                                {
                                    grabbedNodesInitialPosition.Add(grabbedNode.position.position);
                                }
                                
                                return;
                            }
                        }

                        //Clicked on a connection?
                        if(hoveredConnection != null)
                        {
                            DeleteConnection(hoveredConnection);
                            if(Vector2.Distance(hoveredConnection.fromPosition,mousePosition) < Vector2.Distance(hoveredConnection.toPosition, mousePosition))
                            {
                                selectedIOPoint = hoveredConnection.toPoint;
                            }
                            else
                            {
                                selectedIOPoint = hoveredConnection.fromPoint;
                            }
                            return;
                        }

                        //Clicked on grid?
                        LeftClickOnGrid();
                    }

                    //Middle Click
                    if(_event.button == 2)
                    {
                        ClearSelectionRect();
                    }
                }

                //Mouse Drag
                if (_event.type == EventType.MouseDrag)
                {
                    //Middle Mouse Drag
                    if (_event.button == 2)
                    {
                        //Control is pressed
                        if (_event.control)
                        {
                            ZoomIn(_event.delta.y);
                            return;
                        }
                        EditorData.offset += _event.delta;
                    }
                    //Left mouse drag
                    if (_event.button == 0)
                    {
                        //Move grabbed nodes
                        if (grabbedNodes.Count > 0)
                        {
                            if (_event.shift)
                            {

                                Vector2 offset = DesignerUtility.GetGridPositionFromScreenPoint(GetNearestGridPoint(mousePosition)) - activeClickedNode.position.position;

                                foreach(var node in grabbedNodes)
                                {
                                    node.Move(offset);
                                }
                            }
                            else
                            {
                                foreach (var node in grabbedNodes)
                                {
                                    node.Move(_event.delta / EditorData.zoomRatio);
                                }
                            }
                            return;
                        }

                        //Selection rect
                        if (selectionRect.position.magnitude > 0.01f)
                        {
                            DrawSelectionRect();
                        }

                        if(selectedIOPoint != null)
                        {
                            //Check if hovering io points
                            CheckHoveredIOPoints();
                        }
                    }
                }

                //Right click
                if (_event.type == EventType.ContextClick)
                {
                    //Clicked on node
                    foreach (var node in visibleNodes)
                    {
                        if (node.screenPosition.Contains(mousePosition))
                        {
                            //if clicked on a node   
                            RightClickOnNode(node);
                            return;
                        }
                    }

                    //Clicked on group
                    foreach(var group in activeGroups)
                    {
                        if (group.screenPosition.Contains(mousePosition))
                        {
                            RightClickOnGroup(group);
                            return;
                        }
                    }
                    //If didnt return, clicked on empty space
                    RightClickOnGrid();
                }

                //Scroll
                if (_event.type == EventType.ScrollWheel)
                {
                    ZoomIn(_event.delta.y * 5f);
                }

                //Mouse up
                if (_event.type == EventType.MouseUp)
                {
                    activeClickedNode = null;
                    //Dragging a bezier from io point
                    if (selectedIOPoint != null)
                    {
                        //Check if connecting to a node
                        foreach (var node in visibleNodes)
                        {
                            if (selectedIOPoint.isInput)
                            {
                                if (node.output.rect.Contains(mousePosition))
                                {
                                    ConnectNodes(node, selectedIOPoint.parentNode);
                                    return;
                                }
                            }
                            else
                            {
                                if (node.input.rect.Contains(mousePosition))
                                {
                                    ConnectNodes(selectedIOPoint.parentNode, node);
                                    return;
                                }
                            }
                        }
                        //Did not connect to a node, bring context menu
                        DropBezierOnGrid();
                    }
                }

                //Keyboard keys
                if (_event.type == EventType.KeyDown)
                {
                    if (_event.keyCode == KeyCode.F)
                    {
                        if (selectedNodes.Count > 0)
                        {
                            Vector2 midPoint = new Vector2();
                            foreach (var node in selectedNodes)
                            {
                                midPoint += node.position.center;
                            }
                            midPoint = midPoint * 1 / selectedNodes.Count;

                            DesignerUtility.CenterViewToPosition(midPoint);
                        }
                        else
                        {
                            SetZoom(100f);
                        }
                    }
                    //_event.Use();
                }
            }

            void HandleGeneralEvents(Vector2 mousePosition, Event _event)
            {
                Rect optionsPanelHandleRect = new Rect(optionsPanelWidth - 2f, 0f, 4f, position.height);
                EditorGUIUtility.AddCursorRect(optionsPanelHandleRect, MouseCursor.ResizeHorizontal);

                //Mouse Down
                if (_event.type == EventType.MouseDown)
                {
                    if (optionsPanelHandleRect.Contains(mousePosition))
                    {
                        grabbedOptionsPanel = true;
                        ClearSelectionRect();
                    }
                }

                //Mouse Drag
                if (_event.type == EventType.MouseDrag)
                {
                    mouseDragged = true;
                    //Left mouse drag
                    if (_event.button == 0)
                    {
                        //Resize options panel
                        if (grabbedOptionsPanel)
                        {
                            optionsPanelWidth = mousePosition.x;
                        }
                    }
                }

                //Mouse Up
                if (_event.type == EventType.MouseUp)
                {
                    grabbedOptionsPanel = false;
                    mouseDragged = false;
                    selectedIOPoint = null;

                    //Release all grabbed nodes
                    grabbedNodes.Clear();

                    //Clear selection rect
                    ClearSelectionRect();
                }

                //Key Down
                if(_event.type == EventType.KeyDown)
                {
                    if (_event.keyCode == KeyCode.None)
                    {
                        LoseFocus();
                    }
                }
            }

            Rect gridRect = new Rect(optionsPanelWidth, 0f, position.width - optionsPanelWidth, position.height);
            if (gridRect.Contains(currentEvent.mousePosition))
            {
                HandleGridEvents(currentEvent.mousePosition, currentEvent);
            }
            HandleGeneralEvents(currentEvent.mousePosition, currentEvent);
        }

        #endregion

        #region Editor - Grid

        private void ResetEditorSettings()
        {
            var oldNodeTexture = settings.nodeTexture;
            settings = DesignerUtility.ResetEditorSettings();
            settings.nodeTexture = oldNodeTexture;
        }

        /// <summary>
        /// Actually changes grid size and adjusts offset to make a zoom effect
        /// </summary>
        /// <param name="zoomDelta"></param>
        private void ZoomIn(float zoomDelta)
        {
            Vector2 oldCenter = DesignerUtility.GetGridPositionFromScreenPoint(position.size / 2);

            gridSize -= zoomDelta;
            //Clamp
            if (gridSize < settings.minGridSize)
                gridSize = settings.minGridSize;
            if (gridSize > settings.maxGridSize)
                gridSize = settings.maxGridSize;

            EditorData.SetZoomRatio(gridSize / 100f);
            DesignerUtility.CenterViewToPosition(oldCenter);
            Event.current.Use();
        }

        private void SetZoom(float zoom)
        {
            Vector2 oldCenter = DesignerUtility.GetGridPositionFromScreenPoint(position.size / 2);

            gridSize = zoom;
            //Clamp
            if (gridSize < settings.minGridSize)
                gridSize = settings.minGridSize;
            if (gridSize > settings.maxGridSize)
                gridSize = settings.maxGridSize;

            EditorData.SetZoomRatio(gridSize / 100f);
            DesignerUtility.CenterViewToPosition(oldCenter);
        }

        private void DrawGrid()
        {
            int horizontalLineCount = Mathf.FloorToInt(position.height / gridSize) + 2;
            int verticalLineCount = Mathf.FloorToInt(position.width / gridSize) + 2;
            Vector2 beginPos, endPos;
            Vector2 activeOffset = new Vector2(EditorData.offset.x % gridSize, EditorData.offset.y % gridSize);
            Vector2 origin = new Vector2(-gridSize, -gridSize) + activeOffset;

            verticalWhiteLineBegin = Mathf.FloorToInt(EditorData.offset.x / gridSize + 1) % 10;
            horizontalWhiteLineBegin = Mathf.FloorToInt(EditorData.offset.y / gridSize + 1) % 10;

            if (verticalWhiteLineBegin < 0)
                verticalWhiteLineBegin = verticalWhiteLineBegin + 10;
            if (horizontalWhiteLineBegin < 0)
                horizontalWhiteLineBegin = horizontalWhiteLineBegin + 10;

            gridPoints.Clear();

            Handles.BeginGUI();
            Color oldColor = Handles.color;

            Handles.color = settings.gridLineColor;

            float[] xPositions = new float[verticalLineCount];
            for (int i = 0; i < verticalLineCount; i++)
            {
                beginPos = origin + Vector2.right * gridSize * i;
                endPos = origin + new Vector2(0f, position.height + 2 * gridSize) + Vector2.right * gridSize * i;
                if (i % 10 == verticalWhiteLineBegin)
                {
                    Handles.color = settings.gridHighlightColor;
                    Handles.DrawAAPolyLine(2f, beginPos, endPos);
                    Handles.color = settings.gridLineColor;
                }
                else
                {
                    Handles.DrawLine(beginPos, endPos);
                }
                xPositions[i] = beginPos.x;
            }

            float[] yPositions = new float[horizontalLineCount];
            for (int i = 0; i < horizontalLineCount; i++)
            {
                beginPos = origin + Vector2.up * gridSize * i;
                endPos = origin + new Vector2(position.width + 2 * gridSize, 0f) + Vector2.up * gridSize * i;
                if (i % 10 == horizontalWhiteLineBegin)
                {
                    Handles.color = settings.gridHighlightColor;
                    Handles.DrawAAPolyLine(2f, beginPos, endPos);
                    Handles.color = settings.gridLineColor;
                }
                else
                {
                    Handles.DrawLine(beginPos, endPos);
                }
                yPositions[i] = beginPos.y;
            }

            foreach(var xPos in xPositions)
            {
                foreach(var yPos in yPositions)
                {
                    gridPoints.Add(new Vector2(xPos, yPos));
                    //GUI.DrawTexture(new Rect(xPos, yPos, 1f, 1f), Texture2D.whiteTexture);
                }
            }

            Handles.color = oldColor;
            Handles.EndGUI();
        }
        #endregion

        #region Draw Functions
        private void DrawOptionsPanel(float width, ref OptionsPanelData pSettings)
        {
            #region Utility
            void DrawDebugUI()
            {
                GUIStyle elementStyle = new GUIStyle(EditorStyles.helpBox);
                elementStyle.alignment = TextAnchor.MiddleLeft;
                elementStyle.wordWrap = true;
                GUILayout.Box("Mouse Position : " + Event.current.mousePosition.ToString(), elementStyle);
                GUILayout.Box("Mouse Position in Grid : " + DesignerUtility.GetGridPositionFromScreenPoint(Event.current.mousePosition).ToString(), elementStyle);
                GUILayout.Box("Window Rect : " + position.ToString(), elementStyle);
                GUILayout.Box("Offset : " + EditorData.offset.ToString(), elementStyle);
                GUILayout.Box("Grid Size : " + gridSize.ToString(), elementStyle);
                GUILayout.Box("Center Pos : " + DesignerUtility.GetGridPositionFromScreenPoint(position.size / 2).ToString(), elementStyle);
                GUILayout.Box("Viewport Center : " + viewport.center, elementStyle);
                GUILayout.Box("Vertical White Line Begin : " + verticalWhiteLineBegin.ToString(), elementStyle);
                GUILayout.Box("Horizontal White Line Begin : " + horizontalWhiteLineBegin.ToString(), elementStyle);
                GUILayout.Box("Mouse Screen Position (Calculated from Grid Position) : " +
                    DesignerUtility.GetScreenPositionFromGridPoint(DesignerUtility.GetGridPositionFromScreenPoint(Event.current.mousePosition)),
                        elementStyle);
                GUILayout.Box("Viewport : " + viewport.ToString(), elementStyle);
                GUILayout.Box("Selection Rect : " + selectionRect.ToString(), elementStyle);
            }

            void DrawNodeList()
            {
                Node clickNode = null;
                foreach (var node in activeNodes)
                {
                    //string buttonText = "Node Name : " + node.name
                    //+ "\n Node Position : " + node.position
                    //+ "\n Visible Position : " + node.screenPosition
                    //+ "\n Center : " + node.position.center;
                    if (GUILayout.Button(node.name))
                    {
                        clickNode = node;
                        break;
                    }
                }
                if (clickNode != null)
                {
                    DesignerUtility.CenterViewToPosition(clickNode.position.center);
                }
            }

            void DrawGroupList()
            {
                foreach(var group in activeGroups)
                {
                    if (GUILayout.Button(group.name))
                    {
                        DesignerUtility.CenterViewToPosition(group.position.center);
                    }
                }
            }

            void DrawEditorSettings()
            {
                settings.gridLineColor = EditorGUILayout.ColorField("Grid Line Color", settings.gridLineColor);
                settings.gridHighlightColor = EditorGUILayout.ColorField("Grid Highlight Color", settings.gridHighlightColor);
                settings.optionsPanelColor = EditorGUILayout.ColorField("Options Panel Color", settings.optionsPanelColor);
                settings.groupColor = EditorGUILayout.ColorField("Group Color", settings.groupColor);
                settings.maxGridSize = EditorGUILayout.FloatField("Max Grid Size", settings.maxGridSize);
                settings.minGridSize = EditorGUILayout.FloatField("Min Grid Size", settings.minGridSize);
                settings.SetRawNodeElementHeight(EditorGUILayout.FloatField("Node Element Height", settings.GetRawNodeElementHeight()));
                settings.nodeTexture = (Texture2D)EditorGUILayout.ObjectField("Node Texture", settings.nodeTexture, typeof(Texture2D), false);
            }
            #endregion

            Rect panelArea = new Rect(0f, 0f, width, position.height);
            GUI.DrawTexture(panelArea, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, settings.optionsPanelColor, 0f, 0f);
            GUILayout.BeginArea(panelArea);
            GUILayout.Label("Options Panel");
            optionsPanelData.scrollPosition = GUILayout.BeginScrollView(pSettings.scrollPosition);

            pSettings.showDebugWindow = EditorGUILayout.Foldout(optionsPanelData.showDebugWindow, "Debug");
            if (pSettings.showDebugWindow)
            {
                DrawDebugUI();
            }
            pSettings.showNodes = EditorGUILayout.Foldout(pSettings.showNodes, "Show Nodes");
            if (pSettings.showNodes)
            {
                DrawNodeList();
            }
            pSettings.showGroups = EditorGUILayout.Foldout(pSettings.showGroups, "Show Groups");
            if (pSettings.showGroups)
            {
                DrawGroupList();
            }
            pSettings.showEditorSettings = EditorGUILayout.Foldout(pSettings.showEditorSettings, "Show Editor Settings");
            if (pSettings.showEditorSettings)
            {
                if (GUILayout.Button("Reset Settings to Default"))
                {
                    ResetEditorSettings();
                }
                DrawEditorSettings();
            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (GUI.Button(new Rect(panelArea.width, 25f, 25f, 25f), optionsPanelEnabled ? "-" : "+"))
            {
                if (optionsPanelEnabled)
                {
                    pSettings.lastWidth = optionsPanelWidth;
                    optionsPanelWidth = 0f;
                    optionsPanelEnabled = false;
                }
                else
                {
                    optionsPanelWidth = pSettings.lastWidth;
                    optionsPanelEnabled = true;
                }
            }
        }

        private void DrawNodes()
        {
            visibleNodes.Clear();

            for (int i = 0; i < activeNodes.Count; i++)
            {
                if (viewport.ContainsArea(activeNodes[i].position))
                {
                    if (hoveredIOPoint == activeNodes[i].input)
                    {
                        if (selectedIOPoint != null)
                        {
                            if (selectedIOPoint.CanConnectTo(activeNodes[i].input))
                            {
                                activeNodes[i].DrawInputActive();
                            }
                            else
                            {
                                activeNodes[i].Draw();
                            }
                        }
                        else
                        {
                            activeNodes[i].DrawInputActive();
                        }
                    }
                    else if (hoveredIOPoint == activeNodes[i].output)
                    {
                        if (selectedIOPoint != null)
                        {
                            if (selectedIOPoint.CanConnectTo(activeNodes[i].output))
                            {
                                activeNodes[i].DrawOutputActive();
                            }
                            else
                            {
                                activeNodes[i].Draw();
                            }
                        }
                        else
                        {
                            activeNodes[i].DrawOutputActive();
                        }
                    }
                    else
                    {
                        activeNodes[i].Draw();
                    }
                    visibleNodes.Add(activeNodes[i]);
                }
            }
        }

        private void DrawGroups()
        {
            foreach(var group in activeGroups)
            {
                group.Draw();
            }
        }

        private void DrawHoveringBezier()
        {
            if (selectedIOPoint != null)
            {
                var io_position = selectedIOPoint.bezierStartPosition;
                var tangent = selectedIOPoint.isInput ? (settings.BezierTangent * EditorData.zoomRatio * -1f) : (settings.BezierTangent * EditorData.zoomRatio);
                Handles.DrawBezier(io_position, Event.current.mousePosition, io_position + tangent, Event.current.mousePosition - tangent, settings.inheritanceConnectionColor, settings.lineTexture, DesignerUtility.EditorSettings.connectionWidth * EditorData.zoomRatio);
            }
        }

        private void DrawSelectionRect()
        {
            GUI.DrawTexture(selectionRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, settings.selectionRectColor, 0f, 0f);
        }
        #endregion

        #region Nodes

        /// <summary>
        /// Creates a new node at mouse position and connects it with selectedIOPoint.parentNode
        /// </summary>
        /// <param name="type"></param>
        private void CreateNodeAndConnect(object _data)
        {
            var data = (object[])_data;
            var _pos = (Vector2)data[0];
            var _type = (Node.NodeType)data[1];
            _pos = DesignerUtility.GetGridPositionFromScreenPoint(_pos);

            Node newNode = null;
            switch (_type)
            {
                case Node.NodeType.Class:
                    {
                        newNode = new Node.Class(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Class");
                    }
                    break;
                case Node.NodeType.Enum:
                    {
                        newNode = new Node.Enum(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Enum");
                    }
                    break;
                case Node.NodeType.Interface:
                    {
                        newNode = new Node.Interface(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Interface");
                    }
                    break;
            }

            string originalName = newNode.name;
            int iteration = 1;
            while (activeNodes.Where(x => x.name == newNode.name).Count() != 0)
            {
                newNode.SetName(originalName + "(" + iteration + ")");
                iteration++;
            }

            activeNodes.Add(newNode);

            if (selectedIOPoint.isInput)
            {
                ConnectNodes(newNode, selectedIOPoint.parentNode);
            }
            else
            {
                ConnectNodes(selectedIOPoint.parentNode, newNode);
            }

            selectedIOPoint = null;
        }
        private void CreateNewNode(object recData)
        {
            var data = (object[])recData;
            var _pos = (Vector2)data[0];
            var _type = (Node.NodeType)data[1];
            _pos = DesignerUtility.GetGridPositionFromScreenPoint(_pos);

            Node newNode = null;
            switch (_type)
            {
                case Node.NodeType.Class:
                    {
                        newNode = new Node.Class(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Class");
                    }
                    break;
                case Node.NodeType.Enum:
                    {
                        newNode = new Node.Enum(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Enum");
                    }
                    break;
                case Node.NodeType.Interface:
                    {
                        newNode = new Node.Interface(new Rect(_pos.x, _pos.y, settings.nodeWidth, 100f), "New Interface");
                    }
                    break;
            }

            string originalName = newNode.name;
            int iteration = 1;
            while (activeNodes.Where(x => x.name == newNode.name).Count() != 0)
            {
                newNode.SetName(originalName + "(" + iteration + ")");
                iteration++;
            }

            activeNodes.Add(newNode);
        }

        private void DeleteSelectedNodes()
        {
            foreach (var node in selectedNodes)
            {
                DeleteNode(node);
            }
        }

        private void GroupSelectedNodes()
        {
            //Group nodes
            var group = new Group() { name = "New group" };
            foreach(var node in selectedNodes)
            {
                node.SetParentGroup(group);
            }
            activeGroups.Add(group);

            //if a group got empty remove it
            for (int i = 0; i < activeGroups.Count; i++)
            {
                var grp = activeGroups[i];

                if (grp.childNodes.Count <= 1)
                {
                    activeGroups.Remove(grp);
                }
            }
        }

        private void UngroupSelectedNodes()
        {
            foreach(var node in selectedNodes)
            {
                UngroupNode(node);
            }
        }

        private void UngroupNode(Node node)
        {
            var exParent = node.parentGroup;

            node.parentGroup.RemoveNode(node);
            node.SetParentGroup(null);

            //If parent group got empty remove it
            if(exParent.childNodes.Count <= 1)
            {
                DeleteGroup(exParent);
            }
        }

        private void UngroupNode(object data)
        {
            Node node = (Node)data;
            UngroupNode(node);
        }

        private void DeleteGroup(object data)
        {
            Group group = (Group)data;
            DeleteGroup(group);
        }
        private void DeleteGroup(Group group)
        {
            for(int i = 0; i < group.childNodes.Count; i++)
            {
                Node node = group.childNodes[i];
                node.SetParentGroup(null);
            }

            if (activeGroups.Contains(group))
            {
                activeGroups.Remove(group);
            }
        }

        private void DeleteNode(object data)
        {
            var node = (Node)data;
            if (activeNodes.Contains(node))
            {
                activeNodes.Remove(node);
                foreach (var connection in node.connections)
                {
                    DeleteConnection(connection);
                }
            }
            else
            {
                Debug.Log("Trying to delete non-existent node from activeNodes!");
            }
        }
        #endregion

        #endregion


    }
}