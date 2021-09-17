using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
using UnityEngine.AddressableAssets;
using System;

/// <summary>
/// UI 面板的基类
/// </summary>
public class UIBasePanel
{
    /// <summary>
    /// 页面显示的时候调用
    /// </summary>
    private void OnEnter() { }

    /// <summary>
    /// 页面暂停的时候
    /// </summary>
    private void OnPause() { }
    
    /// <summary>
    /// 页面恢复的时候调用
    /// </summary>
    private void OnResume() { }

    /// <summary>
    /// 页面退出的时候调用
    /// </summary>
    private void OnExit() { }
}


public class GUIManager
{
    private static GUIManager _instance;
    public static GUIManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GUIManager();
            return _instance;
        }
    }

    //configs data
    private string config_file_path = Configs.Instance.UIPrefabsConfigPath;
    private Dictionary<int, UIPanelPrefabsList.Elem> prefab_msgs_dic_by_id;
    private Dictionary<string, UIPanelPrefabsList.Elem> prefab_msgs_dic_by_key;

    private Stack<UIBasePanel> uiStackLayer;
    private GameObject mainCanvasRoot;
    private GUIManager()
    {
        LoadUIPrefabs();

        uiStackLayer = new Stack<UIBasePanel>();
    }

    private void LoadUIPrefabs()
    {
        prefab_msgs_dic_by_id = new Dictionary<int, UIPanelPrefabsList.Elem>();
        prefab_msgs_dic_by_key = new Dictionary<string, UIPanelPrefabsList.Elem>();
        if (File.Exists(config_file_path))
        {
            UIPanelPrefabsList list = XmlUtil.DeserializeFromFile<UIPanelPrefabsList>(config_file_path);
            if (list != default)
            {
                try
                {
                    if (list.main_canvas_guid=="")
                    {
                        GDebug.Instance.Error("Main Canvas缺失，UI无法启动...");
                        return;
                    }
                    else
                    {
                        new AssetReference(list.main_canvas_guid).InstantiateAsync().Completed += LoadMainCanvasObj_Completed;
                    }
                }
                catch(Exception ex)
                {
                    GDebug.Instance.Error($"operator data[guid]{list.main_canvas_guid}"+ex.ToString());
                }
                if (list.root != default)
                {
                    foreach (var p in list.root)
                    {
                        prefab_msgs_dic_by_id.Add(p.id, p);
                        prefab_msgs_dic_by_key.Add(p.key, p);
                    }
                }
                else
                    GDebug.Instance.Error($"list.root列表数据为空");
            }
            else
                GDebug.Instance.Error($"xml：{config_file_path}反序列化失败");
        }
        else
            GDebug.Instance.Error($"找不到文件：{config_file_path}");
    }

    private void LoadMainCanvasObj_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
    {
        mainCanvasRoot = obj.Result;
    }






    /// <summary>
    /// 通过id打开UI页面
    /// </summary>
    /// <param name="id">页面ID(可通过UIManagerWindow查找)</param>
    public void OpenUIPage(int id)
    {
        if (prefab_msgs_dic_by_id.ContainsKey(id))
        {

        }
        else
            GDebug.Instance.Log($"无法找到id:{id}对应的UI界面");
    }
}
