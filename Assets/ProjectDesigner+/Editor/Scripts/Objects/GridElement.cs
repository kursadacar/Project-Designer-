using UnityEngine;
using System.Collections;

namespace Designer
{
    public abstract class GridElement
    {
        public enum Type
        {
            Node,
            Group,
            Connection,
            IOPoint
        }

        public abstract Type type { get; }

        public virtual Rect position { get; internal set; }
        public virtual Rect screenPosition => new Rect(DesignerUtility.GetScreenPositionFromGridPoint(position.position), position.size * EditorData.zoomRatio);

        public string name { get; set; }
        public bool isHovered { get; set; }

        public abstract void Draw();

        public virtual bool CheckIfHovered()
        {
            if (screenPosition.Contains(EditorData.mousePosition))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}