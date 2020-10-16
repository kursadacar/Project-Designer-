using UnityEngine;
using System.Collections;

namespace Designer
{
    public static class EditorData
    {
        public static float gridSize { get; private set; }
        public static float zoomRatio { get; private set; }
        public static void SetZoomRatio(float ratio)
        {
            zoomRatio = ratio;
        }
        public static void SetGridSize(float val)
        {
            gridSize = val;
        }

        public static Vector2 offset = new Vector2();

        public static Rect windowRect = new Rect();
        public static float deltaTime { get; private set; }
        public static void SetDeltaTime(float value)
        {
            deltaTime = value;
        }

        public static Vector2 mousePosition { get; private set; }
        public static void SetMousePosition(Vector2 pos)
        {
            mousePosition = pos;
        }
    }
}