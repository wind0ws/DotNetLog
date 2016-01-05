using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threshold.LogHelper.Utils
{
    class LogFileHelper
    {
        private static readonly byte[] _lock = new byte[0];
        private static readonly byte[] _createFileLock = new byte[0];
        private string _fileName;
        private static Dictionary<long, long> lockDic = new Dictionary<long, long>();
        /// <summary>  
        /// 获取或设置文件名称  
        /// </summary>  
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == null || !_fileName.Equals(value))
                {
                    Create(value);
                    _fileName = value;
                }
            }
        }
        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="byteCount">每次开辟位数大小，这个直接影响到记录文件的效率</param>  
        /// <param name="fileName">文件全路径名</param>  
        public LogFileHelper(string fileName)
        {
            _fileName = fileName;
            Create(fileName);
        }
        /// <summary>  
        /// 创建文件  
        /// </summary>  
        /// <param name="fileName"></param>  
        public void Create(string fileName)
        {
            if (FileUtil.DiretoryIsValid(fileName, true) && !System.IO.File.Exists(fileName))
            {
                lock(_createFileLock)
                {
                    if (!System.IO.File.Exists(fileName))
                    {
                        using (System.IO.FileStream fs = System.IO.File.Create(fileName))
                        {
                            fs.Close();
                        }
                    }
                }
                
            }
        }
        /// <summary>  
        /// 写入文本  
        /// </summary>  
        /// <param name="content">文本内容</param>  
        private void Write(string content, string newLine)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                return;
                //throw new Exception("FileName不能为空！");
            }

            using (System.IO.FileStream fs = new System.IO.FileStream(_fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, 8, System.IO.FileOptions.Asynchronous))
            {
                //Byte[] dataArray = System.Text.Encoding.ASCII.GetBytes(System.DateTime.Now.ToString() + content + "/r/n");  
                byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(content + newLine);
                bool flag = true;
                long slen = dataArray.Length;
                long len = 0;
                lock (_lock)
                {
                    while (flag)
                    {
                        try
                        {
                            if (len >= fs.Length)
                            {
                                fs.Lock(len, slen);
                                lockDic[len] = slen;
                                flag = false;
                            }
                            else
                            {
                                len = fs.Length;
                            }
                        }
                        catch (Exception)
                        {
                            while (!lockDic.ContainsKey(len))
                            {
                                len += lockDic[len];
                            }
                        }
                    }

                    fs.Seek(len, System.IO.SeekOrigin.Begin);
                    fs.Write(dataArray, 0, dataArray.Length);
                }
                fs.Close();
            }
        }
        /// <summary>  
        /// 写入文件内容  
        /// </summary>  
        /// <param name="content"></param>  
        public void WriteLine(string content)
        {
            this.Write(content, System.Environment.NewLine);
        }
        /// <summary>  
        /// 写入文件  
        /// </summary>  
        /// <param name="content"></param>  
        public void Write(string content)
        {
            this.Write(content, "");
        }
    }
}
