using System;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Helper struct to create indentation blocks.
    /// </summary>
    public readonly struct Indenting : IDisposable
    {
        private readonly ICodeBuilder _builder;
        /// <summary>
        ///     Indents.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to create the indentation for.</param>
        public Indenting(ICodeBuilder builder)
        {
            _builder = builder;
            builder.Indentation++;
        }

        /// <summary>
        ///     Un-Indents.
        /// </summary>
        public void Dispose()
        {
            _builder.Indentation--;
        }
    }
}