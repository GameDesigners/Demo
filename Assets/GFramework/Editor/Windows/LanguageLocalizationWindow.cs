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

        LocalizationCustomSettingInitialization();
        //LoadLocalizationCustomSetting();
        UpdateLanguageIndex();
        ChangeCustomConfig();
        ChangeGameObject();

        Debug.Log("Call OnEnable()");
    }
    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        gird_width = ((int)position.width - 10) / (ui_language_list.Count + 1) - 5;

        LocalizationCustomSettingMainGUI();


        EditorGUILayout.Space(20);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("本地化数据表格编辑器", EditorGUIStyles.Instance.TitleStyle, new[] { GUILayout.Height(30) });
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(" ", EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Width(10) });
        ui_gameobject_selected = EditorGUILayout.Popup(new GUIContent("UI对象"), ui_gameobject_selected, ui_game_object_name_list.ToArray());
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            ChangeGameObject();
            _this.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        if (!ui_localization_editor_info.Show())
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
    private void OnDisable()
    {
        //SaveLocalizationCustomSetting();
    }
    //===========================================================================


    #region 本地化常规设置部分
    private SerializedProperty ui_gameobject_list_property;
    private SerializedProperty ui_language_list_property;

    [SerializeField] private List<GameObject> ui_gameobject_list = new List<GameObject>();
    [SerializeField] private List<string> ui_language_list = new List<string>();

    private List<string> config_file_list = new List<string>();
    private List<string> config_file_names_without_extension = new List<string>();
    private int config_file_select = 0;
    private HelpInfo no_have_configs_info = new HelpInfo(true, "当前无配置文件", MessageType.Info);

    private bool is_editing_custom_setting_list = false;     //是否正在编辑本地常规化设置
    private bool doing_manage_config_file = false;           //是否管理配置文件

    /// <summary>
    /// 加载关于本地化常规设置配置
    /// </summary>
    private void LoadLocalizationCustomSetting()
    {
        config_file_list.Clear();
        config_file_list.Add("None");
        string[] filePaths = Directory.GetFiles(Configs.Instance.Editor_EditorConfigFolderPath, "*.config");
        config_file_list.AddRange(filePaths.ToList());
        if (!Directory.Exists(Configs.Instance.Editor_EditorConfigFolderPath))
        {
            Directory.CreateDirectory(Configs.Instance.Editor_EditorConfigFolderPath);
            return;
        }

        if (File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
        {
            LocalizationLanguageEditorConfig lle_config = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);
            if (lle_config != default)
            {
                ui_gameobject_list.Clear();
                if (lle_config.gameObjectPathsInProject != null)
                {
                    foreach (var path in lle_config.gameObjectPathsInProject)
                        ui_gameobject_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }

                if (lle_config.languageList != null)
                {
                    ui_language_list.Clear();
                    foreach (var l in lle_config.languageList)
                        ui_language_list.Add(l);
                }
            }
        }
    }

    /// <summary>
    /// 保存关于本地化常规设置配置
    /// </summary>
    private void SaveLocalizationCustomSetting(string path)
    {
        LocalizationLanguageEditorConfig lle_config = new LocalizationLanguageEditorConfig();
        foreach (var o in ui_gameobject_list)
            lle_config.gameObjectPathsInProject.Add(AssetDatabase.GetAssetPath(o));
        lle_config.languageList = ui_language_list;
        lle_config.UpdateLanguageIndexDic();
        XmlUtil.Serialize(lle_config, path);

        if (File.Exists(path))
        {
            LanguageLocalizationConfigsManager.Instance.AddPath(path);
            LanguageLocalizationConfigsManager.Instance.SaveToLocalFile();
        }

    }

    private void LocalizationCustomSettingInitialization()
    {
        LanguageLocalizationConfigsManager llm = LanguageLocalizationConfigsManager.Instance;
        config_file_list.Clear();
        config_file_names_without_extension.Clear();
        config_file_list.Add(llm.None);
        llm.CleanNotExistsFileRecord();
        config_file_list.AddRange(llm.GetPaths().ToArray());
        foreach (var p in config_file_list)
            config_file_names_without_extension.Add(Path.GetFileName(p));

        config_file_select = llm.GetSelectedConfigFileIndex() + 1;

        if(config_file_select==0)
        {
            ui_gameobject_list.Clear();
            ui_language_list.Clear();
        }
        else
        {
            RefreshConfigEditor(llm.GetSelectedName());
        }
        _this.Update();
    }

    private bool RefreshConfigEditor(string configPath)
    {
        if (File.Exists(configPath))
        {
            LocalizationLanguageEditorConfig config = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(configPath);
            if (config != default)
            {
                ui_gameobject_list.Clear();
                if (config.gameObjectPathsInProject != null)
                {
                    foreach (var p in config.gameObjectPathsInProject)
                        ui_gameobject_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>(p));
                }

                if (config.languageList != null)
                {
                    ui_language_list.Clear();
                    foreach (var l in config.languageList)
                        ui_language_list.Add(l);
                }
            }
            else
                return false;
            return true;
        }
        return false;
        
    }

    /// <summary>
    /// 本地化常规设置GUI渲染函数
    /// </summary>
    private void LocalizationCustomSettingMainGUI()
    {
        GEditorGUI.Title("本地化常规设置");
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        config_file_select = EditorGUILayout.Popup(new GUIContent("当前配置"), config_file_select, config_file_names_without_extension.ToArray(), new[] { GUILayout.Width(560), GUILayout.Height(20) });
        if(EditorGUI.EndChangeCheck())
        {
            LanguageLocalizationConfigsManager.Instance.ChangeSelectedPath(config_file_list[config_file_select]);
            LocalizationCustomSettingInitialization();
        }
        EditorGUI.BeginDisabledGroup(is_editing_custom_setting_list);
        if (GEditorGUI.Button("新建", 60, 20))
        {
            is_editing_custom_setting_list = true;
        }
        if (GEditorGUI.Button("配置文件列表", 90, 20))
        {
            if (!doing_manage_config_file)
                doing_manage_config_file = true;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GEditorGUI.Label("绝对路径", 150);
        EditorGUILayout.LabelField(config_file_list[config_file_select]);
        EditorGUILayout.EndHorizontal();
        ConfigFilesManagerGUI(doing_manage_config_file);
        EditorConfigFileContentGUI();
    }

    private void ConfigFilesManagerGUI(bool show)
    {
        if (show)
        {
            if (!LanguageLocalizationConfigsManager.Instance.IsHavePaths()&&LanguageLocalizationConfigsManager.Instance.IsDeleteRecordEmpty())
            {
                no_have_configs_info.Show();
                if (GEditorGUI.Button("退出管理", 60, 20))
                {
                    doing_manage_config_file = false;
                }
            }
            else
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("[子窗口管理]");
                List<string> paths = LanguageLocalizationConfigsManager.Instance.GetPaths();
                for (int index = 0; index < paths.Count; index++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GEditorGUI.Label($"{Path.GetFileName(paths[index])}", 150);
                    EditorGUILayout.TextField(paths[index], new[] { GUILayout.Width(408) });
                    if (GEditorGUI.Button("移除", 60, 20))
                    {
                        LanguageLocalizationConfigsManager.Instance.RemovePath(paths[index]);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if(!LanguageLocalizationConfigsManager.Instance.IsHavePaths())
                    no_have_configs_info.Show();

                EditorGUILayout.BeginHorizontal();
                if(LanguageLocalizationConfigsManager.Instance.IsDeleteRecordEmpty())
                {
                    if (GEditorGUI.Button("退出管理", 60, 20))
                    {
                        doing_manage_config_file = false;
                    }
                }
                else
                {
                    if (GEditorGUI.Button("取消修改", 60, 20))
                    {
                        if (EditorUtility.DisplayDialog("警告", "取消后当前修改将不会保存。\n是否执行取消操作。", "确认[Enter]", "取消[Esc]"))
                        {
                            LanguageLocalizationConfigsManager.Instance.CleanDeleteRecord();
                            doing_manage_config_file = false;
                            LocalizationCustomSettingInitialization();
                        }
                    }

                    if (GEditorGUI.Button("保存修改", 60, 20))
                    {
                        if (EditorUtility.DisplayDialog("警告", "确定修改后将不可撤销。\n是否执行撤销操作。", "确认[Enter]", "取消[Esc]"))
                        {
                            LanguageLocalizationConfigsManager.Instance.SaveToLocalFile();
                            doing_manage_config_file = false;
                            LocalizationCustomSettingInitialization();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.Space(20);
        }

    }

    private void EditorConfigFileContentGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(!is_editing_custom_setting_list);
        EditorGUILayout.PropertyField(ui_gameobject_list_property, new GUIContent("UI预制体列表"), true);
        EditorGUILayout.PropertyField(ui_language_list_property, new GUIContent("语言列表"), true);

        EditorGUILayout.Space(15);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GEditorGUI.Button("取消", 60, 20))
        {
            if (EditorUtility.DisplayDialog("警告", "取消后当前的配置信息将不会保存。\n是否执行取消操作。", "确认[Enter]", "取消[Esc]"))
            {
                is_editing_custom_setting_list = false;
            }
        }
        if (GEditorGUI.Button("保存", 60, 20))
        {
            if (is_editing_custom_setting_list)
            {
                /*
                ChangeCustomConfig();
                SaveLocalizationCustomSetting();
                */
                string path = EditorUtility.SaveFilePanel("保存新的配置到", Configs.Instance.Editor_EditorConfigFolderPath, "New Config", "config");
                if (!path.Equals(""))
                {
                    string configFileName = Path.GetFileNameWithoutExtension(path);
                    SaveLocalizationCustomSetting(path);
                    is_editing_custom_setting_list = !is_editing_custom_setting_list;
                }
                LocalizationCustomSettingInitialization();
            }
            else
            {
                UpdateLanguageIndex();
            }
        }
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();


        if (EditorGUI.EndChangeCheck())
        {
            _this.ApplyModifiedProperties();
            ui_game_object_name_list.Clear();
            foreach (var obj in ui_gameobject_list)
                ui_game_object_name_list.Add(obj.name);
        }
    }
    #endregion

    #region 本地化数据表格编辑器

    [SerializeField] private List<string> ui_game_object_name_list = new List<string>();
    [SerializeField] private Dictionary<int, GameObject> ui_language_elem_dic = new Dictionary<int, GameObject>();
    private int ui_gameobject_selected = 0;
    private int gird_width = 0;


    private LocalizationLanguageEditorConfig editor_setting = new LocalizationLanguageEditorConfig();

    private List<List<string>> ui_table_data = new List<List<string>>();
    private ExcelData ui_excel_data = new ExcelData();
    private Dictionary<string, int> language_index = new Dictionary<string, int>();

    private HelpInfo ui_localization_editor_info = new HelpInfo();







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
                    if (query.Count >= 1)
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
                    ui_localization_editor_info.SetState(false);
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
                    ui_localization_editor_info.SetState(true, $"[{ui_gameobject.name}]未找到LanguageElem组件", MessageType.Warning);
            }
            else
                ui_localization_editor_info.SetState(true, $"[{ui_gameobject.name}]未找到LanguageElem组件", MessageType.Warning);
        }
        else
            ui_localization_editor_info.SetState(true, $"[当前选择的GameObject为空]", MessageType.Warning);
    }
    #endregion

}
