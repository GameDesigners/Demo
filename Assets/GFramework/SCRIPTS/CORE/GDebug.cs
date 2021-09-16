using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Framework.DataManager;

public class GDebug
{
    public int max_log_count = 10;


    private string path;

    private static GDebug _instance;
    public static GDebug Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GDebug();
            return _instance;
        }
    }

    private GDebug()
    {
        if(!Directory.Exists(Configs.Instance.LogFolderPath))
            Directory.CreateDirectory(Configs.Instance.LogFolderPath);
        path = $"{Configs.Instance.LogFolderPath}[log]{DataTimeUtil.GetTimeStampForFileName()}.txt";
        //if (!File.Exists(path))
        //    File.Create(path);//创建日志文件
    }

    public void Log(string _log, bool showCodeStack = true, bool showInUnityEditorDebug = true)
    {
        string log = $"[{DataTimeUtil.GetDateTimeHMS()} log]";
        if (showInUnityEditorDebug)
        {
#if UNITY_EDITOR
            Debug.Log($"{log}\n{_log}\n");
#endif
        }
        if (showCodeStack)
            log += GetPrevStackInfo();
        log += $"\n{_log}\n";
        GFileStream.AppendWriteString(path, log);

    }

    private string GetPrevStackInfo()
    {
        System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.StackFrame frame;
        string code_file_name;
        string msg = "";
        if (stack.FrameCount > 2)
        {
            frame = stack.GetFrame(2);
            code_file_name = Path.GetFileName(frame.GetFileName());
        }
        else if (stack.FrameCount == 2)
        {
            frame = stack.GetFrame(1);
            code_file_name = Path.GetFileName(frame.GetFileName());
            msg = "< not find the pre stack frame >";
        }
        else
            return " @stack frame not find";
        
        return $" @File:{code_file_name}  Line:{frame.GetFileLineNumber()} {msg}";
    }


}
