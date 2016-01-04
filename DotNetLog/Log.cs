using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Threshold.Log.Utils;

namespace Threshold.Log
{
    public enum LogLevel
    {
        Verbose = 0, Debug = 1, Info = 2, Warn = 3, Error = 4
    }
    public class Log
    { 

        struct LogParam
        {
            public LogLevel logLevel;
            public string tag;
            public object message;
            public LogParam(LogLevel logLevel, string tag, object message)
            {
                this.logLevel = logLevel;
                this.tag = tag;
                this.message = message;
            }
        }

        private const string INI_SECTION_LOG = "Log";
        private const string INI_KEY_LOG_FOLDER = "LogFolder";
        private const string INI_KEY_IS_PRINT = "IsPrint";
        private const string INI_KEY_IS_ONLY_PRINT_WARN_ERROR = "IsOnlyPrintWarnError";

        private const int READ_CONFIG_FREQUENCY = 5;
        private static readonly string _defaultLogFolder = AppDomain.CurrentDomain.BaseDirectory + @"\Logs";

        private static readonly byte[] _lock = new byte[0];

        private static IniHelper mIniHelper;

        private static LogFileHelper mLogHelper;

        private static DateTime mLastReadConfigTime;

        private static ISet<Interceptor> mInterceptors = new HashSet<Interceptor>();


        private static string _logFolder;
        private static bool _isPrint, _isOnlyPrintWarnError;

        public static string LogFolder
        {
            get
            {
                return _logFolder;
            }
            set
            {
                PerformInitConfig();
                if (_logFolder == null || string.Compare(_logFolder, value, true) != 0)
                {
                    _logFolder = value;
                    mIniHelper.WriteString(INI_SECTION_LOG, INI_KEY_LOG_FOLDER, value);
                }
            }
        }
        public static bool IsPrint
        {
            get
            {
                return _isPrint;
            }
            set
            {
                PerformInitConfig();
                if (_isPrint != value)
                {
                    _isPrint = value;
                    mIniHelper.WriteBool(INI_SECTION_LOG, INI_KEY_IS_PRINT, value);
                }
            }
        }

        public static bool IsOnlyPrintWarnError
        {
            get
            {
                return _isOnlyPrintWarnError;
            }
            set
            {
                PerformInitConfig();
                if (_isOnlyPrintWarnError != value)
                {
                    _isOnlyPrintWarnError = value;
                    mIniHelper.WriteBool(INI_SECTION_LOG, INI_KEY_IS_ONLY_PRINT_WARN_ERROR, value);
                }
            }
        }

        public static bool AddInterceptor(Interceptor interceptor)
        {
            return mInterceptors.Add(interceptor);
        }

        public static bool RemoveInterceptor(Interceptor interceptor)
        {
            return mInterceptors.Remove(interceptor);
        }

        public static void RemoveAllInterceptors()
        {
            mInterceptors.Clear();
        }

        public static void V(string tag, object message)
        {
            WriteLogAsync(LogLevel.Verbose, tag, message);
        }

        public static void V(object message)
        {
            V(GetTag(), message);
        }

        public static void D(string tag, object message)
        {
            WriteLogAsync(LogLevel.Debug, tag, message);
        }

        public static void D(object message)
        {
            D(GetTag(), message);
        }

        public static void I(string tag, object message)
        {
            WriteLogAsync(LogLevel.Info, tag, message);
        }

        public static void I(object message)
        {
            I(GetTag(), message);
        }


        public static void W(string tag, object message)
        {
            WriteLogAsync(LogLevel.Warn, tag, message);
        }


        public static void W(object message)
        {
            W(GetTag(), message);
        }


        public static void E(string tag, object message)
        {
            WriteLogAsync(LogLevel.Error, tag, message);
        }

        public static void E(object message)
        {
            E(GetTag(), message);
        }

        //private static void CheckIniHelper()
        //{
        //    var hasInit = false;
        //    CheckIniHelper(ref hasInit);
        //    System.Diagnostics.Debug.WriteLine("Before Call Log.VDIWE method,You Changed Config,Is Cfg File  On Disk: "+!hasInit);
        //}

       

        private static void CheckLogHelper()
        {
            //string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + DateTime.Now.ToString("yyMMdd") + "Log.txt";
            string fileName = LogFolder + @"\" + DateTime.Now.ToString("yyMMdd") + "Log.txt";
            if (mLogHelper == null)
            {
                mLogHelper = new LogFileHelper(fileName);
            }
            else
            {
                mLogHelper.FileName = fileName;
            }
        }

