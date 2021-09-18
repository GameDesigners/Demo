using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 面板的基类
/// </summary>
public class UIBasePage : MonoBehaviour, IEqualityComparer<UIBasePage>
{
    public UIPanelPrefabsList.Elem page_info;
    public Stack<UIBasePage> child_page = new Stack<UIBasePage>();

    /// <summary>
    /// 页面显示的时候调用
    /// </summary>
    public void OnEnter() { }

    /// <summary>
    /// 页面暂停的时候
    /// </summary>
    public void OnPause() { }

    /// <summary>
    /// 页面恢复的时候调用
    /// </summary>
    public void OnResume() { }

    /// <summary>
    /// 页面退出的时候调用
    /// </summary>
    public void OnExit() { }

    public bool Equals(UIBasePage x, UIBasePage y)
    {
        return (x.page_info.id == y.page_info.id) && (x.gameObject == y.gameObject);
    }

    public int GetHashCode(UIBasePage obj)
    {
        return obj.GetHashCode();
    }
}
