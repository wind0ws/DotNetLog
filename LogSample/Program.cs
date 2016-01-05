using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Threshold.LogHelper;

namespace LogSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console Start Running...");
            //ConfigBeforeRecordLog();
            // OnlyRecordWarnErrorLog();
            // NormalLog();
            LogOffManual();
            Console.WriteLine("Console  Finished..");
            Console.WriteLine("Log.cfg is On your application folder");
            Console.WriteLine("Press Any Key To Exit...");
            Console.ReadKey();
        }

        static void NormalLog()
        {
            Log.V("This is Verbose Log");
            Log.D("This is Debug ");
            Log.I("SelfDefineTag","This is Information");
            Log.W("Warning!!!!");
            Log.E("ErrorTag","Error Occured.Please Check it!");
        }

        static void ConfigBeforeRecordLog()
        {
            Log.IsPrint = true;
            Log.LogFolder = @"D:\MyAppLogFolder";
            Log.W("Check Log File On "+Log.LogFolder);
        }

        static void OnlyRecordWarnErrorLog()
        {
            Log.IsPrint = true;
            Log.IsOnlyPrintWarnError = true;
            Log.D("This won't Record");
            Log.I("This won't Record too");
            Log.W("This is will Record");
            Log.E("This is will Record too.");
        }

        static void LogOffManual()
        {
            Log.IsOnlyPrintWarnError = false;
            Log.D("Debug Log");
            Log.W("Warn Log");
            Thread.Sleep(1000);//Wait a second to write log done.
            Log.IsPrint = false;
            Log.E("This Maybe Not Record Because you Close Log Switch.");
            Log.D("This Maybe Not Record Because you Close Log Switch.");
            Log.V("This Maybe Not Record Because you Close Log Switch.");

        } 

    }
}
