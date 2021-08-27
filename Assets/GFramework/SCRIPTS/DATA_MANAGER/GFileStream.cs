using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Framework.DataManager
{
    /// <summary>
    /// 封装的文件流
    /// </summary>
    public static class GFileStream
    {
        private const int BUFFERSIZE = 256;

        /// <summary>
        /// 读取字符串至某个文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="mode">文件流模式</param>
        /// <param name="access">文件流应用权限</param>
        /// <param name="share">文件流共享方式</param>
        /// <returns></returns>
        public static string ReadString(string filePath, FileMode mode, FileAccess access, FileShare share)
        {
            if (!FullPathMatch(filePath))
            {
                Debug.LogError("文件路径不合法");
                return default;
            }

            try
            {
                FileStream stream = new FileStream(filePath, mode, access, share);
                byte[] buffer = new byte[BUFFERSIZE];
                bool completed = false;
                Encoding encoding = GetEncoding(stream);
                string result = "";
                do
                {
                    int nread = stream.Read(buffer, 0, BUFFERSIZE);
                    if (nread == 0) completed = true;
                    if (nread < BUFFERSIZE)
                        Array.Clear(buffer, nread, BUFFERSIZE - nread);
                    result += encoding.GetString(buffer, 0, nread);
                }
                while (!completed);
                stream.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                return default;
            }
        }

        /// <summary>
        /// 将字符串写入文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="str">写入字符串</param>
        /// <param name="encoding">字符编码器</param>
        public static void WriteString(string filePath,string str,Encoding encoding=default)
        {
            if (!FullPathMatch(filePath))
            {
                Debug.LogError("文件路径不合法");
                return;
            }

            if (encoding == default)
                encoding = Encoding.UTF8;

            try
            {
                FileStream stream = File.OpenWrite(filePath);
                byte[] buffer = encoding.GetBytes(str);
                stream.Write(buffer, 0, buffer.Length);
                stream.Dispose();
            }
            catch(Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 获取某个文件流的编码器
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static Encoding GetEncoding(Stream stream)
        {
            if (!stream.CanSeek) throw new ArgumentException("当前流不能定位（Seek）");

            Encoding encoding = Encoding.ASCII;
            byte[] bom = new byte[5];
            int nRead = stream.Read(bom, offset: 0, count: 5);
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
            {
                stream.Seek(4, SeekOrigin.Begin);
                return Encoding.UTF32;
            }
            else if(bom[0] == 0xff && bom[1] == 0xfe)
            {
                stream.Seek(2, SeekOrigin.Begin);
                return Encoding.Unicode;
            }
            else if (bom[0] == 0xfe && bom[1] == 0xff)
            {
                stream.Seek(2, SeekOrigin.Begin);
                return Encoding.BigEndianUnicode;
            }
            else if(bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
            {
                stream.Seek(3, SeekOrigin.Begin);
                return Encoding.UTF8;
            }

            stream.Seek(0, SeekOrigin.Begin);
            return encoding;
        }

        /// <summary>
        /// 路径的正则表达式
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        private static bool FullPathMatch(string filePath)
        {
            //Regex regex = new Regex(@"^([a-zA-Z]:\\)?[^\/\:\*\?\""\<\>\|\,]*$");
            //return regex.Match(filePath).Success;
            return true;
        }
    }
}