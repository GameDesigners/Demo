using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Framework.DataManager;

public class HelpInfo
{
    public bool isShow = false;
    public string msg = "";
    public MessageType msgType;
}

public class InputManagerWindow : EditorWindow
{
    private static InputManagerWindow _im_window;
    private Vector2 scrollPos = Vector2.zero;

    private static string system_xbox_input_xml_file_path = "xbox_input_config";
    private static string gameplay_input_config_xml_file_path = "game_input_config";

    private static List<string> registerKeyNameList = new List<string>();
    private static string action_log = "";

    private SerializedObject _serializedObject;
    private GUIStyle title_style;

    [SerializeField,Header("手柄按键的对应Key Code字符串")] 
    private List<GameHandle> handle_btns_list = new List<GameHandle>();
    private SerializedProperty handle_btns_list_Property;

    [SerializeField, Header("手柄遥感对应的Key Code字符串[!!修改时需要同时修改旧版本InputManager的配置]")]
    private List<GameHandleAxis> handle_axis_list = new List<GameHandleAxis>();
    private SerializedProperty handle_axis_list_Property;

    [SerializeField]
    private GameInputConfig GAME_INPUT_CONFIG = new GameInputConfig();
    private SerializedProperty game_input_config_Property;


    HelpInfo exists_file_msg_info = new HelpInfo();
    HelpInfo editor_game_input_config_Info = new HelpInfo();

    protected void OnEnable()
    {
        _serializedObject = new SerializedObject(this);
        handle_btns_list_Property = _serializedObject.FindProperty("handle_btns_list");
        handle_axis_list_Property = _serializedObject.FindProperty("handle_axis_list");
        game_input_config_Property=_serializedObject.FindProperty("GAME_INPUT_CONFIG");

        if (title_style==null)
        {
            title_style = new GUIStyle { fontSize = 20, fontStyle = FontStyle.Bold };
            title_style.normal.textColor = Color.white;
        }

        //更新已注册的按键字符关键字
        CheckRegisteredStringKeyList(out action_log);
    }

