using System;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     <see cref="System.EventArgs"/> for
    /// </summary>
    public class BuildMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     Enumeration for the possible build message types.
        /// </summary>
        public enum BuildMessageType
        {
            /// <summary>
            ///     No specific build message type.
            /// </summary>
            None,
            /// <summary>
            ///     Informing about the success of an action.
            /// </summary>
            Success,
            /// <summary>
            ///     A warning message for the executed action.
            /// </summary>
            Warning,
            /// <summary>
            ///     A error message for the executed action.
            /// </summary>
            Error,
            /// <summary>
            ///     Addition information message for the executed action.
            /// </summary>
            Information
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BuildMessageEventArgs"/> class.
        /// </summary>
        /// <param name="filename">The file name the build message was issued for.</param>
        /// <param name="message">The message that was issued.</param>
        /// <param name="messageType">The <see cref="BuildMessageType"/> for the issued message.</param>
        public BuildMessageEventArgs(string filename, string message, BuildMessageType messageType)
        {
            FileName = filename;
            Message = message;
            MessageType = messageType;
        }

        /// <summary>
        ///     Gets the file name the build message was issued for.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Gets the message that was issued.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets the <see cref="BuildMessageType"/> for the issued message.
        /// </summary>
        public BuildMessageType MessageType { get; }
    }
}