﻿using Doorways;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Animations;

public enum LogLevel
{
    Error = 2,
    Warn = 1,
    Info = 0,
    Debug = -1,
    Trace = -2,
}

/// <summary>
/// A simple Logging class that outputs to
/// Cultist Simulator's internal log system.
/// <para />
/// Initialize it using <c>Logger::Initialize(..)</c>.
/// Calling <c>Logger::Initialize</c> more than once will
/// overwrite previously initialized values.
/// <para />
/// You may then call <c>Logger::Log(Logger::LogLevel level, string format, params object[] args)</c>
/// to print to the log.
/// <para />
/// You may also call 
/// <c>Logger::(Error, Warn, Info, etc)(string format, params object[] args)</c>
/// as convenience functions.
/// <para />
/// Finally, you may use <c>Span</c>s to include more helpful information
/// about your log statements. See the documentation on <c>Logger::Span</c>
/// for more information.
/// </summary>
public class Logger
{
    #region Static Init
    internal static LogLevel MinLogLevel;

    /// <summary>
    /// Set up the global logger so it can send messages to the game console.
    /// </summary>
    ///     <param name="modName">The name of of the mod sending 
    ///     log messages, as it should appear in the game log.
    /// </param>
    /// <param name="level">
    ///     The minimum level to emit to the game log. 
    ///     For example, setting it to <c>LogLevel.Warning</c> 
    ///     will cause only Warning and Error messages to be 
    ///     emitted. If it is set to <c>LogLevel.Info</c>, all 
    ///     messages will be emitted.
    /// </param>
    internal static void Initialize(LogLevel level = LogLevel.Info)
    {
        Logger.MinLogLevel = level;
    }
    #endregion

    // For internal convenience.
    internal static Logger Instance { get; } = new Logger("Doorways");

    private string LoggerName;

    /// <summary>
    /// Create a new Logger with an explicitly
    /// declared logger name.
    /// </summary>
    public Logger(string loggerName)
    {
        LoggerName = loggerName;
    }

    /// <summary>
    /// Create a new Logger with an auto-derived
    /// logger name. If the calling assembly is a
    /// Doorways Object with a set name, it will use
    /// that name. Otherwise, it will use the calling
    /// assembly name.
    /// </summary>
    public Logger()
    {
        Assembly caller = Assembly.GetCallingAssembly();
        DoorwaysAttribute attr = caller.GetCustomAttribute<DoorwaysAttribute>();
        if (attr != null && attr.Name != null)
        {
            LoggerName = attr.Name;
        }
        else
        {
            LoggerName = caller.GetName().Name;
        }
    }

    /// <summary>
    /// Clamp the current log level to a Core-compatible level.
    /// </summary>
    public static VerbosityLevel ClampedLogLevel()
    {
        switch (MinLogLevel)
        {
            case LogLevel.Error:
            case LogLevel.Warn:
                return VerbosityLevel.Essential;
            case LogLevel.Info:
                return VerbosityLevel.Significants;
            case LogLevel.Debug:
                return VerbosityLevel.SystemChatter;
            case LogLevel.Trace:
                return VerbosityLevel.Trivia;
            default:
                throw new InvalidOperationException("This should never be reached!");
        }
    }

    private string Format(LogLevel level, string context, string format, params object[] args)
    {
        if (context == null)
        {
            return String.Concat("[", LoggerName, "]", level.ToString(), ": ", String.Format(format, args));
        }
        else
        {
            return String.Concat("[", LoggerName, ":", context, "]", level.ToString(), ": ", String.Format(format, args));
        }
    }

    public void Log(LogLevel level, string format, params object[] args)
    {
        Log(null, level, null, format, args);
    }

