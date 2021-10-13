using System;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Attribute to apply to classes implementing <see cref="IContentProcessor"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentProcessorAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the displayed name for the <see cref="IContentProcessor"/>.
        /// </summary>
        public virtual string? DisplayName { get; set; }
    }
}