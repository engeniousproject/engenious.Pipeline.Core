using System;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Content context for a build process in the importing stage using <see cref="IContentImporter"/>.
    /// </summary>
    public class ContentImporterContext : ContentContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentImporterContext"/> class.
        /// </summary>
        /// <param name="buildId">The build id for the current building process.</param>
        /// <param name="createdContentCode">The created content code to write all generated code to.</param>
        /// <param name="contentDirectory">The directory the content is located in.</param>
        /// <param name="workingDirectory">The working directory for the building process.</param>
        public ContentImporterContext(Guid buildId, CreatedContentCode createdContentCode, string contentDirectory,
            string workingDirectory = "")
            : base(buildId, createdContentCode, contentDirectory, workingDirectory)
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }
    }
}