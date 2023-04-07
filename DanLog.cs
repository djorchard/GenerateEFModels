//Install-Package NLog

using System.Globalization;
using System.Reflection;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = NLog.LogLevel;
// ReSharper disable InconsistentNaming

//  DanLog
//  Logging via NLog

//  ver 1.26 06/04/2023 Minor ReSharper fixes
//  ver 1.25 28/11/2022 Added Azure/Microsoft Graph Custom Debug Methods
//  ver 1.24 28/11/2022 Removed dependency on System.Threading.Timer
//  ver 1.23 27/11/2022 Moved folder Log -> Logs
//  ver 1.22 23/11/2022 ReSharper suggested improvements (variable renaming)
//  ver 1.21 14/10/2022 Improved deleting of old log files using NLog itself to roll archive files
//  ver 1.20 11/07/2022 ReSharper Optimizations
//  ver 1.19 14/06/2022 Fixed bug to dispose TimerLoggingNotEnabledAlert
//  ver 1.18 10/06/2022 Removed email send in Debug mode, Debug mode logging level can now be changed from the default (trace),
//                      Auto delete old log files
//  ver 1.16 27/05/2022 Removed DanSetting dependency
//  ver 1.15 27/05/2022 Added Application name in application log
//  ver 1.14 10/05/2022 Added back missing email on error code removed in previous version
//  ver 1.13 27/04/2022 Made coloured console optional
//  ver 1.12 01/02/2022 Improved log file caching/closing
//  ver 1.11 31/01/2022 Fixed infinite loop when sending an error email about failing to send an error email
//  ver 1.10 20/01/2022 Added colour to console output!
//  ver 1.09 17/01/2022 Fixed ambiguous reference with "LogLevel" (again.)
//  ver 1.08 16/01/2022 Added program version to debug text
//  ver 1.07 11/01/2022 Fixed bug setting log level!!
//  ver 1.06 09/01/2022 Fixed bug with log archiving (they are archived to the root folder not the Log sub folder)
//  ver 1.05 07/01/2022 Changed Log Level to use a MinLogLevel Option (Trace/Debug/Info)
//  ver 1.04 06/01/2022 added Diagnostics/Troubleshooting debug information on log startup
//  ver 1.03 04/01/2022 minor enhancements to improve robustness
//  ver 1.02 04/01/2022 minor fix: ambiguous reference with "LogLevel"
//  ver 1.01 23/08/2021 Changed it so i can call it with shorter syntax by adding wrapper functions
//  ver 1.00 06/04/2021

namespace Dan;

internal static class Log
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static bool _loggingEnabled;
    private static readonly LoggingConfiguration LoggingConfig = new();
    private static FileTarget? _fileTarget;
    private static readonly ConsoleTarget? _consoleTarget;
#if DEBUG
    private static readonly DebuggerTarget? _debugTarget;
