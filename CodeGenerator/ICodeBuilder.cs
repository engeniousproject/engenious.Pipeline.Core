namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Interface for code builders which can generate text code from <see cref="ICode"/>.
    /// </summary>
    public interface ICodeBuilder
    {
        /// <summary>
        ///     Gets or sets the current indentation of this <see cref="ICodeBuilder"/>.
        /// </summary>
        int Indentation { get; set; }
        /// <summary>
        ///     Gets or sets whether the current position is on a new line.
        /// </summary>
        bool IsNewLine { get; }
        /// <summary>
        ///     Ensures that the <see cref="ICodeBuilder"/> is on a new line.
        /// </summary>
        void EnsureNewLine();
        /// <summary>
        ///     Appends a string to the <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to append.</param>
        void Append(string value);
        /// <summary>
        ///     Append a new empty line to the <see cref="ICodeBuilder"/>.
        /// </summary>
        void AppendLine();
        /// <summary>
        ///     Append a new line to the <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="value">The line to append.</param>
        void AppendLine(string value);

        /// <summary>
        ///     Creates a <see cref="Indenting"/> struct helper which is disposable and indents on creation un-indents on dispose.
        /// </summary>
        /// <returns>The <see cref="Indenting"/> struct.</returns>
        Indenting Indent();
        /// <summary>
        ///     Creates a <see cref="Scoping"/> struct helper which is disposable,
        ///     appends a '{' character and indents on creation;
        ///     un-indents and appends a '}' on dispose.
        /// </summary>
        /// <returns>The <see cref="Scoping"/> struct.</returns>
        Scoping Scope();
    }
}