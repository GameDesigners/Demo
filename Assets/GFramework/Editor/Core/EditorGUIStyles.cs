using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorGUIStyles
{
    private static EditorGUIStyles _instance;
    public static EditorGUIStyles Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EditorGUIStyles();
            return _instance;
        }

        private set { }
    }


    public GUIStyle TitleStyle;
    public GUIStyle GirdTextBoldStyle;

    private EditorGUIStyles()
    {
        if (TitleStyle == null)
        {
            TitleStyle = new GUIStyle { fontSize = 20, fontStyle = FontStyle.Bold };
            TitleStyle.normal.textColor = Color.white;
        }

        if(GirdTextBoldStyle==null)
        {
            GirdTextBoldStyle = new GUIStyle { fontSize = 12, fontStyle = FontStyle.Bold };
            GirdTextBoldStyle.alignment = TextAnchor.MiddleLeft;
            GirdTextBoldStyle.normal.textColor = Color.white;
        }
    }
}

public class GEditorGUI
{
    /// <summary>
    /// 标题样式
    /// </summary>
    /// <param name="title">标题文本</param>
    /// <param name="space">上方空间的长度</param>
    /// <param name="height">文本高度</param>
    public static void Title(string title,int space=10,int height=30)
    {
        EditorGUILayout.Space(space);
        EditorGUILayout.LabelField(title, EditorGUIStyles.Instance.TitleStyle, new[] { GUILayout.Height(height) });
    }
    public static void Label(string text, int width=0, int height=0)
    {
        if (width == 0 && height != 0)
            EditorGUILayout.LabelField(text, new[] { GUILayout.Height(height) });
        else if (width != 0 && height == 0)
            EditorGUILayout.LabelField(text, new[] { GUILayout.Width(width) });
        else if (width == 0 && height == 0)
            EditorGUILayout.LabelField(text);
        else
            EditorGUILayout.LabelField(text, new[] { GUILayout.Width(width), GUILayout.Height(height) });

    }
    public static bool Button(string text, int width, int height) => GUILayout.Button(text, new[] { GUILayout.Width(width), GUILayout.Height(height) });
}

public class HelpInfo
{
    private bool isShow = false;
    private string msg = "";
    private MessageType msgType;

    public void SetState(bool _show,string _msg="",MessageType _type= MessageType.None)
    {
        isShow = _show;
        msg = _msg;
        msgType = _type;
    }

    public bool Show(int space = 5)
    {
        EditorGUILayout.Space(space);
        if (isShow)
            EditorGUILayout.HelpBox(msg, msgType);
        return isShow;
    }
}