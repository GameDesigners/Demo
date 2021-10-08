using System;
using System.Collections.Generic;
using System.Text;

namespace ServerProgram
{
    /// <summary>
    /// 工具类
    /// </summary>
    class Utils
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns>返回至1970-1-1至今的时间差</returns>
        public static long GetTimerStamp()=> new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }
}
