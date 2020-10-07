using UnityEngine;
using System.Collections;
using Designer;
using System;
using UnityEditor;

namespace Designer
{
    [Serializable]
    public class Connection
    {
        public enum ConnectionType
        {
            Inheritance,
            Interface
        }
        public IOPoint fromPoint { get; private set; }
        public Node fromNode => fromPoint.parentNode;

        public IOPoint toPoint { get; private set; }
        public Node toNode => toPoint.parentNode;

        public ConnectionType type { get; private set; }
        public Vector2 fromPosition => fromPoint.bezierStartPosition;
        public Vector2 toPosition => toPoint.bezierStartPosition;

        public Vector2 fromTangent => fromPosition + DesignerUtility.EditorSettings.BezierTangent * EditorData.zoomRatio;
        public Vector2 toTangent => toPosition - DesignerUtility.EditorSettings.BezierTangent * EditorData.zoomRatio;

        public Rect boundingRect
        {
            get
            {
                return new Rect(fromPosition, toPosition - fromPosition);
            }
        }

        public Connection(Node from, Node to, ConnectionType type)
        {
            fromPoint = from.output;
            toPoint = to.input;
            this.type = type;
        }

        public Connection(IOPoint from, IOPoint to, ConnectionType type)
        {
            fromPoint = from;
            toPoint = to;
            this.type = type;
        }

        public void Draw()
        {
            var settings = DesignerUtility.EditorSettings;
            var color = type == ConnectionType.Inheritance ? settings.inheritanceConnectionColor : settings.interfaceConnectionColor;
            Handles.DrawBezier(fromPosition, toPosition, fromTangent, toTangent, color, DesignerUtility.EditorSettings.lineTexture, settings.connectionWidth * EditorData.zoomRatio);
        }

        public void Draw(Color customColor)
        {
            Handles.DrawBezier(fromPosition, toPosition, fromTangent, toTangent, customColor, DesignerUtility.EditorSettings.lineTexture, DesignerUtility.EditorSettings.connectionWidth * EditorData.zoomRatio);
        }
    }
}