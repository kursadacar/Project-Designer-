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
        public float lastWidth;

        public OptionsPanelData(bool openAllFoldouts)
        {
            showDebugWindow = openAllFoldouts;
            showNodes = openAllFoldouts;
            showEditorSettings = openAllFoldouts;
            showGroups = openAllFoldouts;
            scrollPosition = new Vector2();
            lastWidth = 250f;
        }
    }
}