using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;

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
        public string ActionName;
        public KeyCode code;
    }

    [Serializable]
    public class HandleButton
    {
        public string ActionName;
        public HandleKey code;
    }

    [Serializable]
    public class HandleAxis
    {
        public string ActionName;
        public HandleAxis axis;
    }

    public List<KeyBoardButton> keyboard_btns;
    public List<HandleButton> handle_btns;
    public List<HandleAxis> handle_axis;
}



public class GInput
{
    private List<GameHandle> handle_btns;
    private List<GameHandleAxis> handle_axis;
    private Dictionary<HandleKey,KeyCode> handle_keycode;
    private Dictionary<HandleAxis, string> handle_axis_maps;

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

    public GInput()
    {
        LoadGameHandleKeyConfig();
    }

    /// <summary>
    /// 加载游戏手柄按键配置
    /// </summary>
    private void LoadGameHandleKeyConfig()
    {
        XBoxOneInputConfig config= XmlUtil.DeserializeFromFile<XBoxOneInputConfig>($"{Configs.Instance.InputConfigFolderPath}xbox_input_config.xml");
        handle_btns = config.HandleKey;
        handle_axis = config.HandleAxis;


        handle_keycode = new Dictionary<HandleKey, KeyCode>();
        foreach (var b in handle_btns)
            handle_keycode.Add(b.code, (KeyCode)Enum.Parse(typeof(KeyCode), b.keyString));

        handle_axis_maps = new Dictionary<HandleAxis, string>();
        foreach (var a in handle_axis)
            handle_axis_maps.Add(a.code, a.keyString);
    }






    public bool GetKeyDown(HandleKey key) => Input.GetKeyDown(handle_keycode[key]);
    public bool GetKeyUp(HandleKey key) => Input.GetKeyUp(handle_keycode[key]);
    public bool GetKey(HandleKey key) => Input.GetKey(handle_keycode[key]);


    public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
    public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);
    public bool GetKey(KeyCode key) => Input.GetKey(key);

    public float GetAxis(HandleAxis axis) => Input.GetAxis(handle_axis_maps[axis]);
}
