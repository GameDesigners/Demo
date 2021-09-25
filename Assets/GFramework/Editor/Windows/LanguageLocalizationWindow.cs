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
        //UpdateLanguageIndex();
        //ChangeCustomConfig();
        UpdateLocalizationDataTableEditorWindowInitialization();

        Debug.Log("Call OnEnable()");
    }
    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        LocalizationCustomSettingMainGUI();
        LocalizationDataTableEditorGUI();
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
    /// 保存关于本地化常规设置配置
    /// </summary>
    private void SaveLocalizationCustomSetting(string path)
    {
        LocalizationLanguageEditorConfig lle_config = new LocalizationLanguageEditorConfig();
        foreach (var o in ui_gameobject_list)
            lle_config.gameObjectPathsInProject.Add(AssetDatabase.GetAssetPath(o));
        lle_config.languageList = ui_language_list;
        lle_config.UpdateLanguageIndexDic();
        lle_config.languageLocalizationExcelFilePath = LanguageLocalizationConfigsManager.Instance.GetRelativePath(CreateExcelTemplateFileForConfig(path, lle_config));
        if (File.Exists(path))
            File.Delete(path);
        XmlUtil.Serialize(lle_config, path);

        if (File.Exists(path))
        {
            LanguageLocalizationConfigsManager.Instance.AddPath(path);
            LanguageLocalizationConfigsManager.Instance.SaveToLocalFile();
        }
    }

    /// <summary>
    /// 初始化本地化常规设置窗口的数据
    /// </summary>
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

        if (config_file_select == 0)
        {
            ui_gameobject_list.Clear();
            ui_language_list.Clear();
        }
        else
        {
            RefreshConfigEditor(llm.GetSelectedPath());
        }
        _this.Update();
    }

    /// <summary>
    /// 刷新本地化常规设置配置编辑器的GUI数据
    /// </summary>
    /// <param name="configPath"></param>
    /// <returns></returns>
    private bool RefreshConfigEditor(string configPath)
    {
        if (File.Exists(configPath))
        {
            LocalizationLanguageEditorConfig config = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(configPath);
            config.languageLocalizationExcelFilePath = LanguageLocalizationConfigsManager.Instance.GetRelativePath(CreateExcelTemplateFileForConfig(configPath, config));
            if (File.Exists(configPath))
                File.Delete(configPath);
            XmlUtil.Serialize(config, configPath);

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
    /// 针对配置文件创建对应的Excel文件
    /// </summary>
    /// <param name="configFileName">配置文件的名称</param>
    /// <returns></returns>
    private string CreateExcelTemplateFileForConfig(string configFilePath,LocalizationLanguageEditorConfig llec)
    {
        if (!Directory.Exists(Configs.Instance.LanguageLocalizationFolderPath))
            Directory.CreateDirectory(Configs.Instance.LanguageLocalizationFolderPath);

        ExcelData data = new ExcelData();
        string path = $"{Configs.Instance.LanguageLocalizationFolderPath}[ProjectRoot%{LanguageLocalizationConfigsManager.Instance.GetRelativePath(configFilePath).Replace("/","%").Replace(":","-")}]language_localization_excel.xlsx";
        if(!File.Exists(path)&& llec!=null)
        {
            foreach (var s in llec.gameObjectPathsInProject)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(s);
                GUILocalization[] comps = obj.GetComponentsInChildren<GUILocalization>();
                Sheet sheetData = new Sheet();
                //写入表头
                List<string> sheet_title = new List<string> { "ID", "KEY" };
                sheet_title.AddRange(llec.languageList);
                sheetData.AddTtile(sheet_title);

                //写入组件的数据
                if (comps.Length > 0)
                {
                    for(int i=0;i<comps.Length;i++)
                    {
                        List<string> row = new List<string>();
                        row.Add($"[{i}]");
                        comps[i].key = $"{comps[i].gameObject.name}[{i}]";
                        row.Add(comps[i].key);
                        EditorUtility.SetDirty(obj);
                        AssetDatabase.SaveAssets();
                        for (int j = 0; j < llec.languageList.Count; j++)
                            row.Add("");
                        sheetData.AddRow(row);
                    }
                }
                data.Add(obj.name, sheetData);

                if (llec.languageLocalizationExcelFilePath == null)
                {
                    llec.languageLocalizationExcelFilePath = LanguageLocalizationConfigsManager.Instance.GetRelativePath(path);
                    Debug.Log($"GetRelativePath:{llec.languageLocalizationExcelFilePath}");
                    XmlUtil.Serialize(llec, LanguageLocalizationConfigsManager.Instance.GetSelectedPath());
                }
            }
            ExcelUtil.Write(path, data);
            AssetDatabase.Refresh();
        }
        return path;
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
        if (EditorGUI.EndChangeCheck())
        {
            LanguageLocalizationConfigsManager.Instance.ChangeSelectedPath(config_file_list[config_file_select]);
            LocalizationCustomSettingInitialization();
            RefreshLocalizationLanguageDataTable();
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

    /// <summary>
    /// 配置文件管理器GUI
    /// </summary>
    /// <param name="show"></param>
    private void ConfigFilesManagerGUI(bool show)
    {
        if (show)
        {
            if (!LanguageLocalizationConfigsManager.Instance.IsHavePaths() && LanguageLocalizationConfigsManager.Instance.IsDeleteRecordEmpty())
            {
                no_have_configs_info.Show();
                EditorGUILayout.BeginHorizontal();
                if (GEditorGUI.Button("手动添加", 60, 20))
                {
                    string path = EditorUtility.OpenFilePanel("选择您添加的配置文件", Configs.Instance.Editor_EditorConfigFolderPath, "config");
                    if(!LanguageLocalizationConfigsManager.Instance.IsInProjectFolder(path))
                        GDebug.Instance.Error($"所选文件的路径:{path}不在项目文件夹中。");
                    else if (path != "")
                    {
                        var parseResult = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(path);
                        if (parseResult == default)
                            GDebug.Instance.Error($"所选文件path:{path}格式不正确。");
                        else
                            LanguageLocalizationConfigsManager.Instance.AddPath(path);
                    }

                }

                if (GEditorGUI.Button("退出管理", 60, 20))
                {
                    doing_manage_config_file = false;
                    LocalizationCustomSettingInitialization();
                }
                EditorGUILayout.EndHorizontal();
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

                if (!LanguageLocalizationConfigsManager.Instance.IsHavePaths())
                    no_have_configs_info.Show();

                EditorGUILayout.BeginHorizontal();
                if (GEditorGUI.Button("手动添加", 60, 20))
                {
                    string path = EditorUtility.OpenFilePanel("选择您添加的配置文件", Configs.Instance.Editor_EditorConfigFolderPath, "config");
                    if (!LanguageLocalizationConfigsManager.Instance.IsInProjectFolder(path))
                        GDebug.Instance.Error($"所选文件的路径:{path}不在项目文件夹中。");
                    else if (path != "")
                    {
                        var parseResult = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(path);
                        if (parseResult == default)
                            GDebug.Instance.Error($"所选文件path:{path}格式不正确。");
                        else
                            LanguageLocalizationConfigsManager.Instance.AddPath(path);
                    }

                }

                if (LanguageLocalizationConfigsManager.Instance.IsDeleteRecordEmpty())
                {
                    if (GEditorGUI.Button("退出管理", 60, 20))
                    {
                        doing_manage_config_file = false;
                        LocalizationCustomSettingInitialization();
                    }
                }
                else
                {
                    if (GEditorGUI.Button("取消删除", 60, 20))
                    {
                        if (EditorUtility.DisplayDialog("警告", "取消后当前修改将不会保存。\n是否执行取消操作。", "确认[Enter]", "取消[Esc]"))
                        {
                            LanguageLocalizationConfigsManager.Instance.CleanDeleteRecord();
                            doing_manage_config_file = false;
                            LocalizationCustomSettingInitialization();
                        }
                    }

                    if (GEditorGUI.Button("保存删除", 60, 20))
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

    /// <summary>
    /// 配置文件元素编辑器GUI
    /// </summary>
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
                if (!LanguageLocalizationConfigsManager.Instance.IsInProjectFolder(path))
                    GDebug.Instance.Error($"所选文件的路径:{path}不在项目文件夹中。");
                else if (path != "")
                {
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
        }
    }
    #endregion

    #region 本地化数据表格编辑器

    [SerializeField] private List<string> ui_game_object_name_list = new List<string>();
    [SerializeField] private Dictionary<int, GameObject> ui_language_elem_dic = new Dictionary<int, GameObject>();

    private int ui_gameobject_selected = 0;
    private int gird_width = 0;


    private Dictionary<string, EditorExcelRowData> rowDic = new Dictionary<string, EditorExcelRowData>();

    private ExcelData ui_excel_data = new ExcelData();
    private Sheet excel_sheet = new Sheet();

    private Dictionary<string, int> language_index = new Dictionary<string, int>();

    private HelpInfo ui_localization_editor_info = new HelpInfo();


    private void UpdateLocalizationDataTableEditorWindowInitialization()
    {
        ui_game_object_name_list.Clear();
        foreach (var obj in ui_gameobject_list)
            ui_game_object_name_list.Add(obj.name);

        RefreshLocalizationLanguageDataTable();
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
            Sheet data = new Sheet();
            data.AddTtile(new List<string> { "KEY" });
            data[0].AddRange(ui_language_list);

            GUILocalization[] locals = obj.GetComponentsInChildren<GUILocalization>();
            if (ui_excel_data.ContainsKey(obj.name))
            {
                List<List<string>> tb = ui_excel_data[obj.name].SheetData();
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
                    data.AddRow(row);
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

        RefreshLocalizationLanguageDataTable();
    }

    /// <summary>
    /// 刷新表格编辑器
    /// </summary>
    private void RefreshLocalizationLanguageDataTable()
    {
        ui_language_elem_dic.Clear();

        rowDic.Clear();
        excel_sheet.Clear();

        //获取当前编辑的物体
        GameObject ui_gameobject = null;
        if (ui_gameobject_list.Count > ui_gameobject_selected)
            ui_gameobject = ui_gameobject_list[ui_gameobject_selected];

        var llce = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(LanguageLocalizationConfigsManager.Instance.GetSelectedPath());
        //Debug.Log($"现在加载的CONFIGS是：{LanguageLocalizationConfigsManager.Instance.GetSelectedPath()}");
        //读取Excel数据
        ui_excel_data = default;
        if (llce != default && ui_gameobject != null)
        {
            //Debug.Log($"现在加载的EXCELS是：{LanguageLocalizationConfigsManager.Instance.GetFullFilePathBaseOnProjectRootPath(llce.languageLocalizationExcelFilePath)}");
            ui_excel_data = ExcelUtil.Read(LanguageLocalizationConfigsManager.Instance.GetFullFilePathBaseOnProjectRootPath(llce.languageLocalizationExcelFilePath));
            if (ui_excel_data != default)
            {
                string sheetName = ui_gameobject.name;
                List<List<string>> sheetData = null;
                if (ui_excel_data.ContainsKey(sheetName))
                {
                    sheetData = ui_excel_data[sheetName].SheetData();
                    foreach (var sd in sheetData)
                    {
                        rowDic.Add(sd[0], new EditorExcelRowData(null, sd));
                    }
                }
            }
        }

        //更新表头
        List<string> title_list = new List<string> { "ID", "KEY" };
        title_list.AddRange(ui_language_list);
        excel_sheet.AddTtile(title_list);
        Debug.Log($"excel:{excel_sheet.GetColumnCount()}  ; title_list:{title_list.Count}");

        List<EditorExcelRowData> newData = new List<EditorExcelRowData>();
        //更新数据至表格编辑器
        if (ui_gameobject != null)
        {
            GUILocalization[] locals = ui_gameobject.GetComponentsInChildren<GUILocalization>();
            if (locals != null)
            {
                if (locals.Length != 0)
                {
                    ui_localization_editor_info.SetState(false);
                    for (int i = 0; i < locals.Length; i++)
                    {
                        //ui_language_elem_dic.Add(excel_sheet.Count(), locals[i].gameObject);

                        List<string> row = new List<string>();
                        if (rowDic.ContainsKey(locals[i].key))
                        {
                            rowDic[locals[i].key].comp = locals[i];
                            row = rowDic[locals[i].key].row_data;
                            excel_sheet.AddRow(row);
                        }
                        else
                            newData.Add(new EditorExcelRowData(locals[i], null));
                    }
                    _this.Update();
                }
                else
                    ui_localization_editor_info.SetState(true, $"[{ui_gameobject.name}]未找到LanguageElem组件", MessageType.Warning);

                //添加表格中未存在的数据
                if(newData.Count>0)
                {
                    for (int i = 0; i < newData.Count; i++)
                    {
                        List<string> newRow = new List<string>();
                        newRow.Add($"[{rowDic.Count + i - 1}]");
                        newRow.Add($"{newData[i].comp.gameObject.name}[{rowDic.Count + i - 1}]");
                        for (int j = 0; j < ui_language_list.Count; j++)
                            newRow.Add("");
                        Debug.Log(excel_sheet.AddRow(newRow));
                        newData[i].row_data = newRow;
                        if(rowDic.ContainsKey(newRow[0]))
                        {
                            rowDic[newRow[0]] = newData[i];
                        }
                        else
                        {
                            rowDic.Add(newRow[0], newData[i]);
                        }
                    }

                    //将新增的数据添加到Excel表格中
                    if (ui_excel_data == default)  //本地文件为空
                    {
                        ui_excel_data = new ExcelData();
                        ui_excel_data.Add(ui_gameobject.name, excel_sheet);
                    }
                    else
                        ui_excel_data[ui_gameobject.name] = excel_sheet;

                    ExcelUtil.Write(LanguageLocalizationConfigsManager.Instance.GetFullFilePathBaseOnProjectRootPath(llce.languageLocalizationExcelFilePath), ui_excel_data);
                }
            }
            else
                ui_localization_editor_info.SetState(true, $"[{ui_gameobject.name}]未找到LanguageElem组件", MessageType.Warning);
        }
        else
            ui_localization_editor_info.SetState(true, $"[当前选择的GameObject为空]", MessageType.Warning);
    }


    /// <summary>
    /// 表格编辑器GUI
    /// </summary>
    private void LocalizationDataTableEditorGUI()
    {
        gird_width = ((int)position.width-40) / (ui_language_list.Count + 2) - 5;

        GEditorGUI.Title("本地化数据表格编辑器", height: 30);


        EditorGUI.BeginChangeCheck();
        ui_gameobject_selected = EditorGUILayout.Popup(new GUIContent("UI对象"), ui_gameobject_selected, ui_game_object_name_list.ToArray(), new[] { GUILayout.Width(720), GUILayout.Height(20) });
        if (EditorGUI.EndChangeCheck())
        {
            RefreshLocalizationLanguageDataTable();
            _this.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        if (!ui_localization_editor_info.Show())
        {
            //if (excel_sheet.Count != 0)
            //    if (excel_sheet[0].Count != 0)
            //        Debug.Log($"cols:{excel_sheet.Count},row:{excel_sheet[0].Count}");
            EditorGUILayout.Space(10);
            for (int i = 0; i < excel_sheet.GetRowCount(); i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (i == 0)
                {
                    EditorGUILayout.LabelField("ID", EditorGUIStyles.Instance.GirdTexMidAligntBoldStyle, new[] { GUILayout.Height(15), GUILayout.Width(40) });
                    EditorGUILayout.LabelField("GameObject", EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Height(15), GUILayout.Width(gird_width) });
                    for (int j = 0; j < excel_sheet[i].Count; j++)
                        EditorGUILayout.LabelField(excel_sheet[i][j], EditorGUIStyles.Instance.GirdTextBoldStyle, new[] { GUILayout.Height(15), GUILayout.Width(gird_width) });
                }
                else
                {
                    GameObject go = null;
                    if (rowDic.ContainsKey(excel_sheet[i][0]))
                    {
                        if (rowDic[excel_sheet[i][0]].comp != null)
                            go = rowDic[excel_sheet[i][0]].comp.gameObject;
                        else
                        {
                            RefreshLocalizationLanguageDataTable();
                            return;
                        }
                    }
                    EditorGUILayout.LabelField("[100]", EditorGUIStyles.Instance.GirdTexMidAligntBoldStyle, new[] { GUILayout.Height(40), GUILayout.Width(40) });
                    EditorGUILayout.ObjectField(go, typeof(GameObject),true, new[] { GUILayout.Height(40), GUILayout.Width(gird_width) });
                    for (int j = 0; j < excel_sheet[i].Count; j++)
                        excel_sheet[i][j] = EditorGUILayout.TextArea(excel_sheet[i][j], new[] { GUILayout.Height(40), GUILayout.Width(gird_width) });

                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GEditorGUI.Button("刷新",60,20))
            {
                RefreshLocalizationLanguageDataTable();
            }
            if (GEditorGUI.Button("保存修改", 60,20))
            {
                if (EditorUtility.DisplayDialog("警告", "此操作将会写入持久化数据\n请确认是否保存修改。", "确认", "取消"))
                {
                    //ExcelUtil.Write(Application.streamingAssetsPath + "/language_localization.xlsx", excel_sheet, ui_game_object_name_list[ui_gameobject_selected]);
                    string name = ui_game_object_name_list[ui_gameobject_selected];
                    if (ui_excel_data.ContainsKey(name))
                        ui_excel_data[name] = excel_sheet;
                    var llce = XmlUtil.DeserializeFromFile<LocalizationLanguageEditorConfig>(LanguageLocalizationConfigsManager.Instance.GetSelectedPath());
                    if (llce != default)
                        ExcelUtil.Write(LanguageLocalizationConfigsManager.Instance.GetFullFilePathBaseOnProjectRootPath(llce.languageLocalizationExcelFilePath), ui_excel_data);


                    foreach(var r in rowDic)
                        EditorUtility.SetDirty(r.Value.comp.gameObject);
                    AssetDatabase.SaveAssets();
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
    }
    #endregion

}
