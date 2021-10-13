using System;
using System.IO;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     A code builder that writes to a <see cref="StreamWriter"/>.
    /// </summary>
    public class StreamWriterCodeBuilder : CodeBuilderBase, IDisposable
    {
        private readonly StreamWriter _writer;
        private char? _lastChar;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StreamWriterCodeBuilder"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to write the code to.</param>
        public StreamWriterCodeBuilder(StreamWriter writer)
        {
            _writer = writer;
        }

        /// <inheritdoc />
        public override char? LastChar => _lastChar;

        /// <inheritdoc />
        public override int Indentation { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            _writer.Dispose();
        }

        /// <inheritdoc />
        public override void Write(string str)
        {
            if (str.Length != 0)
                _lastChar = str[^1];
            _writer.Write(str);
        }

        /// <inheritdoc />
        public override void AppendLine()
        {
            Write(Environment.NewLine);
        }

        /// <inheritdoc />
        public override void AppendLine(string value)
        {
            AppendPrefix();
            Write(value);
            AppendLine();
        }
    }
}