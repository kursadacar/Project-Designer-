using Designer;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class CustomStyles
{
    /// <summary>
    /// Resizes element sizes according to zoom ratio.
    /// </summary>
    /// <param name="style"></param>
    /// <param name="fontSize"></param>
    private static void UpdateStyleSizes(GUIStyle style, int fontSize = 14)
    {
        style.fontSize = (int)(fontSize * EditorData.zoomRatio);
        style.fixedHeight = DesignerUtility.EditorSettings.NodeElementHeight;
    }

    #region Get Custom Styles With Options

    #region Temp Styles
    private static GUIStyle _customLabel;
    private static GUIStyle _customColorPicker;
    #endregion

    public static GUIStyle GetCustomLabel(Color color, int fontSize = 14, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        if(_customLabel == null)
        {
            _customLabel = new GUIStyle(EditorStyles.label);
        }
        _customLabel.alignment = anchor;
        _customLabel.normal.textColor = color;
        _customLabel.fontSize = fontSize;
        UpdateStyleSizes(_customLabel,fontSize);
        return _customLabel;
    }
    #endregion

    private static GUIStyle _button;
    public static GUIStyle button
    {
        get
        {
            if (_button == null)
            {
                _button = new GUIStyle(EditorStyles.miniButton);
            }
            UpdateStyleSizes(_button);
            return _button;
        }
    }



    private static GUIStyle _textField;
    public static GUIStyle textField
    {
        get
        {
            if (_textField == null)
            {
                _textField = new GUIStyle(EditorStyles.miniButton);
                _textField.alignment = TextAnchor.MiddleCenter;
            }
            UpdateStyleSizes(_textField);
            return _textField;
        }
    }

    private static GUIStyle _enumPopup;
    public static GUIStyle enumPopup
    {
        get
        {
            if(_enumPopup == null)
            {
                _enumPopup = new GUIStyle(EditorStyles.toolbarDropDown);
            }
            UpdateStyleSizes(_enumPopup);
            return _enumPopup;
        }
    }

    private static GUIStyle _nodeHeader;
    public static GUIStyle nodeHeader
    {
        get
        {
            if(_nodeHeader == null)
            {
                _nodeHeader = new GUIStyle(EditorStyles.helpBox);
                _nodeHeader.fontSize = 14;
                _nodeHeader.fontStyle = FontStyle.Bold;
                _nodeHeader.alignment = TextAnchor.MiddleCenter;
                _nodeHeader.normal.textColor = Color.white;
                _nodeHeader.wordWrap = false;
                _nodeHeader.focused.textColor = Color.yellow;
            }
            UpdateStyleSizes(_nodeHeader);
            return _nodeHeader;
        }
    }

    private static GUIStyle _groupHeader;
    public static GUIStyle groupHeader
    {
        get
        {
            if (_groupHeader == null)
            {
                _groupHeader = new GUIStyle(EditorStyles.label);
                _groupHeader.fontSize = 22;
                _groupHeader.fontStyle = FontStyle.Bold;
                _groupHeader.alignment = TextAnchor.MiddleCenter;
                _groupHeader.normal.textColor = Color.black;
                _groupHeader.wordWrap = false;
                _groupHeader.focused.textColor = Color.blue;
            }
            UpdateStyleSizes(_groupHeader);
            return _groupHeader;
        }
    }
}