using System;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Interface for content importers.
    /// </summary>
    public interface IContentImporter
    {
        /// <summary>
        ///     Gets the type the content importer exports to.
        /// </summary>
        Type ExportType { get; }

        /// <summary>
        ///     Imports a content file.
        /// </summary>
        /// <param name="filename">The name of the content file to import.</param>
        /// <param name="context">The context for the importing.</param>
        /// <returns>The imported content of type <see cref="ExportType"/> or <c>null</c> if importing failed.</returns>
        object? Import(string filename, ContentImporterContext context);
    }
}