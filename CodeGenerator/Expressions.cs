using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace engenious.Content.CodeGenerator
{
    public class ExpressionBuilder : IDisposable
    {
        private readonly ExpressionBuilder? _parent;
        private List<CodeExpressionDefinition> _lines = new();

        public ExpressionBuilder(ExpressionBuilder? parent)
        {
            _parent = parent;
        }

        public void Dispose()
        {
            _parent?.Append(ToBlockExpression());
        }

        public void Append(CodeExpressionDefinition expression)
        {
            _lines.Add(expression);
        }

        public CodeExpressionDefinition ToExpression()
        {
            return _lines.Count == 1
                ? _lines[0]
                : ToBlockExpression();
        }

        public MultilineExpressionDefinition ToMultilineExpression()
        {
            return new MultilineExpressionDefinition(_lines.ToArray());
        }

        public BlockExpressionDefinition ToBlockExpression()
        {
            return new BlockExpressionDefinition(ToMultilineExpression());
        }
    }

    [Serializable]
    public record SimpleExpressionDefinition(string Expression, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        public override string ToString() => Expression;

        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            yield return (Indentation, Expression, false);
        }

        public static implicit operator SimpleExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<SimpleExpressionDefinition>(expression);
        }
    }

     [Serializable]
    public abstract record CodeExpressionDefinition(int Indentation = 0) : ICode
    {
        public abstract IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode();

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

        public static implicit operator CodeExpressionDefinition(string expression)
        {
            return new SimpleExpressionDefinition(expression, 0);
        }

        public static implicit operator CodeExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<CodeExpressionDefinition>(expression);
        }

        public override string ToString()
        {
            var builder = new StringCodeBuilder();
            WriteTo(builder);
            return builder.ToString();
        }
    }

    [Serializable]
    public record MultilineExpressionDefinition(CodeExpressionDefinition[] Lines, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        public override string ToString() => base.ToString();

        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode() =>
            Lines.Select(x => (x.Indentation, x.ToString(), true));

        public static implicit operator MultilineExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<MultilineExpressionDefinition>(expression);
        }
    }

    [Serializable]
    public record BlockExpressionDefinition(CodeExpressionDefinition ScopeContent, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        public override string ToString() => base.ToString();

        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode() => ScopeContent
            .ToCode().Select((i, _) => (i.indentation + Indentation + 1, i.line, i.ensureNewLine))
            .Prepend((Indentation, "{", true)).Append((Indentation, "}" + Environment.NewLine, true));

        public static implicit operator BlockExpressionDefinition(Expression expression)
        {
            return ExpressionConverter.Convert<BlockExpressionDefinition>(expression);
        }
    }

    [Serializable]
    public record BinaryExpressionDefinition(CodeExpressionDefinition Left,
            CodeExpressionDefinition Right, string Operator, int Indentation = 0)
        : CodeExpressionDefinition(Indentation)
    {
        public override string ToString() => base.ToString();

        public override IEnumerable<(int indentation, string line, bool ensureNewLine)> ToCode()
        {
            yield return (Indentation, $"{Left} {Operator} {Right}", false);
        }
    }
}