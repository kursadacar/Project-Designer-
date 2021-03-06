﻿using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace Designer
{
    public abstract class NodeElement
    {
        private static ElementNames _header;
        public static ElementNames Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new ElementNames();
                }
                return _header;
            }
        }

        private static readonly float rawComponentOffset = 1f;
        private static float componentOffset => rawComponentOffset * EditorData.zoomRatio;

        private Rect firstComponentPosition, secondComponentPosition, thirdComponentPosition;

        public enum ProtectionLevel
        {
            @public,
            @private,
            @internal,
            @protected
        }

        public enum PrimitiveType
        {
            @int,
            @float,
            @string,
            @char,
            @bool
        }

        public enum NodeElementType
        {
            Field,
            Method
        }

        public ProtectionLevel protectionLevel { get; private set; }
        public string name { get; private set; }

        internal abstract void DrawInternal(Rect position);
        public void Draw(Rect position)
        {
            float sliceWidth = position.width / 3f;

            firstComponentPosition = new Rect(0f, 0f, sliceWidth - componentOffset * 2f, position.height);
            secondComponentPosition = new Rect(sliceWidth + componentOffset, 0f, sliceWidth - componentOffset * 2f, position.height);
            thirdComponentPosition = new Rect(sliceWidth * 2f + componentOffset, 0f, sliceWidth - componentOffset * 2f, position.height);

            DrawInternal(position);
        }

        public class ElementNames : NodeElement
        {
            internal override void DrawInternal(Rect position)
            {
                GUILayout.BeginArea(position);
                GUILayout.BeginHorizontal(GUILayout.Height(position.height));

                var labelStyle = StylePresets.GetCustomLabel(Color.white, 12, TextAnchor.MiddleLeft);
                GUI.Label(firstComponentPosition, "Name", StylePresets.GetCustomLabel(Color.black, 12, TextAnchor.MiddleCenter));
                GUI.Label(secondComponentPosition, "Protection Level", StylePresets.GetCustomLabel(Color.black, 12, TextAnchor.MiddleCenter));
                GUI.Label(thirdComponentPosition, "Type", StylePresets.GetCustomLabel(Color.black, 12, TextAnchor.MiddleCenter));
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        public class Field : NodeElement
        {
            public PrimitiveType primitiveType { get; private set; }
            internal override void DrawInternal(Rect position)
            {
                GUILayout.BeginArea(position);
                GUILayout.BeginHorizontal(GUILayout.Height(position.height));
                var labelStyle = StylePresets.GetCustomLabel(Color.white, 12, TextAnchor.MiddleLeft);

                name = GUI.TextField(firstComponentPosition, name, StylePresets.textField);
                protectionLevel = (ProtectionLevel)EditorGUI.EnumPopup(secondComponentPosition, protectionLevel, StylePresets.enumPopup);
                primitiveType = (PrimitiveType)EditorGUI.EnumPopup(thirdComponentPosition, primitiveType, StylePresets.enumPopup);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                //protectionLevel = (ProtectionLevel)EditorGUILayout.EnumPopup(protectionLevel, popupStyle);
                //primitiveType = (PrimitiveType)EditorGUILayout.EnumPopup(primitiveType, popupStyle);
                //name = GUILayout.TextField(name, textFieldStyle);
            }
        }
        public class Method : NodeElement
        {
            public PrimitiveType returnType { get; private set; }
            internal override void DrawInternal(Rect position)
            {
                GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.cyan, 0f, 0f);
                //protectionLevel = (ProtectionLevel)EditorGUILayout.EnumPopup(protectionLevel,popupStyle);
                //returnType = (PrimitiveType)EditorGUILayout.EnumPopup(returnType,popupStyle);
                //name = GUILayout.TextArea(name, textFieldStyle);
            }
        }
    }
}