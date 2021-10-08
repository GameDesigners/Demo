using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace NetMessage
{
    /// <summary>
    /// 信息事件
    /// </summary>
    public enum ProtoName
    {
        /// <summary>
        /// 客户端发往服务端的心跳信号
        /// </summary>
        MsgHeartBeatPing,
        /// <summary>
        /// 服务端法网客户端的心跳信号
        /// </summary>
        MsgHeartBeatPong,      
    }

    /// <summary>
    /// 长度信息变量类型
    /// </summary>
    public enum LengthMsgDataType : int
    {
        /// <summary>
        /// short类型
        /// </summary>
        INT16 = 2,
        /// <summary>
        /// int类型
        /// </summary>
        INT32 = 4
    }

    /// <summary>
    /// 信息基类
    /// </summary>
    [Serializable]
    public class BaseMsg
    {
        /// <summary>
        /// 信息类名称
        /// </summary>
        public string protoName { get; set; }

        /// <summary>
        /// 对可序列化的实例进行json序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static byte[] Encode<T>(T msg) where T : class
        {
            string json = JsonSerializer.Serialize(msg);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// 对完整的json语句进行解码（要求要符合json格式的byte数组）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static BaseMsg Decode<T>(byte[] bytes, int offset, int count) where T : class
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            BaseMsg msg = JsonSerializer.Deserialize<T>(json) as BaseMsg;
            return msg;
        }

        /// <summary>
        /// 根据实例获取发送信息的字节包
        /// </summary>
        /// <typeparam name="T">打包的实例类型</typeparam>
        /// <param name="protoName">实例类名称</param>
        /// <param name="msg">实例</param>
        /// <returns>字节缓冲</returns>
        public static ByteBuffer GetBytesPackage<T>(string protoName, T msg) where T : BaseMsg
        {
            byte[] data = Encode(msg);

            byte[] len_msg;
            if (Core.LMDT == LengthMsgDataType.INT16)
            {

                short type_str_length = (short)protoName.Length;
                short msg_length = (short)(2 + type_str_length + data.Length);
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
        public static BaseMsg ParseBytesPage(byte[] data, int msgSymbolLength, out string typeName)
        {
            typeName = "";
            if (data.Length < (int)Core.LMDT * 2)
            {
                return default;
            }
            byte[] typeBytes = new byte[msgSymbolLength];
            Array.Copy(data, 0, typeBytes, 0, msgSymbolLength);
            typeName = System.Text.Encoding.UTF8.GetString(typeBytes);
            Assembly assembly = Assembly.LoadFrom("NetMessage.dll");
            Type type = assembly.GetType($"NetMessage.{typeName}");
            if (type != null)
            {
                byte[] jsonBytes = new byte[data.Length - msgSymbolLength];
                Array.Copy(data, msgSymbolLength, jsonBytes, 0, data.Length - msgSymbolLength);
                string json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                //Console.WriteLine($"json:{json}");
                try
                {
                    return JsonSerializer.Deserialize(json, type) as BaseMsg;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"解析json文件[{json}]时发生错误...\n{ex}");
                    return default;
                }
            }
            else
            {
                return default;
            }
        }
    }


    /// <summary>
    /// 心跳机制的Ping
    /// </summary>
    [Serializable]
    public class MsgHeartBeatPing : BaseMsg
    {
        /// <summary>
        /// 初始化protoName的构造函数
        /// </summary>
        public MsgHeartBeatPing()
        {
            base.protoName = "MsgHeartBeatPing";
        }
    }

    /// <summary>
    /// 心跳机制的Pong
    /// </summary>
    [Serializable]
    public class MsgHeartBeatPong : BaseMsg
    {
        /// <summary>
        /// 初始化protoName的构造函数
        /// </summary>
        public MsgHeartBeatPong()
        {
            base.protoName = "MsgHeartBeatPong";
        }
    }
}
