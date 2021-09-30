using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Framework.DataManager
{
    /// <summary>
    /// Xml序列化的工具类
    /// </summary>
    public static class XmlUtil
    {
        /// <summary>
        /// 从文件的反序列化
        /// </summary>
        /// <typeparam name="T">反序列化对象的类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>反序列化获得的对象</returns>
        public static T DeserializeFromFile<T>(string filePath) where T : class
        {
            try
            {
                string str = GFileStream.ReadString(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return DeserializeFromString<T>(str);
            }
            catch(Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        /// <summary>
        /// 从xml字符串的反序列化
        /// </summary>
        /// <typeparam name="T">反序列化对象的类型</typeparam>
        /// <param name="xml">xml字符串</param>
        /// <returns>反序列化获得的对象</returns>
        public static T DeserializeFromString<T>(string xml) where T : class
        {
            if (xml.Length == 0 || !typeof(T).IsSerializable)
                return default;

            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(typeof(T));
                    return xmldes.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        /// <summary>
        /// 序列化对象为字符串
        /// </summary>
        /// <typeparam name="T">序列化对象的类型</typeparam>
        /// <param name="obj">序列化对象</param>
        /// <returns>相对应的xml字符串</returns>
        public static string Serialize<T>(T obj) where T : class
        {
            if (obj == null || !typeof(T).IsSerializable)
                return default;

            try
            {
                MemoryStream stream = new MemoryStream();
                XmlSerializer xmlser = new XmlSerializer(typeof(T));
                xmlser.Serialize(stream, obj);
                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);
                string result = sr.ReadToEnd();
                sr.Dispose();
                stream.Dispose();
                return result;
            }
            catch(Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        /// <summary>
        /// 序列化对象为字符串并将其存储至文件中
        /// </summary>
        /// <typeparam name="T">序列化对象的类型</typeparam>
        /// <param name="obj">序列化对象</param>
        /// <param name="filePath">文件的路径</param>
        /// <returns></returns>
        public static bool Serialize<T>(T obj,string filePath) where T : class
        {
            string xml = Serialize<T>(obj);
            if (xml == default)
                return false;

            GFileStream.CoverWriteString(filePath, xml);
            return true;
        }
    }

    /// <summary>
    /// Json 序列化工具
    /// </summary>
    public static class JsonUtil
    {
        public static T DeserializeFromFile<T>(string filePath) where T : class
        {
            if (filePath.Length == 0 && !typeof(T).IsSerializable)
                return default;

            try
            {
                string jsonstr = GFileStream.ReadString(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return DeserializeFromString<T>(jsonstr);
            }
            catch(Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        public static T DeserializeFromString<T>(string json) where T : class
        {
            if (json.Length == 0 || !typeof(T).IsSerializable)
                return default;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        public static object DeserializeFromString(string json,Type type)
        {
            if (json.Length == 0 || !type.IsSerializable)
                return default;
            try
            {
                return JsonUtility.FromJson(json,type);
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        public static void LoadObjectDataFromFile<T>(string filePath,ref T value)
        {
            if (filePath.Length == 0 && !typeof(T).IsSerializable)
                return;

            try
            {
                string jsonstr = GFileStream.ReadString(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                JsonUtility.FromJsonOverwrite(jsonstr, value);
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
            }
        }

        public static void LoadObjectDataFromString<T>(string json,ref T value)
        {
            if (json.Length == 0 && !typeof(T).IsSerializable)
                return;

            try
            {
                JsonUtility.FromJsonOverwrite(json, value);
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
            }
        }




        /// <summary>
        /// 序列化对象为字符串
        /// </summary>
        /// <typeparam name="T">序列化对象的类型</typeparam>
        /// <param name="obj">序列化对象</param
        /// <param name="prettyPrint">是否转化成可读的格式</param>
        /// <returns>相对应的json字符串</returns>
        public static string Serialize<T>(T obj, bool prettyPrint = false) where T : class
        {
            if (obj == null || !typeof(T).IsSerializable)
                return default;

            try
            {
                return JsonUtility.ToJson(obj, prettyPrint);
            }
            catch (Exception ex)
            {
                GDebug.Instance.Error(ex.ToString());
                return default;
            }
        }

        /// <summary>
        /// 序列化对象为字符串并将其存储至文件中
        /// </summary>
        /// <typeparam name="T">序列化对象的类型</typeparam>
        /// <param name="obj">序列化对象</param>
        /// <param name="filePath">文件的路径</param>
        /// <returns></returns>
        public static bool Serialize<T>(T obj, string filePath, bool prettyPrint = false) where T : class
        {
            string json = Serialize(obj);
            if (json == default)
                return false;

            GFileStream.CoverWriteString(filePath, json);
            return true;
        }
    }
}