#endif

    // ReSharper disable once NotAccessedField.Local
    private static MailTarget? _mailTarget;

    private static bool _sendingAlertEmail;


    static Log()
    {

        Task.Delay(TimeSpan.FromMilliseconds(3000))
            .ContinueWith(_ => LoggingNotEnabledAlert());

        LogManager.Configuration = LoggingConfig;
#if DEBUG
        _debugTarget = new DebuggerTarget { Layout = "${message}", Name = "DebugTarget" };
#endif
        _consoleTarget = new ConsoleTarget { Layout = "${message}", Name = "ConsoleTarget" };
    }

    public static string LogFileName { get; private set; } = "";

    private static void LoggingNotEnabledAlert()
    {
        if (!_loggingEnabled)
        {
            if (Options.ColourConsole)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine("You are using DanLog but you haven't called EnableLogging!");
            if (Options.ColourConsole)
            {
                Console.ResetColor();
            }

            System.Diagnostics.Debug.WriteLine("You are using DanLog but you haven't called EnableLogging!");
        }
    }

    public static bool EnableLogging(LogLevel? logLevel)
    {
        return EnableLogging("", logLevel);
    }

    public static bool EnableLogging(string logFileName = "", LogLevel? logLevel = null)
    {
        if (_loggingEnabled)
        {
            Warn("You are enabling logging more than once!");
        }

        _loggingEnabled = true;
        //set Default log file name
        if (logFileName.IsNullOrEmpty())
        {
            logFileName = $@"{GetAppFolder()}\Logs\{GetAppShortName()}-log.txt";
        }

        string? logFolder = Path.GetDirectoryName(logFileName);
        if (string.IsNullOrEmpty(logFolder))
        {
            if (Options.ColourConsole)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.Error.Write($"Failed to enable logging. Can not get folder name from log file name {logFileName}");
            if (Options.ColourConsole)
            {
                Console.ResetColor();
            }

            return false;
        }

        if (logLevel != null)
        {
            Options.MinLogLevel = logLevel;
        }

        if (!Directory.Exists(logFolder))
        {
            //create the log folder if it doesn't exist
            Directory.CreateDirectory(logFolder);
        }

        LogFileName = logFileName;

        // Targets:
        _fileTarget = new FileTarget { FileName = logFileName, Name = "FileTarget" };
        _fileTarget.KeepFileOpen = true;
        _fileTarget.OpenFileCacheTimeout = 5;
        _fileTarget.OpenFileFlushTimeout = 5;
        _fileTarget.ArchiveAboveSize = 3000000; // 3 mb
        _fileTarget.MaxArchiveDays = 60;
        _fileTarget.MaxArchiveFiles = 10;
        _fileTarget.EnableFileDelete = true;
        _fileTarget.ArchiveNumbering =
            ArchiveNumberingMode.Rolling; //Rolling style numbering (the most recent is always #0 then #1, ..., #N.
        _mailTarget = new MailTarget
        {
            Name = "MailTarget",
            Subject = GetAppShortName() + " Error",
            From = Options.Email.FromEmail,
            To = Options.Email.DeveloperEmail
        };

#if DEBUG
        Options.Email.SendOnError = false; //disable email logging in debug mode!
#endif

        AddLoggingRules();

        // log unhandled exceptions (only in current thread i think :( )
        AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

        if (Options.CleanUpOldFilesInLogFolder)
        {
            //delete old txt/png files in log folder (over 60 days)
            foreach (string file in Directory.GetFiles(logFolder))
            {
                var fi = new FileInfo(file);
                if (fi.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
                    fi.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    fi.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    if (fi.LastWriteTime < DateTime.Today.AddDays(-60))
                    {
                        Info($"Deleting Old Log Files: {fi.Name}");
                        File.Delete(file);
                    }
                }
            }
        }

        Logger.Info($"Logging started: {logFileName}");
        if (Options.ShowSystemInfoOnStartup)
        {
            SystemInfo();
        }

        return true;
    }

    private static void AddLoggingRules()
    {
        // Rules for mapping loggers to targets
        LoggingConfig.LoggingRules.Clear();
        if (_consoleTarget != null)
        {
            LoggingConfig.AddRule(Options.MinLogLevel, LogLevel.Fatal, _consoleTarget);
        }

        if (_fileTarget != null)
        {
            LoggingConfig.AddRule(Options.MinLogLevel, LogLevel.Fatal, _fileTarget);
        }
#if DEBUG
        if (_debugTarget != null)
        {
            LoggingConfig.AddRule(Options.MinLogLevel, LogLevel.Fatal, _debugTarget);
        }

#else
            if (_mailTarget != null) { LoggingConfig.AddRule(NLog.LogLevel.Fatal, NLog.LogLevel.Fatal, _mailTarget); }
#endif
        //Apply Config
        LogManager.Configuration = LoggingConfig;
        Debug($"Minimum Logging Level set to {Options.MinLogLevel}");
    }

    public static void Trace(string text)
    {
        if (Options.ColourConsole)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        Logger.Trace(text);
        if (Options.ColourConsole)
        {
            Console.ResetColor(); //Reset to default
        }
    }

    public static void Debug(string text)
    {
        if (Options.ColourConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        Logger.Debug(text);
        if (Options.ColourConsole)
        {
            Console.ResetColor(); //Reset to default
        }
    }

    public static void Info(string text, ConsoleColor? color = null)
    {
        if (color != null && Options.ColourConsole)
        {
            Console.ForegroundColor = (ConsoleColor)color;
        }

        Logger.Info(text);
        if (color != null && Options.ColourConsole)
        {
            Console.ResetColor(); //Reset to default
        }
    }

    public static void Warn(string text)
    {
        if (Options.ColourConsole)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        Logger.Warn("WARNING: " + text);
        if (Options.ColourConsole)
        {
            Console.ResetColor(); //Reset to default
        }
    }

    public static void Error(string text)
    {
        if (Options.ColourConsole)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Logger.Error("ERROR: " + text);
        if (Options.ColourConsole)
        {
            Console.ResetColor(); //Reset to default
        }

        if (Options.Email.SendOnError)
        {
            //send email in background thread without waiting
        }
    }

    public static void Error(Exception e, string text)
    {
        Logger.Error(e, text);
        string errorText =
            $"{e.Message}\n{e.InnerException}\n{e.GetType()}\n{e.Source}\n{e.TargetSite}\n{e.Data}\n{e.StackTrace}";
        if (Options.Email.SendOnError)
        {
            //send email in background thread without waiting
        }
    }

    public static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        try
        {
            Error((Exception)args.ExceptionObject, "");
        }
        catch (Exception ex)
        {
            if (Options.ColourConsole)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.Error.WriteLine(ex.ToString());
            if (Options.ColourConsole)
            {
                Console.ResetColor();
            }
        }
    }


    public static void SystemInfo()
    {
        Info(SystemInfoString());
    }

    public static string SystemInfoString()
    {
        var text = new StringBuilder();
        text.AppendLine("----------------------------------------");
        text.AppendLine("System Information:");
        text.AppendLine("----------------------------------------");
        text.AppendLine($"Directory: {Environment.CurrentDirectory}");
        text.AppendLine($"File:      {Assembly.GetExecutingAssembly().Location}");
        text.AppendLine($"LogFile:   {LogFileName}");
        text.AppendLine($"App:       {Assembly.GetCallingAssembly().GetName().Name ?? "UnknownApplicationName"}");
        text.AppendLine($"Version:   {GetAppVersion()}");
        text.AppendLine($"LogLevel:  {Options.MinLogLevel}");
        text.AppendLine($"Computer:  {Environment.MachineName}");
        text.AppendLine($"User:      {Environment.UserName}");
        text.AppendLine($"OS:        {Environment.OSVersion}");
        text.AppendLine($"CLR Ver:   {Environment.Version}");
        text.AppendLine($"TimeZone:  {TimeZoneInfo.Local.StandardName} +{TimeZoneInfo.Local.BaseUtcOffset}");
        text.AppendLine($"Culture:   {CultureInfo.CurrentCulture}");
        text.AppendLine($"UI Culture:{CultureInfo.CurrentUICulture}");
        text.AppendLine($"Domain:    {Environment.UserDomainName}");
        text.AppendLine("----------------------------------------");
        return text.ToString();
    }

    private static string GetAppFolder()
    {
        var ra = Assembly.GetExecutingAssembly();
        return Path.GetDirectoryName(ra.Location) ?? string.Empty;
    }

    private static string GetAppVersion()
    {
        Version? ver = Assembly.GetExecutingAssembly().GetName().Version;
        if (ver != null)
        {
            return $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }

        return "0";
    }

    private static string GetAppShortName()
    {
        return (Assembly.GetCallingAssembly().GetName().Name ?? "UnknownApplicationName").Replace(" ", "");
    }

    public static class Options
    {
        public static bool ColourConsole = false;
#if DEBUG
        public static LogLevel MinLogLevel = LogLevel.Trace;
#else
            public static NLog.LogLevel MinLogLevel = NLog.LogLevel.Info;
#endif

        //Use this method to adjust the log level while the application is running
        public static void SetMinLogLevel(string logLevel)
        {
            logLevel = logLevel.ToLower();
            LogLevel? newLogLevel = logLevel switch
            {
                "trace" => LogLevel.Trace,
                "debug" => LogLevel.Debug,
                _ => LogLevel.Info
            };
            if (newLogLevel != MinLogLevel)
            {
                MinLogLevel = newLogLevel;
                Info($"Log Level set to {MinLogLevel}");
                AddLoggingRules();
            }
        }

        //public static bool ScreenshotOnError = false;
        public static readonly bool ShowSystemInfoOnStartup = true;
        public static readonly bool CleanUpOldFilesInLogFolder = true;

        public static class Email
        {
            public static bool SendOnError = true;
            public static readonly string FromEmail = $"noreply-{GetAppShortName()}@volvo.com";
            public static readonly string DeveloperEmail = "daniel.barnes@volvo.com,oleksandr.vavilov@volvo.com";
        }
    }

}