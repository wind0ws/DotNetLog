using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Threshold.Log.Utils
{
    class FileUtil
    {
        /// <summary>
        /// 在指定路径写入新文件，如果文件存在则根据第三个参数是接着写入或者是覆盖写入，写入成功返回True
        /// </summary>
        /// <param name="fileName">写入的文件的路径</param>
        /// <param name="content">要写入的字符串</param>
        /// <param name="isOverride">如果文件存在是否重写文件（即覆盖原文件内容）</param>
        /// <param name="isWriteLine">是否在新的一行写入内容</param>
        /// <returns></returns>
        public static bool WriteStringToFile(string fileName, string content, bool isOverride = false, bool isWriteLine = true)
        {
            if (!CreateFile(fileName, isOverride))
            {
                return false;
            }
            FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write);
            //FileStream fs = new FileStream(fileName, FileMode.Create,FileAccess.ReadWrite);//新地方创建文件并覆盖
            using (StreamWriter sr = new StreamWriter(fs, Encoding.Default))
            {
                if (isWriteLine)
                {
                    sr.WriteLine(content);
                }
                else
                {
                    sr.Write(content);
                }
                sr.Close();
            }
            fs.Close();
            return true;
        }

        /// <summary>
        /// 创建文件。如果文件存在，且isOverride为false，则不做任何处理直接返回true；如果文件存在，且isOverride为true，则覆盖文件并创建新文件。如果文件不存在则按正常方式创建新文件
        /// </summary>
        /// <param name="fileName">要创建文件的全路径</param>
        /// <param name="isOverride">如果文件存在是否覆盖并创建新文件</param>
        /// <returns>创建成功或原文件存在则为true，创建失败则为false</returns>
        public static bool CreateFile(string fileName, bool isOverride = false)
        {
            if (File.Exists(fileName) && !isOverride)
            {
                return true;
            }
            else
            {
                if (!DiretoryIsValid(fileName, true))
                {
                    return false;
                }
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Close();
                    return true;
                }
            }
        }

        /// <summary>
        /// 判断给定的路径是否有效。
        /// 如果路径不存在时根据第二个参数判断是否需要进行创建。
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="needCreate">路径无效时是否需要创建路径</param>
        /// <returns>true代表路径有效，false代表路径无效</returns>
        public static bool DiretoryIsValid(string path, bool needCreate = false)
        {
            if (path.IndexOf('\\') == -1)//说明这个路径根本不存在，可能只是文件名
            {
                return false;
            }
            string directory = path;
            if (path.LastIndexOf('.') > -1) //包括文件名的路径，那么我们只截取文件夹部分。
            {
                directory = path.Substring(0, path.LastIndexOf('\\') + 1);
            }
            if (Directory.Exists(directory))
            {
                return true;
            }
            else
            {
                if (needCreate)
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                        return true;
                    }
                    catch (Exception err)
                    { System.Diagnostics.Debug.WriteLine("调用DiretoryIsValid(string path,bool needCreate=false)方法出错。\r\n" + err.ToString()); }
                }
                return false;
            }
        }
    }
}
