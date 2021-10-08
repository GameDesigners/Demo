using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMessage
{
    /// <summary>
    /// NetMessage核心信息类
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// 长度信息数据类型（分为Int16和Int32）
        /// 需要与客户端保持一致
        /// </summary>
        public static readonly LengthMsgDataType LMDT= LengthMsgDataType.INT16;
    }
}
