using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Framework.DataManager;


[System.Serializable]
public class LocalizationLanguageEditorConfig
{
    /// <summary>
    /// 本地化Excel文件路径信息
    /// </summary>
    public string languageLocalizationExcelFilePath;

    /// <summary>
    /// UI游戏对象路径列表
    /// </summary>
    public List<string> gameObjectPathsInProject;
    
    /// <summary>
    /// 语言列表
    /// </summary>
    public List<string> languageList;

    /// <summary>
    /// 语言列表中的索引
    /// </summary>
    private Dictionary<string, int> language_index;

    public LocalizationLanguageEditorConfig()
    {
        gameObjectPathsInProject = new List<string>();
        languageList = new List<string>();
        language_index = new Dictionary<string, int>();
    }

    public void UpdateLanguageIndexDic()
    {
        language_index.Clear();
        for (int index = 0; index < languageList.Count; index++)
            language_index.Add(languageList[index], index);
    }

    public int GetLanguageIndex(string language)
    {
        if (language_index.ContainsKey(language))
            return language_index[language];
        else
            return -1;
    }
}


/// <summary>
/// 配置编辑器
/// </summary>
[System.Serializable]
public class ConfigFilePaths
{
    public string selectedPaths;
    public List<string> paths;
}

public class LanguageLocalizationConfigsManager
{
    public readonly string None = "None";

    private static LanguageLocalizationConfigsManager _instance;
    private ConfigFilePaths configFilePaths;
    private List<string> deleteRecord;

    public static LanguageLocalizationConfigsManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new LanguageLocalizationConfigsManager();
            return _instance;
        }
    }

    public List<string> GetPaths() => configFilePaths.paths;
    public bool IsHavePaths() => configFilePaths.paths.Count != 0;
    public bool IsDeleteRecordEmpty() => deleteRecord.Count == 0;
    public string GetSelectedName() => configFilePaths.selectedPaths;

    public int GetSelectedConfigFileIndex() => ConfigFileIndex(configFilePaths.selectedPaths);
    public int ConfigFileIndex(string selectFilePath)
    {
        for(int index=0;index<configFilePaths.paths.Count;index++)
        {
            if (selectFilePath == configFilePaths.paths[index])
                return index;
        }
        return -1;
    }

    public LanguageLocalizationConfigsManager()
    {
        deleteRecord = new List<string>();
        configFilePaths = default;
        if (File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
            configFilePaths = XmlUtil.DeserializeFromFile<ConfigFilePaths>(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);
        else
        {
            configFilePaths = new ConfigFilePaths();
            configFilePaths.selectedPaths = "";
            configFilePaths.paths = new List<string>();
            XmlUtil.Serialize(configFilePaths, Configs.Instance.Editor_LanguageLocalizationConfigFilePath);
        }

        CleanNotExistsFileRecord();
    }

    /// <summary>
    /// 添加路径记录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool AddPath(string path)
    {
        if (configFilePaths.paths == null)
            configFilePaths.paths = new List<string>();

        if (configFilePaths.paths.Contains(path))
            return false;

        configFilePaths.paths.Add(path);
        configFilePaths.selectedPaths = path;
        return true;
    }

    /// <summary>
    /// 删除路径记录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool RemovePath(string path)
    {
        if (configFilePaths.paths == null)
        {
            configFilePaths.paths = new List<string>();
            return false;
        }
        if (!configFilePaths.paths.Contains(path))
            return false;

        configFilePaths.paths.Remove(path);
        deleteRecord.Add(path);
        return true;
    }

    public void CleanDeleteRecord()
    {
        deleteRecord.Clear();
        if (File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
            configFilePaths = XmlUtil.DeserializeFromFile<ConfigFilePaths>(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);
    }

    /// <summary>
    /// 将修改保存至本地文件
    /// </summary>
    public void SaveToLocalFile()
    {
        if (!Directory.Exists(Configs.Instance.Editor_EditorConfigFolderPath))
            Directory.CreateDirectory(Configs.Instance.Editor_EditorConfigFolderPath);
        if (File.Exists(Configs.Instance.Editor_LanguageLocalizationConfigFilePath))
            File.Delete(Configs.Instance.Editor_LanguageLocalizationConfigFilePath);

        XmlUtil.Serialize(configFilePaths, Configs.Instance.Editor_LanguageLocalizationConfigFilePath);

        foreach (var p in deleteRecord)
            if (File.Exists(p))
                File.Delete(p);
        deleteRecord.Clear();
    }

    /// <summary>
    /// 清除不存在的文件记录
    /// </summary>
    public List<string> CleanNotExistsFileRecord()
    {
        for(int index=configFilePaths.paths.Count-1;index>=0;index--)
        {
            if (!File.Exists(configFilePaths.paths[index]))
            {
                if (configFilePaths.paths[index] == configFilePaths.selectedPaths)
                    configFilePaths.selectedPaths = "";
                configFilePaths.paths.Remove(configFilePaths.paths[index]);
            }
        }
        SaveToLocalFile();
        return GetPaths();
    }

    public void ChangeSelectedPath(string path)
    {
        if (GetPaths().Contains(path))
            configFilePaths.selectedPaths = path;
        else
            configFilePaths.selectedPaths = "";
        SaveToLocalFile();
    }
}