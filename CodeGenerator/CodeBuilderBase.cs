namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     A base class for <see cref="ICodeBuilder"/>.
    /// </summary>
    public abstract class CodeBuilderBase : ICodeBuilder
    {
        /// <summary>
        ///     Gets the last written character of this <see cref="ICodeBuilder"/> or null if no character was written yet.
        /// </summary>
        public abstract char? LastChar { get; }

        /// <inheritdoc />
        public bool IsNewLine => LastChar is '\r' or '\n';

        /// <inheritdoc />
        public Indenting Indent()
        {
            return new(this);
        }

        /// <inheritdoc />
        public Scoping Scope()
        {
            return new(this);
        }

        /// <inheritdoc />
        public void EnsureNewLine()
        {
            if (!IsNewLine)
                AppendLine();
        }

        /// <inheritdoc />
        public void Append(string value)
        {
            AppendPrefix();
            Write(value);
        }

        /// <inheritdoc />
        public abstract void AppendLine();

        /// <inheritdoc />
        public abstract void AppendLine(string value);

        /// <inheritdoc />
        public abstract int Indentation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        public abstract void Write(string str);

        /// <summary>
        /// 
        /// </summary>
        protected void AppendPrefix()
        {
            if (!IsNewLine)
                return;
            for (var i = 0; i < Indentation; i++)
                Write("    ");
        }
    }
}