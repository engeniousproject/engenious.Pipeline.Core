using System;
using System.Collections.Generic;
using System.IO;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Base class for content context in the building process
    ///     (used in <see cref="IContentImporter"/> and <see cref="IContentProcessor"/>).
    /// </summary>
    public abstract class ContentContext : IContentContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentContext"/> class.
        /// </summary>
        /// <param name="buildId">The build id for the current building process.</param>
        /// <param name="createdContentCode">The created content code to write all generated code to.</param>
        /// <param name="contentDirectory">The directory the content is located in.</param>
        /// <param name="workingDirectory">The working directory for the building process.</param>
        protected ContentContext(Guid buildId, CreatedContentCode createdContentCode, string contentDirectory,
            string workingDirectory = "")
        {
            BuildId = buildId;
            CreatedContentCode = createdContentCode;
            ContentDirectory = contentDirectory;
            Dependencies = new List<string>();
            if (workingDirectory.Length > 0)
            {
                var lastChar = workingDirectory[^1];
                workingDirectory = lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar
                    ? workingDirectory
                    : workingDirectory + Path.DirectorySeparatorChar;
            }

            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        ///     Gets the build id for the current building process.
        /// </summary>
        public Guid BuildId { get; }

        /// <summary>
        ///     Gets the created content code to write all generated code to.
        /// </summary>
        public CreatedContentCode CreatedContentCode { get; }

        /// <summary>
        ///     Gets the directory the content is located in.
        /// </summary>
        public string ContentDirectory { get; }

        /// <summary>
        ///     Gets the working directory for the building process.
        /// </summary>
        public string WorkingDirectory { get; }

        /// <inheritdoc />
        public event BuildMessageDel? BuildMessage;
        
        /// <inheritdoc />
        public List<string> Dependencies { get; }


        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public void RaiseBuildMessage(string filename, string message,
            BuildMessageEventArgs.BuildMessageType messageType)
        {
            BuildMessage?.Invoke(this, new BuildMessageEventArgs(filename, message, messageType));
        }

        /// <summary>
        ///     Adds a dependency to the current building step.
        /// </summary>
        /// <param name="file">The file the current building step depends on.</param>
        public void AddDependency(string file)
        {
            Dependencies.Add(file);
        }

        /// <summary>
        ///     Creates a relative path from a given path to the <see cref="WorkingDirectory"/>.
        /// </summary>
        /// <param name="subPath">The path to make relative to the <see cref="WorkingDirectory"/>.</param>
        /// <returns>The created relative path.</returns>
        public string GetRelativePathToWorkingDirectory(string subPath)
        {
            return GetRelatiePath(subPath, WorkingDirectory);
        }

        /// <summary>
        ///     Creates a relative path from a given path to the <see cref="ContentDirectory"/>.
        /// </summary>
        /// <param name="subPath">The path to make relative to the <see cref="ContentDirectory"/>.</param>
        /// <returns>The created relative path.</returns>
        public string GetRelativePathToContentDirectory(string subPath)
        {
            return GetRelatiePath(subPath, ContentDirectory);
        }

        private static string GetRelatiePath(string subPath, string relativeToPath)
        {
            try
            {
                var parentUri = new Uri(relativeToPath);
                var subUri = new Uri(subPath);
                var relUri = parentUri.MakeRelativeUri(subUri);
                return relUri.ToString();
            }
            catch (Exception)
            {
                return subPath;
            }
        }
    }
}