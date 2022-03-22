using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NonSucking.Framework.Serialization;
namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     A class for building combined expressions from other expressions.
    /// </summary>
    public class ExpressionBuilder : IDisposable
    {
        private readonly ExpressionBuilder? _parent;
        private readonly List<CodeExpressionDefinition> _lines = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpressionBuilder"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="ExpressionBuilder"/> to append this expression to or <c>null</c>.</param>
        public ExpressionBuilder(ExpressionBuilder? parent = null)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _parent?.Append(ToBlockExpression());
        }

        /// <summary>
        ///     Append an expression line to this <see cref="ExpressionBuilder"/>.
        /// </summary>
        /// <param name="expression">The expression to append.</param>
        public void Append(CodeExpressionDefinition expression)
        {
            _lines.Add(expression);
        }

        /// <summary>
        ///     Converts this expression to the most fitting expression type
        ///     (Either a singular expression or a <see cref="BlockExpressionDefinition"/>).
        /// </summary>
        /// <returns></returns>
        public CodeExpressionDefinition ToExpression()
        {
            return _lines.Count == 1
                ? _lines[0]
                : ToBlockExpression();
        }

        /// <summary>
        ///     Converts this expression to a <see cref="MultilineExpressionDefinition"/>.
        /// </summary>
        /// <returns>The newly created <see cref="MultilineExpressionDefinition"/>.</returns>
        public MultilineExpressionDefinition ToMultilineExpression()
        {
            return new MultilineExpressionDefinition(_lines.ToArray());
        }

        /// <summary>
        ///     Converts this expression to a <see cref="BlockExpressionDefinition"/>.
        /// </summary>
        /// <returns>The newly created <see cref="BlockExpressionDefinition"/>.</returns>
        public BlockExpressionDefinition ToBlockExpression()
        {
            return new BlockExpressionDefinition(ToMultilineExpression());
        }
    }

    /// <summary>
    ///     Represents a simple expression using a <see cref="string"/>.
    /// </summary>
    /// <param name="Expression">The string value for the expression</param>
    /// <param name="Indentation">The indentation level of this expression.</param>
    [Nooson]
    public partial record SimpleExpressionDefinition(string Expression, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Expression;
        }

        /// <inheritdoc />
        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            yield return (Indentation, Expression, false);
        }

        /// <summary>
        ///     Tries to convert an expression from the Expression API to a <see cref="SimpleExpressionDefinition"/>.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>The resulting <see cref="SimpleExpressionDefinition"/>.</returns>
        public static implicit operator SimpleExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<SimpleExpressionDefinition>(expression);
        }
    }

    /// <summary>
    ///     A base class for all code expressions.
    /// </summary>
    /// <param name="Indentation">The indentation level of this expression.</param>
    [NoosonDynamicType(typeof(BinaryExpressionDefinition),
        typeof(BlockExpressionDefinition),
        typeof(MultilineExpressionDefinition),
        typeof(SimpleExpressionDefinition))]
    public abstract partial record CodeExpressionDefinition(int Indentation = 0) : ICode
    {
        /// <inheritdoc />
        public virtual void WriteTo(ICodeBuilder builder)
        {
            builder.Indentation += Indentation;
            if (!builder.IsNewLine && Indentation != 0)
                builder.AppendLine();
            foreach (var (indentation, line, ensureNewLine) in ToCode())
            {
                builder.Indentation += indentation;
                if (!builder.IsNewLine && indentation != 0)
                    builder.AppendLine();
                else if (ensureNewLine)
                    builder.EnsureNewLine();
                builder.Append(line);
                builder.Indentation -= indentation;
            }

            builder.Indentation -= Indentation;
        }

        /// <summary>
        ///     Converts this <see cref="CodeExpressionDefinition"/> into code text lines with specific indentations.
        /// </summary>
        /// <returns>An enumeration of the code lines in this <see cref="CodeExpressionDefinition"/>.</returns>
        public abstract IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode();
        
        /// <summary>
        ///     Tries to convert a <see cref="string"/> to a <see cref="CodeExpressionDefinition"/>.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>The resulting <see cref="CodeExpressionDefinition"/>.</returns>
        public static implicit operator CodeExpressionDefinition(string expression)
        {
            return new SimpleExpressionDefinition(expression); // TODO: perhaps multiline expressions as well
        }

        /// <summary>
        ///     Tries to convert an expression from the Expression API to a <see cref="MultilineExpressionDefinition"/>.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>The resulting <see cref="MultilineExpressionDefinition"/>.</returns>
        public static implicit operator CodeExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<CodeExpressionDefinition>(expression);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringCodeBuilder();
            WriteTo(builder);
            return builder.ToString();
        }
    }

    /// <summary>
    ///     Represents a collection of lines of expressions.
    /// </summary>
    /// <param name="Lines">The lines this expression consists of.</param>
    /// <param name="Indentation">The indentation level of this expression.</param>
    [Nooson]
    public partial record MultilineExpressionDefinition(List<CodeExpressionDefinition> Lines, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        /// <inheritdoc />
        public MultilineExpressionDefinition(IEnumerable<CodeExpressionDefinition> initialLines, int indentation = 0)
            : this(new List<CodeExpressionDefinition>(initialLines), indentation)
        {
                
        }

        /// <inheritdoc />
        public MultilineExpressionDefinition(int indentation = 0)
            : this(new List<CodeExpressionDefinition>(), indentation)
        {
                
        }
        
        /// <inheritdoc />
        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            return Lines.Select(x => (x.Indentation, x.ToString(), true));
        }

        /// <summary>
        ///     Tries to convert an expression from the Expression API to a <see cref="MultilineExpressionDefinition"/>.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>The resulting <see cref="MultilineExpressionDefinition"/>.</returns>
        public static implicit operator MultilineExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<MultilineExpressionDefinition>(expression);
        }
    }

    /// <summary>
    ///     Represents a block of expressions.
    /// </summary>
    /// <param name="ScopeContent">The expressions inside the scope of the block.</param>
    /// <param name="Indentation">The indentation level of this expression.</param>
    [Nooson]
    public partial record BlockExpressionDefinition(CodeExpressionDefinition ScopeContent, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        /// <inheritdoc />
        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            return ScopeContent
                .ToCode().Select((i, _) => (i.indentation + Indentation + 1, i.line, i.ensureNewLine))
                .Prepend((Indentation, "{", true)).Append((Indentation, "}" + Environment.NewLine, true));
        }

        /// <summary>
        ///     Tries to convert an expression from the Expression API to a <see cref="BlockExpressionDefinition"/>.
        /// </summary>
        /// <param name="expression">The expression to convert</param>
        /// <returns>The resulting <see cref="BlockExpressionDefinition"/>.</returns>
        public static implicit operator BlockExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<BlockExpressionDefinition>(expression);
        }
    }

    /// <summary>
    ///     Represents a binary expression.
    /// </summary>
    /// <param name="Left">The left hand side expression.</param>
    /// <param name="Right">The right hand side expression.</param>
    /// <param name="Operator">The operator to use on the expressions.</param>
    /// <param name="Indentation">The indentation level of this expression.</param>
    [Nooson]
    public partial record BinaryExpressionDefinition(CodeExpressionDefinition Left,
            CodeExpressionDefinition Right, string Operator, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        /// <inheritdoc />
        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            yield return (Indentation, $"{Left} {Operator} {Right}", false);
        }
    }
}