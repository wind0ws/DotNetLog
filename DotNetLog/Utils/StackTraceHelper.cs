using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Threshold.LogHelper.Utils
{
    class StackTraceHelper
    {
        public enum TraceInfo
        {
            LineNumber, FileName, Method, ColumnNumber, Full
        }
        public static string GetLineNumber(StackFrame frame)
        {
            return GetStackFrameInfo(frame, TraceInfo.LineNumber);
        }

        public static string GetFileName(StackFrame frame)
        {
            return GetStackFrameInfo(frame, TraceInfo.FileName);
        }

        public static string GetFileNameWithoutPath(StackFrame frame)
        {
            var fileName = GetFileName(frame);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                int index = fileName.LastIndexOf("\\");
                if (index + 1 <= fileName.Length - 1)
                {
                    return fileName.Substring(index + 1);
                }
            }
            return fileName;
        }

        public static string GetMethod(StackFrame frame)
        {
            return GetStackFrameInfo(frame, TraceInfo.Method);
        }

        public static string GetColumnNumber(StackFrame frame)
        {
            return GetStackFrameInfo(frame, TraceInfo.ColumnNumber);
        }

        public static string GetFullStatckTraceInfo(StackFrame frame)
        {
            return GetStackFrameInfo(frame, TraceInfo.Full);
        }

        public static string GetStackFrameInfo(StackFrame frame, TraceInfo traceInfo)
        {
            if (frame != null)
            {
                switch (traceInfo)
                {
                    case TraceInfo.LineNumber:
                        return frame.GetFileLineNumber().ToString();
                    case TraceInfo.FileName:
                        return frame.GetFileName();
                    case TraceInfo.Method:
                        return frame.GetMethod().Name;
                    case TraceInfo.ColumnNumber:
                        return frame.GetFileColumnNumber().ToString();
                    case TraceInfo.Full:
                        return new StackTrace(frame).ToString();
                }
            }
            return "Error: StackFrame==null";
        }




        //private static StackFrame GetStackFrame(StackTrace trace)
        //{
        //    if(trace.FrameCount>0)
        //    {
        //        return trace.GetFrame(0);
        //    }
        //    return null;
        //}
    }
}
