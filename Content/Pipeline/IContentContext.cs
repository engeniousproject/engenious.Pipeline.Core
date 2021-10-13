using System;
using System.Collections.Generic;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Delegate to issue build messages in a <see cref="IContentContext.BuildMessage"/> event.
    /// </summary>
    public delegate void BuildMessageDel(object sender, BuildMessageEventArgs e);

    /// <summary>
    ///     Interface for context of the processing pipeline.
    /// </summary>
    public interface IContentContext : IDisposable
    {
        // TODO: Used for temporary storage per build file, prevents being able to multithreading of content build
        /// <summary>
        ///     Gets the dependencies used for the current building step.
        /// </summary>
        List<string> Dependencies { get; }

        /// <summary>
        ///     Raises the <see cref="BuildMessage"/> event.
        /// </summary>
        /// <param name="filename">The file the build message was issued from.</param>
        /// <param name="message">The message that was issued.</param>
        /// <param name="messageType">The message type that was issued.</param>
        void RaiseBuildMessage(string filename, string message, BuildMessageEventArgs.BuildMessageType messageType);

        /// <summary>
        ///     Occurs when a build message was issued.
        /// </summary>
        event BuildMessageDel? BuildMessage;
    }
}