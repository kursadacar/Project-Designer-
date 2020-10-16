using UnityEngine;
using System.Collections;
using Designer;
using System;
using UnityEditor;

namespace Designer
{
    [Serializable]
    public class Connection : GridElement
    {
        public override Type type => Type.Connection;

        public override Rect position
        {
            get
            {
                return new Rect(fromPoint.position.center, toPoint.position.center - fromPoint.position.center);
            }
            internal set
            {
            }
        }

        public override bool CheckIfHovered()
        {
            var dist = HandleUtility.DistancePointBezier(EditorData.mousePosition, fromPosition, toPosition, fromTangent, toTangent);
            if (dist < DesignerUtility.EditorSettings.connectionWidth * EditorData.zoomRatio * 1.3f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public enum ConnectionType
        {
            Inheritance,
            Interface
        }
        public IOPoint fromPoint { get; private set; }
        public Node fromNode => fromPoint.parentNode;

        public IOPoint toPoint { get; private set; }
        public Node toNode => toPoint.parentNode;

        public ConnectionType connectionType { get; private set; }
        public Vector2 fromPosition => fromPoint.bezierStartPosition;
        public Vector2 toPosition => toPoint.bezierStartPosition;

        public Vector2 fromTangent => fromPosition + DesignerUtility.EditorSettings.BezierTangent * EditorData.zoomRatio;
        public Vector2 toTangent => toPosition - DesignerUtility.EditorSettings.BezierTangent * EditorData.zoomRatio;

        public Connection(Node from, Node to, ConnectionType type)
        {
            fromPoint = from.output;
            toPoint = to.input;
            connectionType = type;
        }

        public Connection(IOPoint from, IOPoint to, ConnectionType type)
        {
            fromPoint = from;
            toPoint = to;
            connectionType = type;
        }

        public override void Draw()
        {
            var settings = DesignerUtility.EditorSettings;
            var color = connectionType == ConnectionType.Inheritance ? settings.inheritanceConnectionColor : settings.interfaceConnectionColor;
            Handles.DrawBezier(fromPosition, toPosition, fromTangent, toTangent, isHovered ? Color.cyan : color, DesignerUtility.EditorSettings.lineTexture, settings.connectionWidth * EditorData.zoomRatio);
        }
    }
}