using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 博客参考：https://www.cnblogs.com/liuqifeng/p/9148810.html
/// </summary>
public class DataTimeUtil
{
    public static string GetTimeStampForFileName() => DateTime.Now.ToString("yyyy-MM-dd HH时-mm分-ss秒");
    public static string GetDateTimeHMS() => DateTime.Now.ToString("HH:mm:ss");
}
