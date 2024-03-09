namespace DropBear.Codex.Core.Enums;

/// <summary>
/// Defines event IDs for logging messages.
/// </summary>
public enum LoggerMessageEventIds
{
    /// <summary>
    /// Represents no specific event ID.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents an error log event.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Represents a warning log event.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Represents an informational log event.
    /// </summary>
    Information = 3,

    /// <summary>
    /// Represents a debug log event.
    /// </summary>
    Debug = 4,

    // Reserve 100-199 for custom application-specific events
}
