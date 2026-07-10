using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SelfishNetv3
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // التقاط أخطاء الواجهة (UI)
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // التقاط أخطاء الخلفية (Background threads)
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            ApplicationConfiguration.Initialize();
            Application.Run(new ArpForm());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogError(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError((Exception)e.ExceptionObject);
        }

        static void LogError(Exception ex)
        {
            string filePath = "error_log.txt";
            string logMessage = $"[{DateTime.Now}] Error: {ex.Message}\nStack Trace: {ex.StackTrace}\n-----------------------------------\n";

            File.AppendAllText(filePath, logMessage);

            MessageBox.Show("حصل خطأ! تم تسجيل التفاصيل في ملف error_log.txt", "خطأ برمجي", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}