using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threshold.LogHelper.Utils
{
    class IniHelper
    {
        private string iniFullPath; //INI文件名
                                    //声明读写INI文件的API函数
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        /// <summary>
        /// 构造函数（绝对路径）
        /// </summary>
        /// <param name="fullPath">绝对路径</param>
        /// <param name="defaultIniContent">默认ini内容（在ini文件不存在时创建）</param>
        public IniHelper(string fullPath, string defaultIniContent = "This File haven't config yet.")
        {
            FileUtil.DiretoryIsValid(fullPath, true);
            // 判断文件是否存在
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fullPath);
            if (!fileInfo.Exists)
            { //|| (FileAttributes.Directory in fileInfo.Attributes))
              //文件不存在，建立文件
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fullPath, false, System.Text.Encoding.Default);
                try
                {
                    sw.Write(defaultIniContent);
                    sw.Close();
                }
                catch
                {
                    throw (new ApplicationException("Ini Config File Can't be created。Check your account permissions."));
                }
            }
            //必须是完全路径，不能是相对路径
            iniFullPath = fileInfo.FullName;
        }

        /// <summary>
        /// 构造函数（相对路径）
        /// </summary>
        /// <param name="releativePath">文件名称。如“设置.ini”</param>
        public IniHelper(string releativePath)
            : this(string.Format(@"{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, releativePath), "This File haven't config yet.")
        { }

        /// <summary>
        /// 写入字符串到ini指定Section下的Key中
        /// </summary>
        /// <param name="section">结点</param>
        /// <param name="key">标识变量名</param>
        /// <param name="value">写入的值</param>
        public void WriteString(string section, string key, string value)
        {
            if (!WritePrivateProfileString(section, key, value, iniFullPath))
            {
                throw (new ApplicationException("写Ini文件出错"));
            }
        }
        /// <summary>
        /// 读取指定结点的指定标识位置
        /// </summary>
        /// <param name="section">结点位置</param>
        /// <param name="key">标示符位置</param>
        /// <param name="Value">默认值，亦即读取不到时返回的值</param>
        public string ReadString(string section, string key, string defaultVal)
        {
            if (!SectionExists(section) || !KeyExists(section, key))
            {
                WriteString(section, key, defaultVal);
                return defaultVal;
            }
            byte[] Buffer = new byte[65535];
            int bufLen = GetPrivateProfileString(section, key, defaultVal, Buffer, Buffer.GetUpperBound(0), iniFullPath);
            //必须设定0（系统默认的代码页）的编码方式，否则无法支持中文
            string s = Encoding.GetEncoding(0).GetString(Buffer);
            s = s.Substring(0, bufLen);
            return s.Trim().Replace("\0", "");
        }

        /// <summary>
        /// 读取指定结点位置的指定标识变量并返回一个整数
        /// </summary>
        /// <param name="section">结点名</param>
        /// <param name="key">标识变量名</param>
        /// <param name="defaultVal">默认值，读取失败时返回</param>
        /// <returns></returns>
        public int ReadInteger(string section, string key, int defaultVal)
        {
            string intStr = ReadString(section, key, defaultVal.ToString());
            try
            {
                return Convert.ToInt32(intStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return defaultVal;
            }
        }

        /// <summary>
        /// 写整数
        /// </summary>
        /// <param name="section">结点名</param>
        /// <param name="key">标识变量名</param>
        /// <param name="value">要写入的整数</param>
        public void WriteInteger(string section, string key, int value)
        {
            WriteString(section, key, value.ToString());
        }

        /// <summary>
        /// 读取指定结点指定标示符位置的布尔值
        /// </summary>
        /// <param name="section">结点名称</param>
        /// <param name="key">标识符名称</param>
        /// <param name="defaultVal">默认返回的布尔值</param>
        /// <returns></returns>
        public bool ReadBool(string section, string key, bool defaultVal)
        {
            try
            {
                return Convert.ToBoolean(ReadString(section, key, defaultVal.ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex, "IniHelper.ReadBool()方法");
                return defaultVal;
            }
        }

        /// <summary>
        /// 写入Bool值
        /// </summary>
        /// <param name="section">结点名</param>
        /// <param name="key">标识符名称</param>
        /// <param name="value">要写入的bool值</param>
        public void WriteBool(string section, string key, bool value)
        {
            WriteString(section, key, value.ToString());
        }


        /// <summary>
        /// 从Ini文件中，将指定的Section名称中的所有Key添加到列表中
        /// </summary>
        /// <param name="section"></param>
        /// <param name="Keys"></param>
        public List<string> ReadSectionKeys(string section)
        {
            byte[] buffer = new byte[16384];
            int bufLen = GetPrivateProfileString(section, null, null, buffer, buffer.GetUpperBound(0), iniFullPath);
            //对Section进行解析
            return GetStringsFromBuffer(buffer, bufLen);
        }

        /// <summary>
        /// 从Buffer中读取string
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="bufLen">长度</param>
        /// <returns></returns>
        private List<string> GetStringsFromBuffer(byte[] buffer, int bufLen)
        {
            List<string> strings = new List<string>();
            if (bufLen != 0)
            {
                int start = 0;
                for (int i = 0; i < bufLen; i++)
                {
                    if ((buffer[i] == 0) && ((i - start) > 0))
                    {
                        string s = Encoding.GetEncoding(0).GetString(buffer, start, i - start);
                        strings.Add(s);
                        start = i + 1;
                    }
                }
            }
            return strings;
        }

        /// <summary>
        ///  从Ini文件中，读取所有的Sections的名称
        /// </summary>
        /// <returns></returns>
        public List<string> ReadSections()
        {
            //Note:必须得用Bytes来实现，StringBuilder只能取到第一个Section
            byte[] buffer = new byte[65535];
            int bufLen = 0;
            bufLen = GetPrivateProfileString(null, null, null, buffer,
             buffer.GetUpperBound(0), iniFullPath);
            return GetStringsFromBuffer(buffer, bufLen);
        }

        /*
        /// <summary>
        /// 读取指定的Section的所有Key和Value到Array列表中
        /// </summary>
        /// <param name="Section">结点名称</param>
        /// <returns>返回</returns>
        public ArrayList ReadSectionValues(string Section)
        {
           List<string> KeysList= ReadSectionKey(Section);
           ArrayList arry = new ArrayList();
            foreach (string key in KeysList)
            {
                KeyPairValue<string, string> pair = new KeyPairValue<string, string>(key, ReadString(Section, key, ""));
                arry.Add(pair);
            }
            return arry;
        }*/

        /// <summary>
        /// 读取指定的Section的所有Key和Value到Array列表中
        /// </summary>
        /// <param name="Section">结点名称</param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> ReadSectionValues(string Section)
        {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            List<string> KeysList = ReadSectionKeys(Section);
            foreach (string Key in KeysList)
            {
                KeyValuePair<string, string> pair = new KeyValuePair<string, string>(Key, ReadString(Section, Key, ""));
                values.Add(pair);
            }
            return values;
        }

        ////读取指定的Section的所有Value到列表中，
        //public void ReadSectionValues(string Section, NameValueCollection Values,char splitString)
        //{　 string sectionValue;
        //　　string[] sectionValueSplit;
        //　　StringCollection KeyList = new StringCollection();
        //　　ReadSection(Section, KeyList);
        //　　Values.Clear();
        //　　foreach (string key in KeyList)
        //　　{
        //　　　　sectionValue=ReadString(Section, key, "");
        //　　　　sectionValueSplit=sectionValue.Split(splitString);
        //　　　　Values.Add(key, sectionValueSplit[0].ToString(),sectionValueSplit[1].ToString());

        //　　}
        //}



        /// <summary>
        /// 删除某个Section整个结点
        /// </summary>
        /// <param name="Section">结点名称</param>
        public void DeleteSection(string section)
        {
            if (!WritePrivateProfileString(section, null, null, iniFullPath))
            {
                throw (new ApplicationException("无法清除Ini文件中的Section"));
            }
        }

        /// <summary>
        /// 删除某个Section下的键
        /// </summary>
        /// <param name="section">结点名称</param>
        /// <param name="key">标识符变量名</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteKey(string section, string key)
        {
            if (WritePrivateProfileString(section, key, null, iniFullPath))
            { return true; }
            else { return false; }
        }
        //Note:对于Win9X来说需要实现UpdateFile方法将缓冲中的数据写入文件
        //在Win NT, 2000和XP上，都是直接写文件，没有缓冲，所以，无须实现UpdateFile
        //执行完对Ini文件的修改之后，应该调用本方法更新缓冲区。
        public void UpdateFile()
        {
            WritePrivateProfileString(null, null, null, iniFullPath);
        }


        /// <summary>
        /// 检查某个Section下的某个Key是否存在
        /// </summary>
        /// <param name="Section">结点名</param>
        /// <param name="key">标识符变量名</param>
        /// <returns>是否存在</returns>
        public bool KeyExists(string Section, string key)
        {
            if (!SectionExists(Section))
            {
                throw new Exception("在判断Key是否存在之前，应判断Section是否存在。当前检测到Section不存在");
            }
            List<string> Keys = ReadSectionKeys(Section);
            return Keys.IndexOf(key) > -1;
        }
        /// <summary>
        /// 判断指定的Section是否存在
        /// </summary>
        /// <param name="section">结点名称</param>
        /// <returns></returns>
        public bool SectionExists(string section)
        {
            List<string> sections = ReadSections();
            return sections.IndexOf(section) > -1;
        }
    }
}
