﻿using DropBear.Codex.Core.Enums;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Core.Extensions;

/// <summary>
/// Provides extension methods for logging various levels of messages.
/// </summary>
public static partial class LoggerMessageExtensions
{
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use for logging.</param>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">The exception related to the error, if any.</param>
    [LoggerMessage(EventId = (int)LoggerMessageEventIds.Error, Level = LogLevel.Error, Message = "An error occurred: {Message}")]
    public static partial void LmLogError(this ILogger logger, string message, Exception? exception = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use for logging.</param>
    /// <param name="message">The warning message to log.</param>
    /// <param name="exception">The exception related to the warning, if any.</param>
    [LoggerMessage(EventId = (int)LoggerMessageEventIds.Warning, Level = LogLevel.Warning, Message = "A warning occurred: {Message}")]
    public static partial void LmLogWarning(this ILogger logger, string message, Exception? exception = null);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use for logging.</param>
    /// <param name="message">The informational message to log.</param>
    /// <param name="exception">The exception related to the message, if any.</param>
    [LoggerMessage(EventId = (int)LoggerMessageEventIds.Information, Level = LogLevel.Information, Message = "An informational message: {Message}")]
    public static partial void LmLogInformation(this ILogger logger, string message, Exception? exception = null);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use for logging.</param>
    /// <param name="message">The debug message to log.</param>
    /// <param name="exception">The exception related to the debug message, if any.</param>
    [LoggerMessage(EventId = (int)LoggerMessageEventIds.Debug, Level = LogLevel.Debug, Message = "A debug message: {Message}")]
    public static partial void LmLogDebug(this ILogger logger, string message, Exception? exception = null);
}
