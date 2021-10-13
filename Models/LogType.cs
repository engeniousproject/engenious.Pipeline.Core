namespace engenious.Content.Models
{
    /// <summary>
    ///     The type of a log message.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        ///     No specific type.
        /// </summary>
        None,

        /// <summary>
        ///     The operation was a success.
        /// </summary>
        Success,

        /// <summary>
        ///     Additional information on an operation.
        /// </summary>
        Information,

        /// <summary>
        ///     The operation issued a warning.
        /// </summary>
        Warning,

        /// <summary>
        ///     The operation resulted in an error.
        /// </summary>
        Error
    }
}