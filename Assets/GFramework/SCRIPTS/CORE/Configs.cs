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
    public string UIConfigFolderPath;

    /*
     * Files Path
     */
    public string UIPrefabsConfigPath;
    public string LocalizationLanguageConfigFilePath;





    /*
     * Editor Config File Path
     */
#if UNITY_EDITOR
    public string Editor_ProjectRootFolderPath;
    public string Editor_LogFolderPath;
    public string Editor_EditorConfigFolderPath;
    public string Editor_LanguageLocalizationConfigFilePath;
#endif

    public Configs()
    {
        InputConfigFolderPath = Application.streamingAssetsPath + "/InputConfigs/";
        UIConfigFolderPath = Application.streamingAssetsPath + "/UIConfigs/";


#if UNITY_EDITOR
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath);
        Editor_ProjectRootFolderPath = di.Parent.Parent.FullName;
        Editor_LogFolderPath = Editor_ProjectRootFolderPath + "/Logs/";
        Editor_EditorConfigFolderPath = Editor_ProjectRootFolderPath + "/EditorConfigs/";


        Editor_LanguageLocalizationConfigFilePath = $"{Editor_EditorConfigFolderPath}language_localization_window_config.xml";
#else
        LogFolderPath = Application.streamingAssetsPath + "/Logs/";
#endif


        UIPrefabsConfigPath = $"{UIConfigFolderPath}ui_panel_prefabs_config.xml";
        LocalizationLanguageConfigFilePath = $"{Application.streamingAssetsPath}/language_localization.xlsx";

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
