using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace engenious.Content.CodeGenerator
{
    [Flags]
    public enum GenericModifiers
    {
        None = 0,
        Public = 1,
        Private = 2,
        Protected = 4,
        Internal = 8,
        Static = 16,
        Unsafe = 32,
        Partial = 64,
    }
    [Flags]
    public enum TypeModifiers
    {
        None = 0,
        Public = 1,
        Private = 2,
        Protected = 4,
        Internal = 8,
        Static = 16,
        Unsafe = 32,
        Partial = 64,
        Class = 128,
        Struct = 256
    }
    [Flags]
    public enum MethodModifiers
    {
        None = 0,
        Public = 1,
        Private = 2,
        Protected = 4,
        Internal = 8,
        Static = 16,
        Unsafe = 32,
        Partial = 64,
        Virtual = 128,
        Abstract = 256,
        Override = 512
    }

    public static class TypeSystem
    {
        public static TypeReference Void = new TypeReference(null, "void");
        public static TypeReference String = new TypeReference(null, "string");
        public static TypeReference Byte = new TypeReference(null, "byte");
        public static TypeReference SByte = new TypeReference(null, "SByte");
        public static TypeReference Int16 = new TypeReference(null, "short");
        public static TypeReference UInt16 = new TypeReference(null, "ushort");
        public static TypeReference Int32 = new TypeReference(null, "int");
        public static TypeReference UInt32 = new TypeReference(null, "uint");
        public static TypeReference Int64 = new TypeReference(null, "long");
        public static TypeReference UInt64 = new TypeReference(null, "ulong");
    }
    public class Helper
    {
        public static void WriteModifiers(ICodeBuilder builder, TypeModifiers modifiers)
        {
            WriteModifiersPart1(builder, (GenericModifiers)modifiers);
            WriteModifiersPart2(builder, (GenericModifiers)modifiers);
            
            if ((modifiers & TypeModifiers.Class) != 0)
            {
                if ((modifiers & (TypeModifiers.Struct)) != 0)
                    throw new NotSupportedException(
                        $"class modifier is incompatible with struct modifiers.");
                builder.Append("class ");
            }
            else if ((modifiers & TypeModifiers.Struct) != 0)
            {
                builder.Append("struct ");
            }
        }
        public static void WriteModifiers(ICodeBuilder builder, MethodModifiers modifiers)
        {
            WriteModifiersPart1(builder, (GenericModifiers)modifiers);
            
            if ((modifiers & MethodModifiers.Abstract) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Override | MethodModifiers.Virtual)) != 0)
                    throw new NotSupportedException(
                        $"abstract modifier is incompatible with static, virtual, override modifiers.");
                builder.Append("abstract ");
            }
            if ((modifiers & MethodModifiers.Virtual) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Override | MethodModifiers.Abstract)) != 0)
                    throw new NotSupportedException(
                        $"virtual modifier is incompatible with static, override, abstract modifiers.");
                builder.Append("virtual ");
            }
            if ((modifiers & MethodModifiers.Override) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Abstract | MethodModifiers.Virtual)) != 0)
                    throw new NotSupportedException(
                        $"override modifier is incompatible with static, abstract, virtual modifiers.");
                builder.Append("override ");
            }
            
            
            WriteModifiersPart2(builder, (GenericModifiers)modifiers);
        }

        public static void WriteModifiers(ICodeBuilder builder, GenericModifiers modifiers)
        {
            WriteModifiersPart1(builder, modifiers);
            WriteModifiersPart2(builder, modifiers);
        }
        private static void WriteModifiersPart1(ICodeBuilder builder, GenericModifiers modifiers)
        {
            if ((modifiers & GenericModifiers.Public) != 0)
            {
                if ((modifiers & (GenericModifiers.Private | GenericModifiers.Protected | GenericModifiers.Internal)) != 0)
                    throw new NotSupportedException(
                        $"public modifier is incompatible with private, protected, internal modifiers.");
                builder.Append("public ");
            }
            else if ((modifiers & GenericModifiers.Private) != 0)
            {
                if ((modifiers & (GenericModifiers.Public | GenericModifiers.Protected | GenericModifiers.Internal)) != 0)
                    throw new NotSupportedException(
                        $"private modifier is incompatible with public, protected, internal modifiers.");
                builder.Append("private ");
            }
            else if ((modifiers & GenericModifiers.Protected) != 0)
            {
                if ((modifiers & (GenericModifiers.Public | GenericModifiers.Private)) != 0)
                    throw new NotSupportedException(
                        $"protected modifier is incompatible with public, private modifiers.");
                builder.Append("protected ");
            }

            if ((modifiers & GenericModifiers.Internal) != 0)
            {
                builder.Append("internal ");
            }
        }

        private static void WriteModifiersPart2(ICodeBuilder builder, GenericModifiers modifiers)
        {
            if ((modifiers & GenericModifiers.Partial) != 0)
            {
                builder.Append("partial ");
            }

            if ((modifiers & GenericModifiers.Static) != 0)
            {
                builder.Append("static ");
            }

            if ((modifiers & GenericModifiers.Unsafe) != 0)
            {
                builder.Append("unsafe ");
            }
        }
    }


    public interface ICode
    {
        void WriteTo(ICodeBuilder builder);
    }
    [Serializable]
    public abstract record CodeDefinition : ICode
    {
        public abstract void WriteTo(ICodeBuilder builder);

        public override string ToString()
        {
            var builder = new StringCodeBuilder();
            WriteTo(builder);
            return builder.ToString();
        }
    }

    [Serializable]
    public record TypeReference(string? Namespace, string Name) : CodeDefinition
    {
        public override string ToString()
        {
            var sb = new StringCodeBuilder();

            WriteReference(sb);
            
            return sb.ToString();
        }

        public override void WriteTo(ICodeBuilder builder)
        {
            WriteReference(builder);
        }

        public void WriteReference(ICodeBuilder builder)
        {
            if (!string.IsNullOrEmpty(Namespace))
            {
                builder.Append(Namespace);
                builder.Append(".");
            }
            builder.Append(Name);
        }
    }

    // public record GeneratedCodeDefinition
    // {
    //     public List<FileDefinition> Files { get; } = new();
    //
    //     private static void CreatePathRecursively(string path)
    //     {
    //         Directory.CreateDirectory(Path.GetDirectoryName(path) ??
    //                                   throw new ArgumentException(
    //                                       "Not a valid file to create the parent directory of"));
    //     }
    //
    //     public void Save(string path)
    //     {
    //         foreach (var f in Files)
    //         {
    //             CreatePathRecursively(f.Name);
    //         }
    //     }
    // }

    [Serializable]
    public record FileDefinition(string Name) : ICode
    {
        public List<string> Usings { get; } = new();
        public Dictionary<string, TypeDefinition> Types { get; } = new();

        public void WriteTo(ICodeBuilder builder)
        {
            foreach (var u in Usings)
            {
                builder.AppendLine("using {u};");
            }

            builder.AppendLine();
            foreach (var n in Types)
            {
                n.Value.WriteTo(builder);
            }
        }
    }
    [Serializable]
    public record TypeDefinition(string Namespace, TypeModifiers Modifiers, string Name, TypeReference[]? BaseTypes) : TypeReference(Namespace, Name)
    {
        public PropertyDefinition AddAutoProperty(MethodModifiers modifiers, TypeReference type, string name, MethodModifiers getterModifiers = MethodModifiers.None, MethodModifiers setterModifiers = MethodModifiers.None)
        {
            var p = new PropertyDefinition(modifiers, type, name, new SimplePropertyGetter(),
                new SimplePropertySetter(), getterModifiers, setterModifiers);
            Properties.Add(p);
            return p;
        }

        public (PropertyDefinition, FieldDefinition) CreateEmptyProperty(MethodModifiers modifiers, TypeReference type, string name, string fieldName, MethodModifiers getterModifiers = MethodModifiers.None, MethodModifiers setterModifiers = MethodModifiers.None)
        {
            var p = new PropertyDefinition(modifiers, type, name, null, null, getterModifiers, setterModifiers);
            var f = new FieldDefinition(GenericModifiers.Private, type, fieldName);
            return (p, f);
        }
        
        public List<MethodDefinition> Methods { get; } = new();
        public List<FieldDefinition> Fields { get; } = new();
        public List<PropertyDefinition> Properties { get; } = new();

        public List<TypeDefinition> NestedTypes { get; } = new();
        public string FullName => Namespace + "." + Name;

        protected void WriteTypeDefinition(ICodeBuilder builder)
        {
            builder.EnsureNewLine();
            Helper.WriteModifiers(builder, Modifiers);
            builder.Append(Name);
            if (BaseTypes is { Length: > 0 })
            {
                builder.Append(" : ");
                for (int i = 0; i < BaseTypes.Length; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    BaseTypes[i].WriteReference(builder);
                }
            }
            using (builder.Scope())
            {
                foreach (var f in Fields)
                {
                    f.WriteTo(builder);
                }

                foreach (var m in Methods)
                {
                    m.WriteTo(builder);
                }

                foreach (var p in Properties)
                {
                    p.WriteTo(builder);
                }

                foreach (var t in NestedTypes)
                {
                    t.WriteTypeDefinition(builder);
                }
            }
        }
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.AppendLine($"namespace {Namespace}");
            using (builder.Scope())
            {
                WriteTypeDefinition(builder);
            }
        }
    }

    [Serializable]
    public record ParameterDefinition(TypeReference Type, string Name) : CodeDefinition
    {
        public override void WriteTo(ICodeBuilder builder)
        {
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
        }
    }

    [Serializable]
    public record SignatureDefinition(MethodModifiers Modifiers, TypeReference ReturnType, string Name,
        ParameterDefinition[] Parameters) : CodeDefinition
    {
        public override void WriteTo(ICodeBuilder builder)
        {
            Helper.WriteModifiers(builder, Modifiers);
            ReturnType.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            builder.Append("(");
            for (int i = 0; i < Parameters.Length; i++)
            {
                Parameters[i].WriteTo(builder);
                if (i != Parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
        }
    }

    [Serializable]
    public abstract record MethodDefinition : CodeDefinition;

    [Serializable]
    public record ConstructorDefinition(TypeReference ParentType, MethodModifiers Modifiers, ParameterDefinition[] Parameters, MethodBodyDefinition MethodBody, CodeExpressionDefinition[]? BaseCalls = null) : MethodDefinition
    {
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.EnsureNewLine();
            Helper.WriteModifiers(builder, Modifiers);
            builder.Append(" ");
            builder.Append(ParentType.Name);
            builder.Append("(");
            for (int i = 0; i < Parameters.Length; i++)
            {
                Parameters[i].WriteTo(builder);
                if (i != Parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.Append(")");
            if (BaseCalls is { Length: > 0 })
            {
                builder.AppendLine();
                using (builder.Indent())
                {
                    builder.Append(": ");
                    for (int i = 0; i < BaseCalls.Length; i++)
                    {
                        if (i != 0)
                            builder.Append(", ");
                        BaseCalls[i].WriteTo(builder);
                    }
                }
                builder.AppendLine();
            }
            MethodBody.WriteTo(builder);
        }
    }
[Serializable]
    public record ImplementedMethodDefinition
        (SignatureDefinition Signature, MethodBodyDefinition? MethodBody) : MethodDefinition
    {
        
        public override void WriteTo(ICodeBuilder builder)
        {
            Signature.WriteTo(builder);
            if (MethodBody == null)
                builder.Append(";");
            else
                MethodBody.WriteTo(builder);
        }
    }

    [Serializable]
    public record MethodBodyDefinition(CodeExpressionDefinition Body) : ICode
    {
        public static MethodBodyDefinition EmptyBody =
            new(new BlockExpressionDefinition(new SimpleExpressionDefinition(string.Empty)));
        public void WriteTo(ICodeBuilder builder)
        {
            bool isBlockExpr = (Body is BlockExpressionDefinition);
            if (!isBlockExpr)
                builder.Append(" => ");

            Body.WriteTo(builder);


            if (!isBlockExpr)
                builder.Append(";");
        }
    }

    [Serializable]
    public record FieldDefinition(GenericModifiers Modifiers, TypeReference Type, string Name) : ICode
    {
        public void WriteTo(ICodeBuilder builder)
        {
            Helper.WriteModifiers(builder, Modifiers);
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            builder.AppendLine(";");
        }
    }

    [Serializable]
    public abstract record PropertyMethodDefinition : MethodDefinition
    {
        private readonly bool _isSetter;
        public PropertyMethodDefinition(bool isSetter)
        {
            _isSetter = isSetter;
        }

        protected abstract void WriteBody(ICodeBuilder builder);
        
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.Append(_isSetter ? "set" : "get");
            WriteBody(builder);
        }
    }

    [Serializable]
    public record ImplementedPropertymethodDefinition : PropertyMethodDefinition
    {
        public MethodBodyDefinition MethodBody { get; }
        public ImplementedPropertymethodDefinition(MethodBodyDefinition methodBody, bool isSetter)
            : base(isSetter)
        {
            MethodBody = methodBody;
        }

        protected override void WriteBody(ICodeBuilder builder)
        {
            MethodBody.WriteTo(builder);
        }
    }

[Serializable]
    public record SimplePropertyGetter : PropertyMethodDefinition
    {
        public SimplePropertyGetter()
            : base(false)
        {
        }

        protected override void WriteBody(ICodeBuilder builder)
        {
            builder.Append(";");
        }
    }

    [Serializable]
    public record SimplePropertySetter : PropertyMethodDefinition
    {
        public SimplePropertySetter()
            : base(true)
        {
        }
        protected override void WriteBody(ICodeBuilder builder)
        {
            builder.Append(";");
        }
    }

    [Serializable]
    public record PropertyDefinition(MethodModifiers Modifiers, TypeReference Type, string Name, PropertyMethodDefinition? GetMethod,
        PropertyMethodDefinition? SetMethod, MethodModifiers GetterModifiers = MethodModifiers.None,
        MethodModifiers SetterModifiers = MethodModifiers.None, TypeReference? IndexerType = null, string? IndexerName = null) : CodeDefinition
    {
        public override void WriteTo(ICodeBuilder builder)
        {
            Helper.WriteModifiers(builder, Modifiers);
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            bool hasIndexer = (IndexerType != null && IndexerName != null);

            if (hasIndexer)
            {
                builder.Append("[");
                IndexerType!.WriteReference(builder);
                builder.Append($" {IndexerName}");
                builder.Append("]");
            }
            
            if (GetMethod is SimplePropertyGetter simpleGetter && SetMethod is SimplePropertySetter simpleSetter)
            {
                builder.Append("{ ");
                Helper.WriteModifiers(builder, GetterModifiers);
                simpleGetter.WriteTo(builder);
                builder.Append(" ");
                Helper.WriteModifiers(builder, SetterModifiers);
                simpleSetter.WriteTo(builder);
                builder.AppendLine(" }");
            }
            else
            {
                builder.AppendLine();

                builder.AppendLine("{");
                builder.Indentation++;
                if (GetMethod != null)
                {
                    Helper.WriteModifiers(builder, GetterModifiers);
                    GetMethod.WriteTo(builder);

                    builder.AppendLine();
                }

                if (SetMethod != null)
                {
                    Helper.WriteModifiers(builder, SetterModifiers);
                    SetMethod.WriteTo(builder);
                    builder.AppendLine();
                }

                builder.Indentation--;
                builder.AppendLine("}");
            }
        }
    }
}