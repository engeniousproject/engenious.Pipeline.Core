using System;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Interface for content processors.
    /// </summary>
    public interface IContentProcessor
    {
        /// <summary>
        ///     Gets or sets the settings associated with this content processor.
        /// </summary>
        ProcessorSettings? Settings { get; set; }

        /// <summary>
        ///     Gets the type the content processor can import from.
        /// </summary>
        Type ImportType { get; }

        /// <summary>
        ///     Gets the type the content processor exports to.
        /// </summary>
        Type ExportType { get; }

        /// <summary>
        ///     Processes imported input from <see cref="IContentImporter"/>.
        /// </summary>
        /// <param name="input">The imported input of type <see cref="ImportType"/> to process.</param>
        /// <param name="filename">The name of the content file to process.</param>
        /// <param name="context">The context for the processing.</param>
        /// <returns>The processed content of type <see cref="ExportType"/> or <c>null</c> if processing failed.</returns>
        object? Process(object input, string filename, ContentProcessorContext context);
    }
}