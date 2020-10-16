using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Designer
{
    //[CreateAssetMenu(fileName = "Editor Settings", menuName = "Temp")]
    public class EditorSettings : ScriptableObject
    {
        [Header("Options")]
        public float optionsPanelWidth = 250f;
        public float maxGridSize = 200f;
        public float minGridSize = 30f;
        public float connectionWidth = 6f;

        [Header("Textures")]
        public Texture2D nodeTexture;
        public Texture2D lineTexture;
        public Texture2D gradientTexture;
        public Texture2D IOPointTexture;
        public Texture2D nodeOutlineTexture;
        public Texture2D colorWheelTexture;
        public Texture2D shadowTexture;

        [Header("Colors")]
        public Color selectionRectColor = new Color(0f, 200f / 255f, 1f, 158f / 255f);
        public Color inheritanceConnectionColor = Color.blue;
        public Color interfaceConnectionColor = Color.yellow;
        public Color groupColor = Color.yellow;
        public Color groupHeaderColor;
        public Color groupHeaderTextFieldColor;
        public Color IOPointColor;
        public Color nodeShadowColor;
        public Color gridLineColor = Color.gray;
        public Color gridHighlightColor = Color.white;
        public Color optionsPanelColor = new Color(207f / 255f, 207f / 255f, 207f / 255f);

        //public GUIStyle temp;

        public List<NodeColor> nodeColors = new List<NodeColor>();
        public NodeColor interfaceColor;

        [HideInInspector] public Vector2 BezierTangent = new Vector2(50f, 0f);

        [HideInInspector]
        public float NodeElementHeight
        {
            get
            {
                float value = _nodeElementHeight * EditorData.zoomRatio;
                if (value < 3f)
                    value = 3f;
                return value;
            }
        }

        [SerializeField, HideInInspector] private float _nodeElementHeight = 21f;
        [SerializeField, HideInInspector] public readonly float nodeWidth = 300f;

        public void SetRawNodeElementHeight(float value)
        {
            _nodeElementHeight = value;
        }
        public float GetRawNodeElementHeight()
        {
            return _nodeElementHeight;
        }



    }
}