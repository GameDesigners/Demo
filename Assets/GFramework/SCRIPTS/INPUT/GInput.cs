using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
using System.IO;

public enum HandleKey
{
    A,
    B,
    X,
    Y,
    LB,
    RB,
    LS,
    RS
}

public enum HandleAxis
{
    LT,
    RT,
    LSVertical,
    LSHorizontal,
    RSVertical,
    RSHorizontal,
    Vertical,
    Horizontal
}

public enum MouseButton : int
{
    LButton,
    MButton,
    RButton
}

[Serializable]
public class GameHandle
{
    public string tips;
    public HandleKey code;
    public string keyString;
}

[Serializable]
public class GameHandleAxis
{
    public string tips;
    public HandleAxis code;
    public string keyString;
}

[Serializable]
public class XBoxOneInputConfig
{
    public List<GameHandle>     HandleKey;
    public List<GameHandleAxis> HandleAxis;
}

[Serializable]
public class GameInputConfig
{
    [Serializable]
    public class KeyBoardButton
    {
        public string Description;
        public string ActionName;
        public KeyCode code;
    }

    [Serializable]
    public class HandleButton
    {
        public string Description;
        public string ActionName;
        public HandleKey code;
    }

    [Serializable]
    public class XHandleAxis
    {
        public string Description;
        public string ActionName;
        public HandleAxis axis;
    }

    public List<KeyBoardButton> keyboard_btns;
    public List<HandleButton> handle_btns;
    public List<XHandleAxis> handle_axis;
}



public class GInput
{
    //System Input Data Part
    private List<GameHandle> handle_btns;
    private List<GameHandleAxis> handle_axis;
    private Dictionary<HandleKey, KeyCode> handle_keycode;
    private Dictionary<HandleAxis, string> handle_axis_maps;

    //Gameplay Input Data Part
    private Dictionary<string, HandleKey> game_input_handle_btn_dic;
    private Dictionary<string, KeyCode> game_input_keyboard_dic;
    private Dictionary<string, HandleAxis> game_input_handle_axis_dic;

    //Modify Part
    private GameInputConfig modify_record;

