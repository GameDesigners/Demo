using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configs
{
    public string InputConfigFolderPath;

    public Configs()
    {
        InputConfigFolderPath = Application.streamingAssetsPath + "/InputConfigs/";
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
