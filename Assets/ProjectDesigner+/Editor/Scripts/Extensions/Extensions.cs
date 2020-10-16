using UnityEngine;
using System.Linq;
using System.Collections;
using Designer;
using UnityEditor;

namespace Designer
{
    public static class Extensions
    {
        #region Rect
        /// <summary>
        /// Returns true if rect contains any part of the given area
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public static bool ContainsArea(this Rect rect, Rect area)
        {
            if (rect.Contains(area.position, true) ||
                rect.Contains(area.position + area.size, true) ||
                rect.Contains(area.position + new Vector2(area.width, 0f), true) ||
                rect.Contains(area.position + new Vector2(0f, area.height), true))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}