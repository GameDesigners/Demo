using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
using UnityEngine.AddressableAssets;
using System;
using System.Collections;

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

    private Dictionary<int, GameObject> template_page_objs_by_id;
    private List<int> unique_opened_id_list;

    private List<UIBasePage> uiStackLayer=new List<UIBasePage>();
    private GameObject mainCanvasRoot;
    private bool inited = false;
    private UIBasePage currentActivePage;

    public async System.Threading.Tasks.Task<bool> Initialize()
    {
        prefab_msgs_dic_by_id = new Dictionary<int, UIPanelPrefabsList.Elem>();
        prefab_msgs_dic_by_key = new Dictionary<string, UIPanelPrefabsList.Elem>();
        template_page_objs_by_id = new Dictionary<int, GameObject>();
        unique_opened_id_list = new List<int>();

        if (File.Exists(config_file_path))
        {
            UIPanelPrefabsList list = XmlUtil.DeserializeFromFile<UIPanelPrefabsList>(config_file_path);
            if (list != default)
            {
                try
                {
                    if (list.root != default)
                    {
                        foreach (var p in list.root)
                        {
                            prefab_msgs_dic_by_id.Add(p.id, p);
                            prefab_msgs_dic_by_key.Add(p.key, p);
                        }
                    }
                    else
                    {
                        GDebug.Instance.Error($"list.root列表数据为空");
                        return false;
                    }

                    if (list.main_canvas_guid == "")
                    {
                        GDebug.Instance.Error("Main Canvas缺失，UI无法启动...");
                        return false;
                    }
                    else
                    {
                        var tmp = await new AssetReference(list.main_canvas_guid).LoadAssetAsync<GameObject>().Task;
                        if (tmp != null)
                        {
                            mainCanvasRoot = GameObject.Instantiate(tmp);
                            mainCanvasRoot.transform.SetAsLastSibling();//设置为最底部的元素
                        }
                        else
                        {
                            GDebug.Instance.Error("Main Canvas实例化失败");
                            return false;
                        }
                    }


                    foreach (var v in prefab_msgs_dic_by_id)
                    {
                        var tmp = await new AssetReference(v.Value.guid).LoadAssetAsync<GameObject>().Task;
                        if (tmp != null)
                        {
                            template_page_objs_by_id.Add(v.Key, tmp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GDebug.Instance.Error($"operator data[guid]{list.main_canvas_guid}" + ex.ToString());
                    return false;
                }
            }
            else
            {
                GDebug.Instance.Error($"xml：{config_file_path}反序列化失败");
                return false;
            }
        }
        else
        {
            GDebug.Instance.Error($"找不到文件：{config_file_path}");
            return false;
        }



        uiStackLayer = new List<UIBasePage>();

        if (template_page_objs_by_id.Count == prefab_msgs_dic_by_key.Count && mainCanvasRoot != null)
        {
            inited = true;
            return true;
        }
        else
        {
            GDebug.Instance.Error($"数据不完整：template_page_objs_by_id.count({template_page_objs_by_id.Count})<->prefab_msgs_dic_by_key.count({prefab_msgs_dic_by_key.Count})\n或者：mainCanvasRoot为空。");
            return false;
        }
    }




    public int OpenPageNum() => uiStackLayer.Count;


    /// <summary>
    /// 通过id打开UI页面
    /// </summary>
    /// <param name="id">页面ID(可通过UIManagerWindow查找)</param>
    public UIBasePage OpenUIPage(int id)
    {
        //判断UI框架是否初始化成功
        if (!inited)
            return default;

        if (template_page_objs_by_id.ContainsKey(id) && prefab_msgs_dic_by_id.ContainsKey(id))
        {
            bool is_unique = prefab_msgs_dic_by_id[id].unique;
            //判断打开的页面是否要求唯一
            if (is_unique)
            {
                if (unique_opened_id_list.Contains(id))
                    return default;
            }

            var obj = GameObject.Instantiate(template_page_objs_by_id[id]);
            if (obj != null)
            {
                UIBasePage comp = obj.GetComponent<UIBasePage>();
                if (comp != null)
                {
                    if (mainCanvasRoot != null)
                    {
                        comp.page_info = prefab_msgs_dic_by_id[id];
                        uiStackLayer.Add(comp);
                        obj.transform.SetParent(mainCanvasRoot.transform, false);
                        if (is_unique)
                            unique_opened_id_list.Add(id);
                        comp.OnEnter();
                        return comp;
                    }
                    else
                    {
                        GDebug.Instance.Error("Main Canvas为空");
                        return default;
                    }
                }
                else
                {
                    GDebug.Instance.Error($"生成的[id:{id};key:{prefab_msgs_dic_by_id[id].key}]页面不存在UIBasePage组件");
                    return default;
                }
            }
            else
                return default;
        }
        else
        {
            GDebug.Instance.Error($"无法找到id:{id}对应的UI界面模板GameObject");
            return default;
        }
    }

    public bool CloseUIPage(UIBasePage page)
    {
        if(uiStackLayer.Contains(page))
        {
            page.OnExit();
            uiStackLayer.Remove(page);
            GameObject.Destroy(page.gameObject);
            if (unique_opened_id_list.Contains(page.page_info.id))
                unique_opened_id_list.Remove(page.page_info.id);

            return true;
        }
        return false;
    }

    /// <summary>
    /// 通过key值打开UI界面
    /// </summary>
    /// <param name="key"></param>
    public UIBasePage OpenUIPage(string key)
    {
        if (prefab_msgs_dic_by_key.ContainsKey(key))
            return OpenUIPage(prefab_msgs_dic_by_key[key].id);
        else
            return default;
    }
}
