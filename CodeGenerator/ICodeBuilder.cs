using System;
using System.IO;
using System.Text;

namespace engenious.Content.CodeGenerator
{
    public struct Indenting : IDisposable
    {
        private readonly ICodeBuilder _builder;

        public Indenting(ICodeBuilder builder)
        {
            _builder = builder;
            builder.Indentation++;
        }


        public readonly void Dispose()
        {
            _builder.Indentation--;
        }
    }
    public struct Scoping : IDisposable
    {
        private readonly ICodeBuilder _builder;

        private readonly Indenting _indenting;

        public Scoping(ICodeBuilder builder)
        {
            _builder = builder;
            builder.EnsureNewLine();
            builder.AppendLine("{");
            _indenting = builder.Indent();
        }


        public readonly void Dispose()
        {
            _indenting.Dispose();
            _builder.AppendLine("}");
        }
    }
    public interface ICodeBuilder
    {
        void EnsureNewLine();
        void Append(string value);
        void AppendLine();
        void AppendLine(string value);
        int Indentation { get; set; }
        bool IsNewLine { get; }

        Indenting Indent();
        Scoping Scope();
    }

    public abstract class CodeBuilderBase : ICodeBuilder
    {
        public abstract char? LastChar { get; }
        public bool IsNewLine => LastChar is '\r' or '\n';
        public Indenting Indent() => new Indenting(this);

        public Scoping Scope() => new Scoping(this);

        public abstract void Write(string str);

        public void EnsureNewLine()
        {
            if (!IsNewLine)
                AppendLine();
        }
        protected void AppendPrefix()
        {
            if (!IsNewLine)
                return;
            for (int i=0;i<Indentation;i++)
                Write("    ");
        }
        public void Append(string value)
        {
            AppendPrefix();
            Write(value);
        }
        public abstract void AppendLine();
        public abstract void AppendLine(string value);
        public abstract int Indentation { get; set; }
    }

    public class StreamWriterCodeBuilder : CodeBuilderBase, IDisposable
    {
        private readonly StreamWriter _writer;
        private char? _lastChar;

        public StreamWriterCodeBuilder(StreamWriter writer)
        {
            _writer = writer;
        }

        public override char? LastChar => _lastChar;
        public override void Write(string str)
        {
            if (str.Length != 0)
                _lastChar = str[^1];
            _writer.Write(str);
        }

        public override void AppendLine()
        {
            Write(Environment.NewLine);
        }

        public override void AppendLine(string value)
        {
            AppendPrefix();
            Write(value);
            AppendLine();
        }

        public override int Indentation { get; set; }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
    public class StringCodeBuilder : CodeBuilderBase
    {
        private readonly StringBuilder _builder;

        public StringCodeBuilder() : this(new StringBuilder()) { }
        public StringCodeBuilder(StringBuilder builder)
        {
            _builder = builder;
        }

        public override char? LastChar => (_builder.Length == 0) ? null :  _builder[_builder.Length - 1];


        public override void Write(string str)
        {
            _builder.Append(str);
        }

        public override void AppendLine()
        {
            _builder.AppendLine();
        }
        public override void AppendLine(string value)
        {
            AppendPrefix();
            _builder.AppendLine(value);
        }
        
        public override int Indentation { get; set; }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}