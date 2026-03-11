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
        private static NLog.LogManager? _logManager;
        private static string _logPath = "";

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize()
        {
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
        }

        /// <summary>
        /// 获取日志记录器
        /// </summary>
        public static ILogger GetLogger(string name)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(name));
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
        void Error(string message, Exception ex);
        void Fatal(string message);
        void Fatal(string message, Exception ex);
    }

    /// <summary>
    /// NLog 适配器
    /// </summary>
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
        public void Error(string message, Exception ex) => _logger.Error(ex, message);
        public void Fatal(string message) => _logger.Fatal(message);
        public void Fatal(string message, Exception ex) => _logger.Fatal(ex, message);
    }
}
