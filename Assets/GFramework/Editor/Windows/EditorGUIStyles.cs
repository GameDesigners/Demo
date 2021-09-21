using System.Collections;
using System.Collections.Generic;
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
