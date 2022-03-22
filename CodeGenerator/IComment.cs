namespace engenious.Content.CodeGenerator
{
    /// <summary>
    /// Interface for code structures that can have comments.
    /// </summary>
    public interface IComment
    {
        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        string? Comment { get; init; }
    }
}