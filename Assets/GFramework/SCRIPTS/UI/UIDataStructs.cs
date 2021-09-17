using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 面板的资源引用列表
/// </summary>
[Serializable]
public class UIPanelPrefabsList
{
    [Serializable]
    public class Elem
    {
        public int    id;
        public string key;
        public string description;
        public string guid;
        public bool   unique;
    }
    public string main_canvas_guid;
    public List<Elem> root = new List<Elem>();
}

/// <summary>
/// UI 堆栈”栈帧“
/// </summary>
public class UIStackFrame
{
    public UIPanelPrefabsList.Elem msg;
    public GameObject obj;

    public UIStackFrame(UIPanelPrefabsList.Elem _msg,GameObject _obj)
    {
        msg = _msg;
        obj = _obj;
    }
}