    public void Log(string rootName, LogLevel level, string context, string format, params object[] args)
    {
        if ((int)level >= (int)MinLogLevel)
        {
            string message = Format(level, context, format, args);
            switch (level)
            {
                case LogLevel.Error:
                    NoonUtility.LogWarning(message);
                    break;
                case LogLevel.Warn:
                    NoonUtility.LogWarning(message);
                    break;
                case LogLevel.Info:
                    NoonUtility.Log(message, verbosityNeeded: VerbosityLevel.Essential);
                    break;
                case LogLevel.Debug:
                    NoonUtility.Log(message, verbosityNeeded: VerbosityLevel.SystemChatter);
                    break;
                case LogLevel.Trace:
                    NoonUtility.Log(message, verbosityNeeded: VerbosityLevel.Trivia);
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown Log Level \"{}\"", level.ToString()));
            }
        }
    }

    /// <summary>
    /// This function should NOT be used for irrecoverable errors!
    /// Internally, this just results in a "WARNING" line in the
    /// game log with the "Error" text. For irrecoverable errors,
    /// throw an exception.
    /// </summary>
    public void Error(string format, params object[] args)
    {
        Log(LogLevel.Error, format, args);
    }

    public void Error(object item)
    {
        Error("{}", item);
    }

    public void Warn(string format, params object[] args)
    {
        Log(LogLevel.Warn, format, args);
    }

    public void Warn(object item)
    {
        Warn("{}", item);
    }

    public void Info(string format, params object[] args)
    {
        Log(LogLevel.Info, format, args);
    }

    public void Info(object item)
    {
        Info("{}", item);
    }

    public void Debug(string format, params object[] args)
    {
        Log(LogLevel.Debug, format, args);
    }

    public void Debug(object item)
    {
        Debug("{}", item);
    }

    public void Trace(string format, params object[] args)
    {
        Log(LogLevel.Trace, format, args);
    }

    public void Trace(object item)
    {
        Trace("{}", item);
    }

    /// <summary>
    /// Create a new RAII guard that can be used to submit
    /// log messages. Spans created using this method
    /// intelligently parse the method they are created in
    /// and include that information in log statements
    /// automatically.
    /// </summary>
    public Span Span([CallerMemberName] string memberName = "", string rootName = null)
    {
        MethodBase mth = new StackTrace().GetFrame(1).GetMethod();
        string cls = mth.ReflectedType.Name;
        return new Span(this, String.Concat(cls, ".", memberName), rootName);
    }

    private static Span _UnityExplorerSpan;
    internal Action<string, UnityEngine.LogType> GetUnityExplorerListener()
    {
        _UnityExplorerSpan = this.Span("UEXP");
        return LogUnityExplorer;
    }
    private static void LogUnityExplorer(string message, UnityEngine.LogType severity)
    {
        switch (severity)
        {
            case UnityEngine.LogType.Log:
                _UnityExplorerSpan.Info(message);
                break;
            case UnityEngine.LogType.Warning:
                _UnityExplorerSpan.Warn(message);
                break;
            case UnityEngine.LogType.Assert:
            case UnityEngine.LogType.Error:
                _UnityExplorerSpan.Error(message);
                break;
            case UnityEngine.LogType.Exception:
                throw new Exception(message);
            default:
                _UnityExplorerSpan.Warn("UNKNOWN LOGLEVEL: {0}", message);
                break;
        }
    }
}

/// <summary>
/// A <c>Span</c> allows you to embed some additional context
/// information into your log messages.
/// 
/// Spans created using the <c>Logger::Span()</c> method
/// intelligently parse the method they are create in
/// and include that information in log statements
/// automatically. You can also use <c>Span::ctor(string)</c>
/// to create a Span with a custom message.
/// </summary>
public class Span
{
    public string Context { get; private set; }
    public string Assembly { get; private set; }

    public Logger Parent { get; private set; }

    internal Span(Logger parent, string context, string assembly = null)
    {
        Parent = parent;
        Context = context;
        Assembly = assembly;
    }

    public void Log(LogLevel level, string format, params object[] args)
    {
        Parent.Log(this.Assembly, level, this.Context, format, args);
    }

    /// <summary>
    /// This function should NOT be used for irrecoverable errors!
    /// Internally, this just results in a "WARNING" line in the
    /// game log with the "Error" text. For irrecoverable errors,
    /// throw an exception.
    /// </summary>
    public void Error(string format, params object[] args)
    {
        this.Log(LogLevel.Error, format, args);
    }

    public void Error(object item)
    {
        this.Error("{}", item);
    }

    public void Warn(string format, params object[] args)
    {
        this.Log(LogLevel.Warn, format, args);
    }

    public void Warn(object item)
    {
        this.Warn("{}", item);
    }

    public void Info(string format, params object[] args)
    {
        this.Log(LogLevel.Info, format, args);
    }

    public void Info(object item)
    {
        this.Info("{}", item);
    }

    public void Debug(string format, params object[] args)
    {
        this.Log(LogLevel.Debug, format, args);
    }

    public void Debug(object item)
    {
        this.Debug("{}", item);
    }

    public void Trace(string format, params object[] args)
    {
        this.Log(LogLevel.Trace, format, args);
    }

    public void Trace(object item)
    {
        this.Trace("{}", item);
    }
}