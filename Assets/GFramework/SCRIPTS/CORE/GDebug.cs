using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Framework.DataManager;

public class GDebug
{
    public int max_log_count = 10;


    private string path;
    private const int inner_stack_frame_num = 4;  //在此类中获取调用日志接口的偏移值

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

    private enum LogType
    {
        log,
        warn,
        error
    }

    private GDebug()
    {
        if(!Directory.Exists(Configs.Instance.LogFolderPath))
            Directory.CreateDirectory(Configs.Instance.LogFolderPath);
        path = $"{Configs.Instance.LogFolderPath}[log]{DataTimeUtil.GetTimeStampForFileName()}.txt";
        ClearLogFiles();
    }

    /// <summary>
    /// 打印通用日志
    /// </summary>
    /// <param name="_log">日志字符串</param>
    /// <param name="showCodeStack">是否显示调用的栈帧信息</param>
    /// <param name="showInUnityEditorDebug">是否在Unity编辑器的Debug系统中输出</param>
    public void Log(string _log, bool showCodeStack = true, bool showInUnityEditorDebug = true)
    {
        PrintLog(LogType.log, _log, showCodeStack, showInUnityEditorDebug);
    }

    /// <summary>
    /// 打印警告日志
    /// </summary>
    /// <param name="_log">日志字符串</param>
    /// <param name="showCodeStack">是否显示调用的栈帧信息</param>
    /// <param name="showInUnityEditorDebug">是否在Unity编辑器的Debug系统中输出</param>
    public void Warn(string _log, bool showCodeStack = true,bool showInUnityEditorDebug=true)
    {
        PrintLog(LogType.warn, _log, showCodeStack, showInUnityEditorDebug);
    }

    /// <summary>
    /// 打印错误日志
    /// </summary>
    /// <param name="_log">日志字符串</param>
    /// <param name="showCodeStack">是否显示调用的栈帧信息</param>
    /// <param name="showInUnityEditorDebug">是否在Unity编辑器的Debug系统中输出</param>
    public void Error(string _log, bool showCodeStack = true, bool showInUnityEditorDebug = true)
    {
        PrintLog(LogType.error, _log, showCodeStack, showInUnityEditorDebug);
    }

    /// <summary>
    /// 日志输出函数
    /// </summary>
    /// <param name="type">日志类型</param>
    /// <param name="_log">日志内容字符串</param>
    /// <param name="showCodeStack">是否显示调用栈帧的信息</param>
    /// <param name="showInUnityEditorDebug">是否在Unity编辑器的Debug系统中输出</param>
    private void PrintLog(LogType type, string _log, bool showCodeStack, bool showInUnityEditorDebug)
    {
        string log = $"[{DataTimeUtil.GetDateTimeHMS()} {type}]";
        if (showInUnityEditorDebug)
        {
#if UNITY_EDITOR
            if (type == LogType.log)
                Debug.Log($"{log}\n{_log}\n\n");
            else if (type == LogType.warn)
                Debug.LogWarning($"{log}\n{_log}\n\n");
            else if(type==LogType.error)
                Debug.LogError($"{log}\n{_log}\n\n");
#endif
        }
        if (showCodeStack)
            log += GetPrevStackInfo();
        log += $"\n{_log}\n";
        GFileStream.AppendWriteString(path, log);
    }

    /// <summary>
    /// 获取堆栈栈帧信息
    /// </summary>
    /// <returns></returns>
    private string GetPrevStackInfo()
    {
        System.Diagnostics.StackTrace stack = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.StackFrame frame;
        string code_file_name;
        string msg = "";
        if (stack.FrameCount >= inner_stack_frame_num)
        {
            frame = stack.GetFrame(inner_stack_frame_num-1);
            code_file_name = Path.GetFileName(frame.GetFileName());
        }
        else if (stack.FrameCount == inner_stack_frame_num-1)
        {
            frame = stack.GetFrame(inner_stack_frame_num-2);
            code_file_name = Path.GetFileName(frame.GetFileName());
            msg = "< not find the pre stack frame >";
        }
        else
            return " @stack frame not find";
        
        return $" @File:{code_file_name}  Line:{frame.GetFileLineNumber()} {msg}";
    }

    /// <summary>
    /// 清除日志信息
    /// </summary>
    private void ClearLogFiles()
    {
        DirectoryInfo folder = new DirectoryInfo(Configs.Instance.LogFolderPath);
        FileInfo[] files = folder.GetFiles("*.txt");
        var filter = (
                       from r in files
                       orderby r.CreationTime
                       select r
                     ).Take(files.Length - max_log_count + 1);  //+1表示当前启动未创建的日志文件

        foreach (var f in filter)
            File.Delete(f.ToString());
    }
}