    private static GInput _instance;
    public static GInput Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GInput();

            }
            return _instance;
        }
        private set { }
    }

    public bool Initialize()
    {
        InitialModifyRecordObject();
        if (!LoadGameHandleKeyConfig()) return false;
        if(!LoadGameInputConfig()) return false;
        return true;
    }

    /// <summary>
    /// 加载游戏手柄按键配置
    /// </summary>
    private bool LoadGameHandleKeyConfig()
    {
        XBoxOneInputConfig config = XmlUtil.DeserializeFromFile<XBoxOneInputConfig>($"{Configs.Instance.InputConfigFolderPath}xbox_input_config.xml");
        if (config != default)
        {
            handle_btns = config.HandleKey;
            handle_axis = config.HandleAxis;


            handle_keycode = new Dictionary<HandleKey, KeyCode>();
            foreach (var b in handle_btns)
                handle_keycode.Add(b.code, (KeyCode)Enum.Parse(typeof(KeyCode), b.keyString));

            handle_axis_maps = new Dictionary<HandleAxis, string>();
            foreach (var a in handle_axis)
                handle_axis_maps.Add(a.code, a.keyString);

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 加载游戏Input配置文件
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool LoadGameInputConfig()
    {
        string path = $"{Configs.Instance.InputConfigFolderPath}game_input_config.xml";
        GameInputConfig config = XmlUtil.DeserializeFromFile<GameInputConfig>(path);
        game_input_handle_btn_dic = new Dictionary<string, HandleKey>();
        game_input_keyboard_dic = new Dictionary<string, KeyCode>();
        game_input_handle_axis_dic = new Dictionary<string, HandleAxis>();

        if (config != default)
        {
            foreach (var b in config.handle_btns)
                game_input_handle_btn_dic.Add(b.ActionName, b.code);
            foreach (var b in config.keyboard_btns)
                game_input_keyboard_dic.Add(b.ActionName, b.code);
            foreach (var b in config.handle_axis)
                game_input_handle_axis_dic.Add(b.ActionName, b.axis);
            return true;
        }
        else
        {
            GDebug.Instance.Error("读取用户输入错误");
            return false;
        }


    }

    private bool SaveGameInputConfig()
    {
        string path = $"{Configs.Instance.InputConfigFolderPath}game_input_config.xml";
        GameInputConfig config = new GameInputConfig();
        config.handle_btns = new List<GameInputConfig.HandleButton>();
        config.keyboard_btns = new List<GameInputConfig.KeyBoardButton>();
        config.handle_axis = new List<GameInputConfig.XHandleAxis>();
        foreach (var b in game_input_handle_btn_dic)
        {
            GameInputConfig.HandleButton tmp = new GameInputConfig.HandleButton();
            tmp.ActionName = b.Key;
            tmp.code = b.Value;
            config.handle_btns.Add(tmp);
        }

        foreach(var b in game_input_keyboard_dic)
        {
            GameInputConfig.KeyBoardButton tmp = new GameInputConfig.KeyBoardButton();
            tmp.ActionName = b.Key;
            tmp.code = b.Value;
            config.keyboard_btns.Add(tmp);
        }

        foreach(var a in game_input_handle_axis_dic)
        {
            GameInputConfig.XHandleAxis tmp = new GameInputConfig.XHandleAxis();
            tmp.ActionName = a.Key;
            tmp.axis = a.Value;
            config.handle_axis.Add(tmp);
        }

        if (File.Exists(path))
            File.Delete(path);
        return XmlUtil.Serialize(config, path);


    }

    private void InitialModifyRecordObject()
    {
        modify_record = new GameInputConfig();
        modify_record.handle_btns = new List<GameInputConfig.HandleButton>();
        modify_record.keyboard_btns = new List<GameInputConfig.KeyBoardButton>();
        modify_record.handle_axis = new List<GameInputConfig.XHandleAxis>();
    }





    public bool GetKeyDown(HandleKey key) => Input.GetKeyDown(handle_keycode[key]);
    public bool GetKeyUp(HandleKey key) => Input.GetKeyUp(handle_keycode[key]);
    public bool GetKey(HandleKey key) => Input.GetKey(handle_keycode[key]);


    public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
    public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);
    public bool GetKey(KeyCode key) => Input.GetKey(key);

    public float GetAxis(HandleAxis axis) => Input.GetAxis(handle_axis_maps[axis]);
    public float GetAxisRaw(HandleAxis axis) => Input.GetAxisRaw(handle_axis_maps[axis]);


    /*
     *  通过ActionName字符串键值获取相关输入
     */

    public KeyCode GetKeyCode(string actionName)
    {
        if (game_input_handle_btn_dic.ContainsKey(actionName))
        {
            HandleKey key;
            game_input_handle_btn_dic.TryGetValue(actionName, out key);
            return handle_keycode[key];
        }
        else if (game_input_keyboard_dic.ContainsKey(actionName))
        {
            KeyCode key;
            game_input_keyboard_dic.TryGetValue(actionName, out key);
            return key;
        }
        else
        {
#if UNITY_EDITOR
            GDebug.Instance.Warn($"输入系统中{actionName}键值不存在");
#endif
            return default;
        }
    }

    public bool GetKeyDown(string actionName)
    {
        KeyCode code = GetKeyCode(actionName);
        if (code == default)
            return false;
        return Input.GetKeyDown(code);
    }
    public bool GetKey(string actionName)
    {
        KeyCode code = GetKeyCode(actionName);
        if (code == default)
            return false;
        return Input.GetKey(code);
    }
    public bool GetKeyUp(string actionName)
    {
        KeyCode code = GetKeyCode(actionName);
        if (code == default)
            return false;
        return Input.GetKeyUp(code);
    }
    public float GetAxis(string actionName)
    {
        if (game_input_handle_axis_dic.ContainsKey(actionName))
        {
            HandleAxis axis;
            game_input_handle_axis_dic.TryGetValue(actionName, out axis);
            return Input.GetAxis(handle_axis_maps[axis]);
        }
        return default;
    }
    public float GetAxisRaw(string actionName)
    {
        if (game_input_handle_axis_dic.ContainsKey(actionName))
        {
            HandleAxis axis;
            game_input_handle_axis_dic.TryGetValue(actionName, out axis);
            return Input.GetAxisRaw(handle_axis_maps[axis]);
        }
        return default;
    }


    public bool GetMouseButtonDown(MouseButton btn)
    {
        switch(btn)
        {
            case MouseButton.LButton:
                return Input.GetMouseButtonDown(0);
            case MouseButton.RButton:
                return Input.GetMouseButtonDown(1);
            case MouseButton.MButton:
                return Input.GetMouseButtonDown(2);
        }
        return false;
    }

    public bool GetMouseButton(MouseButton btn)
    {
        switch (btn)
        {
            case MouseButton.LButton:
                return Input.GetMouseButton(0);
            case MouseButton.RButton:
                return Input.GetMouseButton(1);
            case MouseButton.MButton:
                return Input.GetMouseButton(2);
        }
        return false;
    }

    public bool GetMouseButtonUp(MouseButton btn)
    {
        switch (btn)
        {
            case MouseButton.LButton:
                return Input.GetMouseButtonUp(0);
            case MouseButton.RButton:
                return Input.GetMouseButtonUp(1);
            case MouseButton.MButton:
                return Input.GetMouseButtonUp(2);
        }
        return false;
    }

    public Vector3 GetMousePosition() => Input.mousePosition;

    public void ClearModifyRecord()
    {
        modify_record.handle_btns.Clear();
        modify_record.keyboard_btns.Clear();
        modify_record.handle_axis.Clear();
    }

    public void AddModifyCommand(string actionName,HandleKey key)
    {
        if (!game_input_handle_btn_dic.ContainsKey(actionName))
            return;

        var tmp = new GameInputConfig.HandleButton();
        tmp.ActionName = actionName;
        tmp.code = key;
        modify_record.handle_btns.Add(tmp);
    }

    public void AddModifyCommand(string actionName,KeyCode key)
    {
        if (!game_input_keyboard_dic.ContainsKey(actionName))
            return;

        var tmp = new GameInputConfig.KeyBoardButton();
        tmp.ActionName = actionName;
        tmp.code = key;
        modify_record.keyboard_btns.Add(tmp);
    }

    public void AddModifyCommand(string actionName,HandleAxis axis)
    {
        if (!game_input_handle_axis_dic.ContainsKey(actionName))
            return;

        var tmp = new GameInputConfig.XHandleAxis();
        tmp.ActionName = actionName;
        tmp.axis = axis;
        modify_record.handle_axis.Add(tmp);
    }


    public int CommitModifyRecord()
    {
        int modify_num = 0;
        foreach(var b in modify_record.handle_btns)
        {
            if (game_input_handle_btn_dic.ContainsKey(b.ActionName))
            {
                if(game_input_handle_btn_dic[b.ActionName] != b.code)
                {
                    game_input_handle_btn_dic[b.ActionName] = b.code;
                    modify_num++;
                }
            }
        }

        foreach(var b in modify_record.keyboard_btns)
        {
            if (game_input_keyboard_dic.ContainsKey(b.ActionName))
            {
                if (game_input_keyboard_dic[b.ActionName] != b.code)
                {
                    game_input_keyboard_dic[b.ActionName] = b.code;
                    modify_num++;
                }
            }
        }

        foreach (var b in modify_record.handle_axis)
        {
            if (game_input_handle_axis_dic.ContainsKey(b.ActionName))
            {
                if (game_input_handle_axis_dic[b.ActionName] != b.axis)
                {
                    game_input_handle_axis_dic[b.ActionName] = b.axis;
                    modify_num++;
                }
            }
        }

        SaveGameInputConfig();
        return modify_num;
    }
}
