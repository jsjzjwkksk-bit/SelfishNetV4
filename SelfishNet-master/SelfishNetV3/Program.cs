using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace SelfishNetv3
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize structured logging
            LoggingConfig.Initialize();

            // Capture UI thread exceptions
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // Capture background thread exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new ArpForm());
            }
            finally
            {
                LoggingConfig.Shutdown();
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled UI thread exception");
            MessageBox.Show(
                $"An error occurred: {e.Exception.Message}\n\nDetails have been logged.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal(ex, "Unhandled background thread exception (terminating: {IsTerminating})",
                e.IsTerminating);

            MessageBox.Show(
                $"A critical error occurred: {ex.Message}\n\nDetails have been logged.",
                "Critical Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}