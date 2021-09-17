using System;
using UnityEngine;

[Serializable]
public class Configs
{
    /*
     * Folder Path
     */
    public string InputConfigFolderPath;
    public string LogFolderPath;
    public string UIConfigFolderPath;

    /*
     * Files Path
     */
    public string UIPrefabsConfigPath;

    public Configs()
    {
        InputConfigFolderPath = Application.streamingAssetsPath + "/InputConfigs/";
        LogFolderPath = Application.streamingAssetsPath + "/Logs/";
        UIConfigFolderPath = Application.streamingAssetsPath + "/UIConfigs/";

        UIPrefabsConfigPath= $"{UIConfigFolderPath}ui_panel_prefabs_config.xml";
    }

    private static Configs _instance;
    public static Configs Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Configs();
            return _instance;
        }
    }
}
