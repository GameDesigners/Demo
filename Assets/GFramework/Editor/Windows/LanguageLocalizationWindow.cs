using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using Framework.DataManager;

public class LanguageLocalizationWindow : EditorWindow
{
    private static LanguageLocalizationWindow _llw_window;
    private Vector2 scrollPos = Vector2.zero;
    private SerializedObject _this;

    //===========================================================================
    [MenuItem("GFrameworkEditorWindows/LanguageLocalizationWindow")]
    private static void Open()
    {
        if (_llw_window == null)
            _llw_window = GetWindow<LanguageLocalizationWindow>();
        _llw_window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        _this = new SerializedObject(this);

        ui_gameobject_list_property = _this.FindProperty("ui_gameobject_list");
        ui_language_list_property = _this.FindProperty("ui_language_list");

        gird_width = (int)position.width / (ui_language_list.Count + 1);


        LoadLocalizationCustomSetting();
        UpdateLanguageIndex();
        ChangeCustomConfig();
        ChangeGameObject();

        Debug.Log("Call OnEnable()");
    }
    //===========================================================================



    #region 本地化常规设置部分
    private SerializedProperty ui_gameobject_list_property;
    private SerializedProperty ui_language_list_property;

    [SerializeField] private List<GameObject> ui_gameobject_list=new List<GameObject>();
    [SerializeField] private List<string> ui_language_list = new List<string>();

    /// <summary>
    /// 加载关于本地化常规设置配置
    /// </summary>
    private void LoadLocalizationCustomSetting()
    {
        if (!Directory.Exists(Configs.Instance.Editor_EditorConfigFolderPath))
        {
            Directory.CreateDirectory(Configs.Instance.Editor_EditorConfigFolderPath);
            return;
        }

        if(File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
        {
            LocalizationLanguageEditorConfig lle_config = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);
            if(lle_config!=default)
            {
                ui_gameobject_list.Clear();
                if (lle_config.gameObjectPathsInProject!=null)
                {
                    foreach(var path in lle_config.gameObjectPathsInProject)
                        ui_gameobject_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }

                if(lle_config.languageList!=null)
                {
                    ui_language_list.Clear();
                    foreach (var l in lle_config.languageList)
                        ui_language_list.Add(l);
                }
            }
        }
    }

