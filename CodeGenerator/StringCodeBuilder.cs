using System.Text;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     A code builder that writes to a <see cref="StringBuilder"/>.
    /// </summary>
    public class StringCodeBuilder : CodeBuilderBase
    {
        private readonly StringBuilder _builder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringCodeBuilder"/> class.
        /// </summary>
        public StringCodeBuilder() : this(new StringBuilder())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringCodeBuilder"/> class.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to write the code to.</param>
        public StringCodeBuilder(StringBuilder builder)
        {
            _builder = builder;
        }

        /// <inheritdoc />
        public override char? LastChar => _builder.Length == 0 ? null : _builder[_builder.Length - 1];

        /// <inheritdoc />
        public override int Indentation { get; set; }


        /// <inheritdoc />
        public override void Write(string str)
        {
            _builder.Append(str);
        }

        /// <inheritdoc />
        public override void AppendLine()
        {
            _builder.AppendLine();
        }

        /// <inheritdoc />
        public override void AppendLine(string value)
        {
            AppendPrefix();
            _builder.AppendLine(value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}