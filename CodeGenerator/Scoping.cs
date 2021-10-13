using System;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Helper struct to create scoping blocks.
    /// </summary>
    public readonly struct Scoping : IDisposable
    {
        private readonly ICodeBuilder _builder;

        private readonly Indenting _indenting;

        /// <summary>
        ///     Appends a '{' character and indents.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to create the scoping for.</param>
        public Scoping(ICodeBuilder builder)
        {
            _builder = builder;
            builder.EnsureNewLine();
            builder.AppendLine("{");
            _indenting = builder.Indent();
        }


        /// <summary>
        ///     Un-Indents and appends a '}' character.
        /// </summary>
        public void Dispose()
        {
            _indenting.Dispose();
            _builder.AppendLine("}");
        }
    }
}