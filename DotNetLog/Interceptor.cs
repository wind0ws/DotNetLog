using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threshold.Log
{
    /// <summary>
    /// 日志插入器
    /// </summary>
    public interface Interceptor
    {
        /// <summary>
        /// 当调用Log的V D I W E方法时将触发下面的回调接口
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="tag">标签</param>
        /// <param name="message">内容</param>
        /// <returns>返回false将继续执行其他的Interceptor以及最终调用Log的WriteLog方法；返回false将不再执行其余的Interceptor以及WriteLog</returns>
        bool Intercept(LogLevel level, string tag, object message);
    }
}
