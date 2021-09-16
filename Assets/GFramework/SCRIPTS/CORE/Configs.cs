using System;
using UnityEngine;

[Serializable]
public class Configs
{
    public string InputConfigFolderPath;
    public string LogFolderPath;

    public Configs()
    {
        InputConfigFolderPath = Application.streamingAssetsPath + "/InputConfigs/";
        LogFolderPath = Application.streamingAssetsPath + "/Logs/";
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