    [MenuItem("GFrameworkEditorWindows/InputManagerWindow")]
    private static void Open()
    {
        if (_im_window == null)
        {
            _im_window = GetWindow<InputManagerWindow>();
        }
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("系统配置", title_style, new[] { GUILayout.Height(30) });
        EditorGUILayout.LabelField($"Xml文件夹：{Configs.Instance.InputConfigFolderPath}"/*, new[] { GUILayout.Width(300) }*/);
        EditorGUILayout.LabelField($"Xml文件名：{system_xbox_input_xml_file_path}.xml");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("载入数据", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            string path = $"{Configs.Instance.InputConfigFolderPath}{system_xbox_input_xml_file_path}.xml";
            if (File.Exists(path))
            {
                XBoxOneInputConfig xboxInputConfigs = XmlUtil.DeserializeFromFile<XBoxOneInputConfig>(path);
                handle_btns_list = xboxInputConfigs.HandleKey;
                handle_axis_list = xboxInputConfigs.HandleAxis;
                exists_file_msg_info.isShow = false;
            }
            else
            {
                Debug.Log($"{path}不存在");
                exists_file_msg_info.isShow = true;
                exists_file_msg_info.msg = "加载错误，该路径不存在";
                exists_file_msg_info.msgType = MessageType.Error;
            }
        }

        if (GUILayout.Button("更新数据", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            string path = $"{Configs.Instance.InputConfigFolderPath}{system_xbox_input_xml_file_path}.xml";
            XBoxOneInputConfig xboxInputConfigs = new XBoxOneInputConfig();
            xboxInputConfigs.HandleKey = handle_btns_list;
            xboxInputConfigs.HandleAxis = handle_axis_list;
            XmlUtil.Serialize(xboxInputConfigs, path);
        }

        EditorGUILayout.EndHorizontal();

        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(handle_btns_list_Property, true);
        EditorGUILayout.PropertyField(handle_axis_list_Property, true);
        if (exists_file_msg_info.isShow)
            EditorGUILayout.HelpBox(exists_file_msg_info.msg, exists_file_msg_info.msgType);


        EditorGUILayout.Space(50);
        EditorGUILayout.LabelField("用户自定义Input参数", title_style, new[] { GUILayout.Height(30) });

        EditorGUILayout.LabelField($"Xml文件夹：{Configs.Instance.InputConfigFolderPath}"/*, new[] { GUILayout.Width(300) }*/);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Xml文件名：", new[] { GUILayout.Width(60) });
        gameplay_input_config_xml_file_path = EditorGUILayout.TextField(gameplay_input_config_xml_file_path, new[] { GUILayout.Width(200) });
        EditorGUILayout.LabelField($".xml", new[] { GUILayout.Width(60) });
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("从xml文件中载入", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            string path = $"{Configs.Instance.InputConfigFolderPath}{gameplay_input_config_xml_file_path}.xml";
            if (File.Exists(path))
            {
                GAME_INPUT_CONFIG = XmlUtil.DeserializeFromFile<GameInputConfig>(path);
                editor_game_input_config_Info.isShow = false;
            }
            else
            {
                //Debug.Log($"{path}不存在");
                editor_game_input_config_Info.isShow = true;
                editor_game_input_config_Info.msg = "加载错误，该路径不存在";
                editor_game_input_config_Info.msgType = MessageType.Error;
            }
        }

        if (GUILayout.Button("保存修改(不可撤销)", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            if (CheckRegisteredStringKeyList(out action_log))
            {
                string path = $"{Configs.Instance.InputConfigFolderPath}{gameplay_input_config_xml_file_path}.xml";
                if (File.Exists(path))
                    File.Delete(path);
                XmlUtil.Serialize(GAME_INPUT_CONFIG, path);
            }
        }

        EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(game_input_config_Property, true);
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            CheckRegisteredStringKeyList(out action_log);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("键值表");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(action_log, new[] { GUILayout.Height(200) });
        EditorGUI.EndDisabledGroup();
        if (editor_game_input_config_Info.isShow)
            EditorGUILayout.HelpBox(editor_game_input_config_Info.msg, editor_game_input_config_Info.msgType);
        GUILayout.EndScrollView();
    }

    private bool CheckRegisteredStringKeyList(out string actionlog)
    {
        registerKeyNameList.Clear();
        actionlog = default;
        foreach (var b in GAME_INPUT_CONFIG.handle_btns)
        {
            actionlog = "HANDLE_BTNS\n";
            if (!registerKeyNameList.Contains(b.ActionName))
            {
                registerKeyNameList.Add(b.ActionName);
                actionlog += $"{b.Description}    {b.ActionName}    {b.code}\n";
            }
            else
            {
                editor_game_input_config_Info.isShow = true;
                editor_game_input_config_Info.msg = "存在相同键值[位于handle_btns]";
                editor_game_input_config_Info.msgType = MessageType.Error;
                return false;
            }
                
        }
        foreach (var b in GAME_INPUT_CONFIG.handle_axis)
        {
            actionlog += "\nHANDLE_AXIS\n";
            if (!registerKeyNameList.Contains(b.ActionName))
            {
                registerKeyNameList.Add(b.ActionName);
                actionlog += $"{b.Description}    {b.ActionName}    {b.axis}\n";
            }
            else
            {
                editor_game_input_config_Info.isShow = true;
                editor_game_input_config_Info.msg = "存在相同键值[位于handle_axis]";
                editor_game_input_config_Info.msgType = MessageType.Error;
                return false;
            }

        }

        foreach (var b in GAME_INPUT_CONFIG.keyboard_btns)
        {
            actionlog += "\nKEYBOARD_BTNS\n";
            if (!registerKeyNameList.Contains(b.ActionName))
            {
                registerKeyNameList.Add(b.ActionName);
                actionlog += $"{b.Description}    {b.ActionName}    {b.code}\n";
            }
            else
            {
                editor_game_input_config_Info.isShow = true;
                editor_game_input_config_Info.msg = "存在相同键值[位于keyboard_btns]";
                editor_game_input_config_Info.msgType = MessageType.Error;
                return false;
            }

        }
        editor_game_input_config_Info.isShow = false;
        if (action_log == default)
            action_log = "当前未设置任何键值";
        return true;

    }
}
