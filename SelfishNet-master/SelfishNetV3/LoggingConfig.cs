using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace SelfishNetv3
{
    /// <summary>
    /// Centralized Serilog logging configuration.
    /// Replaces the old File.AppendAllText("error_log.txt") approach with
    /// structured, rolling log files and console output.
    /// </summary>
    public static class LoggingConfig
    {
        private static bool _isInitialized;

        /// <summary>
        /// Initializes the global Serilog logger.
        /// Safe to call multiple times — only the first call takes effect.
        /// </summary>
        /// <param name="logDirectory">
        /// Directory for log files. Defaults to "logs" subdirectory
        /// next to the executable.
        /// </param>
        public static void Initialize(string? logDirectory = null)
        {
            if (_isInitialized)
                return;

            logDirectory ??= Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "logs");

            Directory.CreateDirectory(logDirectory);

            var logPath = Path.Combine(logDirectory, "selfishnet-.log");

            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.WithProperty("Application", "SelfishNet")
                .Enrich.WithProperty("Version", "4.0.0")
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 5,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true);

#if DEBUG
            config.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
#endif

            Log.Logger = config.CreateLogger();
            _isInitialized = true;

            Log.Information("=== SelfishNet V4 started ===");
        }

        /// <summary>
        /// Flushes and closes the logger. Call on application exit.
        /// </summary>
        public static void Shutdown()
        {
            Log.Information("=== SelfishNet V4 shutting down ===");
            Log.CloseAndFlush();
        }
    }
}
