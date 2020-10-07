using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Designer
{
    public static class DesignerUtility
    {
        private const string _editorSettingsPath = "Assets/ProjectDesigner+/Data/Editor Settings.asset";
        private const string _defaultEditorSettingsPath = "Assets/ProjectDesigner+/Data/DefaultEditorSettings.asset";
        private static EditorSettings _editorSettings;
        public static EditorSettings EditorSettings
        {
            get
            {
                if (_editorSettings == null)
                {
                    _editorSettings = AssetDatabase.LoadAssetAtPath(_editorSettingsPath, typeof(EditorSettings)) as EditorSettings;
                }
                return _editorSettings;
            }
        }

        public static EditorSettings ResetEditorSettings()
        {
            AssetDatabase.DeleteAsset(_editorSettingsPath);
            AssetDatabase.CopyAsset(_defaultEditorSettingsPath, _editorSettingsPath);
            return AssetDatabase.LoadAssetAtPath(_editorSettingsPath, typeof(EditorSettings)) as EditorSettings;

        }

        /// <summary>
        /// Gets the grid position under given point. Every grid element represents a 100x100 unit square.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetGridPositionFromScreenPoint(Vector2 screenPos)
        {
            return (-EditorData.offset + screenPos) / EditorData.zoomRatio; ;//Works as intended
        }

        /// <summary>
        /// Gets the screen position from given grid point. Every grid element represents a 100x100 unit square.
        /// </summary>
        /// <param name="gridPoint"></param>
        /// <param name="gridOffset"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static Vector2 GetScreenPositionFromGridPoint(Vector2 gridPoint)
        {
            return EditorData.offset + gridPoint * EditorData.zoomRatio;//Works as intended
        }


        /// <summary>
        /// Set viewport center to given grid position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static void CenterViewToPosition(Vector2 pos)
        {
            EditorData.offset = -pos * EditorData.zoomRatio + (EditorData.windowRect.size / 2);//Works as intended
        }
    }
}