        private static void WriteLogAsync(LogLevel level, string tag, object message)
        {
            LogParam logParam = new LogParam(level, tag, message);
            try
            {
                ThreadPool.QueueUserWorkItem(HandleLog, logParam);
            }
            catch (Exception ee)
            {
                //If fatal Error Occured,write it down!
                FileUtil.WriteStringToFile(@"C:\FatalError.txt", DateTime.Now.ToLongTimeString() + ee.ToString());
            }
        }

        private static void HandleLog(object param)
        {
            if (param is LogParam)
            {
                var logParam = (LogParam)param;
                foreach (Interceptor interceptor in mInterceptors)
                {
                    if (interceptor.Intercept(logParam.logLevel, logParam.tag, logParam.message))
                    {
                        return;
                    }
                }
                WriteLog(logParam.logLevel, logParam.tag, logParam.message);
            }
        }

        private static void WriteLog(LogLevel level, string tag, object message)
        {
            PerformInitConfig();

            if (!IsPrint || (IsOnlyPrintWarnError && level < LogLevel.Warn))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(tag))
            {
                tag = "Tag";
            }
            if (message == null)
            {
                message = string.Empty;
            }
            CheckLogHelper();
            string content = string.Format("{0} {1}/{2}: {3}", DateTime.Now.ToString("HH:mm:ss:fff"), level, tag, message);
            mLogHelper.WriteLine(content);
        }


       private static void PerformInitConfig()
        {
            if (IsNeedInitLogCfg())
            {
                lock (_lock)
                {
                    if (IsNeedInitLogCfg())
                    {
                        InitLogFromCfg();
                    }
                }
            }
        }


        private static bool IsNeedInitLogCfg()
        {
            return mIniHelper==null|| mLastReadConfigTime == default(DateTime) ||
                (DateTime.Now - mLastReadConfigTime).TotalMinutes > READ_CONFIG_FREQUENCY;
        }


        public static void InitLogFromCfg()
        {
            var hasInit = false;
            CheckIniHelper(ref hasInit);
            if (!hasInit)
            {
                _logFolder = mIniHelper.ReadString(INI_SECTION_LOG, INI_KEY_LOG_FOLDER, _defaultLogFolder);
                _isPrint = mIniHelper.ReadBool(INI_SECTION_LOG, INI_KEY_IS_PRINT, false);
                _isOnlyPrintWarnError = mIniHelper.ReadBool(INI_SECTION_LOG, INI_KEY_IS_ONLY_PRINT_WARN_ERROR, false);
            }
            mLastReadConfigTime = DateTime.Now;
        }

        private static void CheckIniHelper(ref bool hasInit)
        {
            if (mIniHelper == null)
            {
                var cfgIsPrint = _isPrint.ToString();
                var cfgIsOnlyPrintWarnError = _isOnlyPrintWarnError.ToString();
                var defaultCfg = string.Format(@"[Log]
LogFolder={0}
IsPrint={1}
IsOnlyPrintWarnError={2}", _defaultLogFolder, cfgIsPrint, cfgIsOnlyPrintWarnError);
                var cfgFileName = AppDomain.CurrentDomain.BaseDirectory + @"\Log.cfg";
                if (!System.IO.File.Exists(cfgFileName))
                {
                    _logFolder = _defaultLogFolder;
                    _isPrint = false;
                    _isOnlyPrintWarnError = false;
                    hasInit = true;
                }
                mIniHelper = new IniHelper(cfgFileName, defaultCfg);
            }
        }

        /// <summary>
        /// 获取当前运行的代码信息作为日志的Tag
        /// 格式为 文件名-方法名-代码所在行数
        /// </summary>
        /// <param name="skipFrames"></param>
        /// <returns></returns>
        public static string GetTag(int skipFrames = 2)
        {
            StackFrame frame = new StackFrame(skipFrames, true);
            var lineNumber = StackTraceHelper.GetLineNumber(frame);
            var method = StackTraceHelper.GetMethod(frame);
            var fileName = StackTraceHelper.GetFileNameWithoutPath(frame);
            return string.Format("{0}-{1}-{2}", fileName, method, lineNumber);
        }
    }
}
