using System;
using System.Linq;
using Framework.DataManager;

namespace Framework.GNetwork
{
    /// <summary>
    /// 客户连接状态
    /// </summary>
    public enum ClientConnectState
    {
        Disconnect,
        Connecting,
        Closing,
        Connected,
    }

    /// <summary>
    /// 网络事件类型
    /// </summary>
    public enum NetEvent
    {
        ConnectSucc,
        ConnectFail,
        Close
    }

    /// <summary>
    /// 信息事件
    /// </summary>
    public enum ProtoName
    {
        MsgHeartBeatPing,      //客户端发往服务端的心跳信号
        MsgHeartBeatPong,      //服务端法网客户端的心跳信号
    }

    /// <summary>
    /// 长度信息变量类型
    /// </summary>
    public enum LengthMsgDataType : int
    {
        INT16 = 2,
        INT32 = 4
    }

    /// <summary>
    /// 信息基类
    /// </summary>
    [Serializable]
    public class BaseMsg
    {
        public string protoName = ""; 

        public static byte[] Encode<T>(T msg) where T : class
        {
            string json = JsonUtil.Serialize(msg);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public static BaseMsg Decode<T>(byte[] bytes,int offset,int count) where T : class
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            BaseMsg msg = JsonUtil.DeserializeFromString<T>(json) as BaseMsg;
            return msg;
        }

        /// <summary>
        /// 根据实例获取发送信息的字节包
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="msg">信息实例</param>
        /// <returns>打包字节</returns>
        public static ByteBuffer GetBytesPackage<T>(string protoName,T msg) where T : BaseMsg
        {
            byte[] data = Encode(msg);

            byte[] len_msg;
            if (GNetworkManager.LMDT == LengthMsgDataType.INT16)
            {
                
                short type_str_length = (short)protoName.Length;
                short msg_length = (short)(2 + type_str_length+ data.Length);
                len_msg = BitConverter.GetBytes(msg_length);
                len_msg = len_msg.Concat(BitConverter.GetBytes(type_str_length)).ToArray();
            }
            else
            {
                int msg_length = (int)data.Length;
                int type_str_length = (int)protoName.Length;
                len_msg = BitConverter.GetBytes(msg_length);
                len_msg = len_msg.Concat(BitConverter.GetBytes(type_str_length)).ToArray();
            }
            byte[] protoNameBytes = System.Text.Encoding.UTF8.GetBytes(protoName);
            return new ByteBuffer(len_msg.Concat(protoNameBytes).ToArray().Concat(data).ToArray());
        }

        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="data">实际的json数据</param>
        /// <param name="msgSymbolLength">信息标志的长度</param>
        /// <param name="typeName">[out]类型名称</param>
        /// <returns>解析出来的实例【装箱成根父类】</returns>
        public static BaseMsg ParseBytesPage(byte[] data, int msgSymbolLength,out string typeName)
        {
            typeName = "";
            if (data.Length < (int)GNetworkManager.LMDT * 2)
            {
                GDebug.Instance.Error("解析的数据包不完整。");
                return default;
            }
            byte[] typeBytes = new byte[msgSymbolLength];
            Array.Copy(data, 0, typeBytes, 0, msgSymbolLength);
            typeName = System.Text.Encoding.UTF8.GetString(typeBytes);
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                byte[] jsonBytes = new byte[data.Length - msgSymbolLength];
                Array.Copy(data, msgSymbolLength, jsonBytes, 0, data.Length - msgSymbolLength);
                string json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                return JsonUtil.DeserializeFromString(json, type) as BaseMsg;
            }
            else
            {
                GDebug.Instance.Error("从信息中无法获取解析的类型");
                return default;
            }
        }
    }


    [Serializable]
    public class MsgHeartBeatPing : BaseMsg
    {
        public MsgHeartBeatPing()
        {
            base.protoName = "MsgHeartBeatPing";
        }
    }

    [Serializable]
    public class MsgHeartBeatPong : BaseMsg
    {
        public MsgHeartBeatPong()
        {
            base.protoName = "MsgHeartBeatPong";
        }
    }

    
}