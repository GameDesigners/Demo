using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace Framework.DataManager
{
    public static class Serializer
    {
        /// <summary>
        /// 将可序列化的类实例序列化成字节数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null || !obj.GetType().IsSerializable)
                return null;

            BinaryFormatter formatter = new BinaryFormatter();
            using(MemoryStream stream=new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                byte[] data = stream.ToArray();
                return data;
            }
        }


        /// <summary>
        /// 将字符数组反序列化为类实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : class
        {
            if (data == null || !typeof(T).IsSerializable)
                return default;

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                object obj = formatter.Deserialize(stream);
                return obj as T;
            }
        }
    }

    
}
