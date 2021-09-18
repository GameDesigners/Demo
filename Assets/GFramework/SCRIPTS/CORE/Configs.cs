using System;
using System.IO;
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
        UIConfigFolderPath = Application.streamingAssetsPath + "/UIConfigs/";


#if UNITY_EDITOR
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath);
        string preDirectory = di.Parent.Parent.FullName;
        LogFolderPath = preDirectory + "/Logs/";
#else
        LogFolderPath = Application.streamingAssetsPath + "/Logs/";
#endif


        UIPrefabsConfigPath = $"{UIConfigFolderPath}ui_panel_prefabs_config.xml";
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
