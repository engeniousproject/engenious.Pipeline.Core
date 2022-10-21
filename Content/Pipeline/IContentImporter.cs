using System;
using System.Collections.Generic;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Interface for content importers.
    /// </summary>
    public interface IContentImporter
    {
        /// <summary>
        ///     Gets the type the content importer can import from.
        /// </summary>
        Type? DependencyType { get; }

        /// <summary>
        ///     Gets the type the content importer exports to.
        /// </summary>
        Type ExportType { get; }

        /// <summary>
        ///     Imports a content file.
        /// </summary>
        /// <param name="filename">The name of the content file to import.</param>
        /// <param name="context">The context for the importing.</param>
        /// <param name="dependencyImport">The dependency type instance to import with.</param>
        /// <returns>The imported content of type <see cref="ExportType"/> or <c>null</c> if importing failed.</returns>
        object? Import(string filename, ContentImporterContext context, object? dependencyImport = null);

        /// <summary>
        ///     Imports dependencies for a content file.
        /// </summary>
        /// <param name="filename">The name of the content file to import dependencies from.</param>
        /// <param name="context">The context for the importing.</param>
        /// <param name="dependencies">The list to add collected dependencies to.</param>
        /// <returns>The imported dependency import of type <see cref="DependencyType"/>.</returns>
        object? DependencyImportBase(string filename, ContentImporterContext context, ICollection<string> dependencies);
    }
}