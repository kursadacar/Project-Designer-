using UnityEngine;
using System.Collections;

namespace Designer
{
    public struct OptionsPanelData
    {
        public bool showDebugWindow;
        public bool showNodes;
        public bool showGroups;
        public bool showEditorSettings;
        public Vector2 scrollPosition;
        public float targetX;
        public float activeX;
        public float lastX;
        public float minX => -250f;
        public float width => 250f;

        public OptionsPanelData(bool openAllFoldouts)
        {
            showDebugWindow = openAllFoldouts;
            showNodes = openAllFoldouts;
            showEditorSettings = openAllFoldouts;
            showGroups = openAllFoldouts;
            scrollPosition = new Vector2();
            targetX = 0f;
            lastX = targetX;
            activeX = targetX;
        }
    }
}