    private void SaveLocalizationCustomSetting()
    {
        if (!Directory.Exists(Configs.Instance.Editor_EditorConfigFolderPath))
            Directory.CreateDirectory(Configs.Instance.Editor_EditorConfigFolderPath);
        if (File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
            File.Delete(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);

        LocalizationLanguageEditorConfig lle_config = new LocalizationLanguageEditorConfig();
    }

    /// <summary>
    /// 本地化常规设置GUI渲染函数
    /// </summary>
    private void LocalizationCustomSettingGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("本地化常规设置", EditorGUIStyles.Instance.TitleStyle, new[] { GUILayout.Height(30) });
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(!is_editing_language_list);
        EditorGUILayout.PropertyField(ui_gameobject_list_property, new GUIContent("UI预制体列表"), true);
        EditorGUILayout.PropertyField(ui_language_list_property, new GUIContent("语言列表"), true);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(is_editing_language_list ? "保存" : "编辑", new[] { GUILayout.Width(60), GUILayout.Height(20) }))
        {
            if (is_editing_language_list)
            {
                gird_width = (int)position.width / (ui_language_list.Count + 1);
                ChangeCustomConfig();
            }
            else
                UpdateLanguageIndex();
            is_editing_language_list = !is_editing_language_list;
        }
        if (GUILayout.Button("更新xlsx", new[] { GUILayout.Width(60), GUILayout.Height(20) }))
        {
            if (EditorUtility.DisplayDialog("重要警告", "此操作将会覆盖原本的数据，请确定是否操作。", "确认", "取消"))
            {
                ExcelUtil.Write(Configs.Instance.LocalizationLanguageConfigFilePath, ui_excel_data);
                GDebug.Instance.Log("更新了language_localization.xlsx");
            }
        }
        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            _this.ApplyModifiedProperties();
            ui_game_object_name_list.Clear();
            foreach (var obj in ui_gameobject_list)
                ui_game_object_name_list.Add(obj.name);
        }
    }


    #endregion

























    [SerializeField] private List<string> ui_game_object_name_list = new List<string>();
    [SerializeField] private Dictionary<int, GameObject> ui_language_elem_dic = new Dictionary<int, GameObject>();
    private int ui_gameobject_selected = 0;
    private bool is_editing_language_list = false;
    private int gird_width = 0;


    private LocalizationLanguageEditorConfig editor_setting = new LocalizationLanguageEditorConfig();

    private List<List<string>> ui_table_data = new List<List<string>>();
    private ExcelData ui_excel_data = new ExcelData();
    private Dictionary<string, int> language_index = new Dictionary<string, int>();

    private HelpInfo ui_localization_editor_info = new HelpInfo();

    
    

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        gird_width = ((int)position.width-10) / (ui_language_list.Count + 1)-5;

        LocalizationCustomSettingGUI();


        EditorGUILayout.Space(20);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("本地化数据表格编辑器", EditorGUIStyles.Instance.TitleStyle, new[] { GUILayout.Height(30) });
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(" ", EditorGUIStyles.Instance.GirdTextBoldStyle, new[] {GUILayout.Width(10) });
        ui_gameobject_selected = EditorGUILayout.Popup(new GUIContent("UI对象"), ui_gameobject_selected, ui_game_object_name_list.ToArray());
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            ChangeGameObject();
            _this.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        if(!ui_localization_editor_info.Show())
        {
            EditorGUILayout.Space(10);
            for (int i = 0; i < ui_table_data.Count; i++)
            {

                EditorGUILayout.BeginHorizontal();
                if (i == 0)
                {
                    EditorGUILayout.LabelField(" ", EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Height(15), GUILayout.Width(10) });
                    for (int j = 0; j < ui_table_data[i].Count; j++)
                        EditorGUILayout.LabelField(ui_table_data[i][j], EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Height(15), GUILayout.Width(gird_width) });
                }
                else
                {
                    EditorGUILayout.LabelField(" ", EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Height(40), GUILayout.Width(10) });
                    for (int j = 0; j < ui_table_data[i].Count; j++)
                        ui_table_data[i][j] = EditorGUILayout.TextArea(ui_table_data[i][j], new[] { GUILayout.Height(40), GUILayout.Width(gird_width) });

                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存修改", new[] { GUILayout.Width(60), GUILayout.Height(20) }))
            {
                if (EditorUtility.DisplayDialog("警告", "此操作将会写入持久化数据\n请确认是否保存修改。", "确认", "取消"))
                {
                    //ExcelUtil.Write(Application.streamingAssetsPath + "/language_localization.xlsx", ui_table_data, ui_game_object_name_list[ui_gameobject_selected]);
                    string name = ui_game_object_name_list[ui_gameobject_selected];
                    if (ui_excel_data.ContainsKey(name))
                        ui_excel_data[name] = ui_table_data;
                    ExcelUtil.Write(Application.streamingAssetsPath + "/language_localization.xlsx", ui_excel_data);


                    foreach (var e in ui_language_elem_dic)
                    {
                        if (e.Key < ui_table_data.Count)
                        {
                            GUILocalization l = e.Value.GetComponent<GUILocalization>();
                            if (l != null)
                                l.key = ui_table_data[e.Key][0];
                            EditorUtility.SetDirty(e.Value);
                        }
                    }
                    AssetDatabase.Refresh();
                }
                else
                    Debug.Log("取消修改");
            }
            GUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck())
        {
            _this.ApplyModifiedProperties();
        }
        GUILayout.EndScrollView();
    }


    /// <summary>
    /// 加载本地文件记录的数据
    /// </summary>
    private void LoadLocalFileRecordData()
    {
        ui_excel_data = ExcelUtil.Read(Configs.Instance.LocalizationLanguageConfigFilePath);
        ui_gameobject_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TestFiles/Prefabs/@gaming_page.prefab"));
    }

    /// <summary>
    /// 改变常规设置:UI Prefabs列表
    /// 此函数需要将原来的xslx文本
    /// </summary>
    private void ChangeCustomConfig()
    {
        ExcelData newData = new ExcelData();

        foreach (var obj in ui_gameobject_list) 
        {
            List<List<string>> data = new List<List<string>>();
            data.Add(new List<string> { "KEY" });
            data[0].AddRange(ui_language_list);

            GUILocalization[] locals = obj.GetComponentsInChildren<GUILocalization>();
            if (ui_excel_data.ContainsKey(obj.name))
            {
                List<List<string>> tb = ui_excel_data[obj.name];
                foreach (var l in locals)
                {
                    List<string> row = new List<string>();
                    row.Add(l.key);
                    var query = (from r in tb
                                where r[0] == l.key
                                select r).ToList();
                    if(query.Count>=1)
                    {
                        //需要原来xlsx文件上记录的东西拷贝到新的Data上
                        foreach (var language in ui_language_list)
                        {
                            if (language_index.ContainsKey(language))
                                row.Add(query[0][language_index[language]]);
                            else
                                row.Add("");
                        }
                    }
                    else
                    {
                        foreach (var language in ui_language_list)
                            row.Add("");
                    }
                    data.Add(row);
                }
            }

            newData.Add(obj.name, data);
        }
        ui_excel_data = newData;
    }

    private void UpdateLanguageIndex()
    {
        //编辑前更新语言索引
        language_index.Clear();
        for (int index = 0; index < ui_language_list.Count; index++)
            language_index.Add(ui_language_list[index], index);
    }

    private void ChangeGameObject()
    {
        ui_language_elem_dic.Clear();
        ui_table_data.Clear();
        List<string> title_list = new List<string> { "KEY" };
        title_list.AddRange(ui_language_list);

        ui_table_data.Add(title_list);
        GameObject ui_gameobject = null;
        if (ui_gameobject_list.Count > ui_gameobject_selected)
            ui_gameobject = ui_gameobject_list[ui_gameobject_selected];
        if (ui_gameobject != null)
        {
            GUILocalization[] locals = ui_gameobject.GetComponentsInChildren<GUILocalization>();
            if (locals != null)
            {
                if (locals.Length != 0)
                {
                    ui_localization_editor_info.isShow = false;
                    foreach (var l in locals)
                    {
                        ui_language_elem_dic.Add(ui_table_data.Count, l.gameObject);
                        List<string> default_list = new List<string>();
                        default_list.Add(l.key == "" ? l.gameObject.name : l.key);
                        for (int i = 0; i < ui_language_list.Count; i++)
                            default_list.Add("");
                        ui_table_data.Add(default_list);
                    }
                    _this.Update();
                }
                else
                {
                    ui_localization_editor_info.isShow = true;
                    ui_localization_editor_info.msgType = MessageType.Warning;
                    ui_localization_editor_info.msg = $"[{ui_gameobject.name}]未找到LanguageElem组件";
                }
            }
            else
            {
                ui_localization_editor_info.isShow = true;
                ui_localization_editor_info.msgType = MessageType.Warning;
                ui_localization_editor_info.msg = $"[{ui_gameobject.name}]未找到LanguageElem组件";
            }
        }
        else
        {
            ui_localization_editor_info.isShow = true;
            ui_localization_editor_info.msgType = MessageType.Warning;
            ui_localization_editor_info.msg = $"[当前选择的GameObject为空]";
        }
    }


}
