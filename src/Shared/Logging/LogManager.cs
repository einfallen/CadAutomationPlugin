using NLog;
using NLog.Config;
using NLog.Targets;

namespace Shared.Logging
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public static class LogManager
    {
        private static string _logPath = "";

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize()
        {
#if !CLOUD_BUILD
            var config = new LoggingConfiguration();

            // 设置日志路径
            _logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CadAutomationPlugin",
                "logs");

            Directory.CreateDirectory(_logPath);

            // 文件目标
            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = Path.Combine(_logPath, "plugin-${shortdate}.log"),
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=tostring}",
                MaxArchiveFiles = 7,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date
            };

            // 控制台目标（调试用）
            var consoleTarget = new ConsoleTarget("consoleTarget")
            {
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
            };

            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            // 日志规则
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

            NLog.LogManager.Configuration = config;
#endif
        }

        /// <summary>
        /// 获取日志记录器
        /// </summary>
        public static ILogger GetLogger(string name)
        {
#if CLOUD_BUILD
            return new ConsoleLogger(name);
#else
            return new NLogLogger(NLog.LogManager.GetLogger(name));
#endif
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public static string GetLogPath()
        {
            return _logPath;
        }
    }

    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(string message, System.Exception ex);
        void Fatal(string message);
        void Fatal(string message, System.Exception ex);
    }

    /// <summary>
    /// NLog 适配器
    /// </summary>
#if !CLOUD_BUILD
    public class NLogLogger : ILogger
    {
        private readonly NLog.Logger _logger;

        public NLogLogger(NLog.Logger logger)
        {
            _logger = logger;
        }

        public void Debug(string message) => _logger.Debug(message);
        public void Info(string message) => _logger.Info(message);
        public void Warn(string message) => _logger.Warn(message);
        public void Error(string message) => _logger.Error(message);
        public void Error(string message, System.Exception ex) => _logger.Error(ex, message);
        public void Fatal(string message) => _logger.Fatal(message);
        public void Fatal(string message, System.Exception ex) => _logger.Fatal(ex, message);
    }
#endif

    /// <summary>
    /// 控制台日志适配器（云编译用）
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly string _name;

        public ConsoleLogger(string name)
        {
            _name = name;
        }

        public void Debug(string message) => System.Diagnostics.Debug.WriteLine($"[{_name}] DEBUG: {message}");
        public void Info(string message) => System.Console.WriteLine($"[{_name}] INFO: {message}");
        public void Warn(string message) => System.Console.WriteLine($"[{_name}] WARN: {message}");
        public void Error(string message) => System.Console.WriteLine($"[{_name}] ERROR: {message}");
        public void Error(string message, System.Exception ex) => System.Console.WriteLine($"[{_name}] ERROR: {message} - {ex.Message}");
        public void Fatal(string message) => System.Console.WriteLine($"[{_name}] FATAL: {message}");
        public void Fatal(string message, System.Exception ex) => System.Console.WriteLine($"[{_name}] FATAL: {message} - {ex.Message}");
    }
}
