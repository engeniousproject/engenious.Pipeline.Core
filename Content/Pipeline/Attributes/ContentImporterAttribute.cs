using System;
using System.Collections.Generic;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Attribute to apply to classes implementing <see cref="IContentImporter"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentImporterAttribute : Attribute
    {
        private readonly List<string> _fileExtensions;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentImporterAttribute"/> class.
        /// </summary>
        /// <param name="fileExtensions">The file extensions that the <see cref="IContentImporter"/> works on.</param>
        public ContentImporterAttribute(params string[] fileExtensions)
        {
            _fileExtensions = new List<string>();
            foreach (string ext in fileExtensions)
                _fileExtensions.Add(ext.ToUpperInvariant());
        }

        /// <summary>
        ///     Gets the file extensions that the <see cref="IContentImporter"/> works on.
        /// </summary>
        public IEnumerable<string> FileExtensions => _fileExtensions;

        /// <summary>
        ///     Gets or sets the displayed name for the <see cref="IContentImporter"/>.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the default <see cref="IContentProcessor"/> name to use.
        ///     <c>null</c> to fallback to the best matching processor.
        /// </summary>
        public string? DefaultProcessor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether imported data should be cached.
        /// </summary>
        public bool CacheImportedData { get; set; }
    }
}