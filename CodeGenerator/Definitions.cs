using System;
using System.Collections.Generic;

namespace engenious.Content.CodeGenerator
{

    /// <summary>
    ///     A collection of the common types.
    /// </summary>
    public static class TypeSystem
    {
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="System.Void"/> or <c>void</c>.
        /// </summary>
        public static readonly TypeReference Void = new(null, "void");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="string"/>.
        /// </summary>
        public static readonly TypeReference String = new(null, "string");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="ulong"/>.
        /// </summary>
        public static readonly TypeReference Byte = new(null, "byte");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="sbyte"/>.
        /// </summary>
        public static readonly TypeReference SByte = new(null, "sbyte");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="short"/>.
        /// </summary>
        public static readonly TypeReference Int16 = new(null, "short");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="ushort"/>.
        /// </summary>
        public static readonly TypeReference UInt16 = new(null, "ushort");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="int"/>.
        /// </summary>
        public static readonly TypeReference Int32 = new(null, "int");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="uint"/>.
        /// </summary>
        public static readonly TypeReference UInt32 = new(null, "uint");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="long"/>.
        /// </summary>
        public static readonly TypeReference Int64 = new(null, "long");
        /// <summary>
        ///     A <see cref="TypeReference"/> to <see cref="ulong"/>.
        /// </summary>
        public static readonly TypeReference UInt64 = new(null, "ulong");
    }


    /// <summary>
    ///     Base class for all code definitions.
    /// </summary>
    [Serializable]
    public abstract record CodeDefinition : ICode
    {
        /// <inheritdoc />
        public abstract void WriteTo(ICodeBuilder builder);

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringCodeBuilder();
            WriteTo(builder);
            return builder.ToString();
        }
    }

    /// <summary>
    ///     Represents a reference to a type.
    /// </summary>
    /// <param name="Namespace">
    ///     The namespace of the type or <c>null</c> for global types as well as nested <see cref="TypeDefinition"/>.
    /// </param>
    /// <param name="Name">The name of the type.</param>
    [Serializable]
    public record TypeReference(string? Namespace, string Name) : CodeDefinition
    {
        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringCodeBuilder();

            WriteReference(sb);

            return sb.ToString();
        }

        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            WriteReference(builder);
        }

        /// <summary>
        /// Writes the <see cref="TypeReference"/> to the <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        public void WriteReference(ICodeBuilder builder)
        {
            if (!string.IsNullOrEmpty(Namespace))
            {
                builder.Append(Namespace!);
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

    /// <summary>
    ///     Represents a file containing multiple types.
    /// </summary>
    /// <param name="Name">The name of the file.</param>
    [Serializable]
    public record FileDefinition(string Name) : ICode
    {
        /// <summary>
        ///     Gets the used namespace imports for this file.
        /// </summary>
        public List<string> Usings { get; } = new();
        /// <summary>
        ///     Gets the types defined in this file.
        /// </summary>
        public Dictionary<string, TypeDefinition> Types { get; } = new();

        /// <inheritdoc />
        public void WriteTo(ICodeBuilder builder)
        {
            foreach (var u in Usings) builder.AppendLine("using {u};");

            builder.AppendLine();
            foreach (var n in Types) n.Value.WriteTo(builder);
        }
    }

    /// <summary>
    ///     Represents a definition for a type.
    /// </summary>
    /// <param name="Namespace">The namespace of the type.</param>
    /// <param name="Modifiers">The <see cref="TypeModifiers"/> for this type.</param>
    /// <param name="Name">The name of this type.</param>
    /// <param name="BaseTypes">The implemented and inherited base types.</param>
    [Serializable]
    public record TypeDefinition
        (string Namespace, TypeModifiers Modifiers, string Name, TypeReference[]? BaseTypes) : TypeReference(Namespace,
            Name)
    {
        /// <summary>
        ///     Gets the methods of this <see cref="TypeDefinition"/>.
        /// </summary>
        public List<MethodDefinition> Methods { get; } = new();
        /// <summary>
        ///     Gets the fields of this <see cref="TypeDefinition"/>.
        /// </summary>
        public List<FieldDefinition> Fields { get; } = new();
        /// <summary>
        ///     Gets the properties of this <see cref="TypeDefinition"/>.
        /// </summary>
        public List<PropertyDefinition> Properties { get; } = new();

        /// <summary>
        ///     Gets the nested types of this <see cref="TypeDefinition"/>.
        /// </summary>
        public List<TypeDefinition> NestedTypes { get; } = new();
        
        /// <summary>
        ///     Gets the full name of this type.
        /// </summary>
        public string FullName => Namespace + "." + Name;

        /// <summary>
        ///     Create an auto property implementing a simple getter and setter method.
        /// </summary>
        /// <param name="modifiers">The parent <see cref="MethodModifiers"/> to use for the property.</param>
        /// <param name="type">The type of the property and field.</param>
        /// <param name="name">The name of the property to create.</param>
        /// <param name="getterModifiers">The <see cref="MethodModifiers"/> for the getter method.</param>
        /// <param name="setterModifiers">The <see cref="MethodModifiers"/> for the setter method.</param>
        /// <returns>The created auto property.</returns>
        public PropertyDefinition AddAutoProperty(MethodModifiers modifiers, TypeReference type, string name,
            MethodModifiers getterModifiers = MethodModifiers.None,
            MethodModifiers setterModifiers = MethodModifiers.None)
        {
            var p = new PropertyDefinition(modifiers, type, name, new SimplePropertyGetter(),
                new SimplePropertySetter(), getterModifiers, setterModifiers);
            Properties.Add(p);
            return p;
        }

        /// <summary>
        ///     Creates a new empty property and a corresponding private field.
        /// </summary>
        /// <param name="modifiers">The parent <see cref="MethodModifiers"/> to use for the property.</param>
        /// <param name="type">The type of the property and field.</param>
        /// <param name="name">The name of the property to create.</param>
        /// <param name="fieldName">The name for the field to create.</param>
        /// <param name="getterModifiers">The <see cref="MethodModifiers"/> for the getter method.</param>
        /// <param name="setterModifiers">The <see cref="MethodModifiers"/> for the setter method.</param>
        /// <returns>A tuple of the created <see cref="PropertyDefinition"/> and <see cref="FieldDefinition"/>.</returns>
        public (PropertyDefinition, FieldDefinition) CreateEmptyProperty(MethodModifiers modifiers, TypeReference type,
            string name, string fieldName, MethodModifiers getterModifiers = MethodModifiers.None,
            MethodModifiers setterModifiers = MethodModifiers.None)
        {
            var p = new PropertyDefinition(modifiers, type, name, null, null, getterModifiers, setterModifiers);
            var f = new FieldDefinition(GenericModifiers.Private, type, fieldName);
            return (p, f);
        }

        /// <summary>
        ///     Writes the complete definition of the type to a <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        protected void WriteTypeDefinition(ICodeBuilder builder)
        {
            builder.EnsureNewLine();
            builder.WriteModifiers(Modifiers);
            builder.Append(Name);
            if (BaseTypes is { Length: > 0 })
            {
                builder.Append(" : ");
                for (var i = 0; i < BaseTypes.Length; i++)
                {
                    if (i != 0)
                        builder.Append(", ");
                    BaseTypes[i].WriteReference(builder);
                }
            }

            using (builder.Scope())
            {
                foreach (var f in Fields) f.WriteTo(builder);

                foreach (var m in Methods) m.WriteTo(builder);

                foreach (var p in Properties) p.WriteTo(builder);

                foreach (var t in NestedTypes) t.WriteTypeDefinition(builder);
            }
        }

        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.AppendLine($"namespace {Namespace}");
            using (builder.Scope())
            {
                WriteTypeDefinition(builder);
            }
        }
    }

    /// <summary>
    ///     Represents a parameter of a <see cref="SignatureDefinition"/>-
    /// </summary>
    /// <param name="Type">The type of the parameter.</param>
    /// <param name="Name">The name of the parameter.</param>
    [Serializable]
    public record ParameterDefinition(TypeReference Type, string Name) : CodeDefinition
    {
        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
        }
    }

    /// <summary>
    ///     Represents the signature of a <see cref="MethodDefinition"/>.
    /// </summary>
    /// <param name="Modifiers">The <see cref="MethodModifiers"/> of the methods signature.</param>
    /// <param name="ReturnType">The return type of the method.</param>
    /// <param name="Name">The name of the method.</param>
    /// <param name="Parameters">The parameters of the method.</param>
    [Serializable]
    public record SignatureDefinition(MethodModifiers Modifiers, TypeReference ReturnType, string Name,
        ParameterDefinition[] Parameters) : CodeDefinition
    {
        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.WriteModifiers(Modifiers);
            ReturnType.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            builder.Append("(");
            for (var i = 0; i < Parameters.Length; i++)
            {
                Parameters[i].WriteTo(builder);
                if (i != Parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
        }
    }

    /// <summary>
    ///     A base class for method definitions.
    /// </summary>
    [Serializable]
    public abstract record MethodDefinition : CodeDefinition;

    /// <summary>
    ///     Represents a constructor method.
    /// </summary>
    /// <param name="ParentType">The type that can be created using this constructor.</param>
    /// <param name="Modifiers">The <see cref="MethodModifiers"/> of the constructor signature.</param>
    /// <param name="Parameters">The parameters of the constructor.</param>
    /// <param name="MethodBody">The body implementation of the constructor.</param>
    /// <param name="BaseCalls">The base constructor call initializers.</param>
    [Serializable]
    public record ConstructorDefinition(TypeReference ParentType, MethodModifiers Modifiers,
        ParameterDefinition[] Parameters, MethodBodyDefinition MethodBody,
        CodeExpressionDefinition[]? BaseCalls = null) : MethodDefinition
    {
        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.EnsureNewLine();
            builder.WriteModifiers(Modifiers);
            builder.Append(" ");
            builder.Append(ParentType.Name);
            builder.Append("(");
            for (var i = 0; i < Parameters.Length; i++)
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
                    for (var i = 0; i < BaseCalls.Length; i++)
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

    /// <summary>
    ///     Represents an implemented method with a body.
    /// </summary>
    /// <param name="Signature">The signature of the method.</param>
    /// <param name="MethodBody">The method body.</param>
    [Serializable]
    public record ImplementedMethodDefinition
        (SignatureDefinition Signature, MethodBodyDefinition? MethodBody) : MethodDefinition
    {
        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            Signature.WriteTo(builder);
            if (MethodBody == null)
                builder.Append(";");
            else
                MethodBody.WriteTo(builder);
        }
    }

    /// <summary>
    ///     Represents the body of a method.
    /// </summary>
    /// <param name="Body">The code expressions of the body.</param>
    [Serializable]
    public record MethodBodyDefinition(CodeExpressionDefinition Body) : ICode
    {
        /// <summary>
        ///     Gets an empty body.
        /// </summary>
        public static MethodBodyDefinition EmptyBody =
            new(new BlockExpressionDefinition(new SimpleExpressionDefinition(string.Empty)));

        /// <inheritdoc />
        public void WriteTo(ICodeBuilder builder)
        {
            var isBlockExpr = Body is BlockExpressionDefinition;
            if (!isBlockExpr)
                builder.Append(" => ");

            Body.WriteTo(builder);


            if (!isBlockExpr)
                builder.Append(";");
        }
    }

    /// <summary>
    ///     Represents a field definition.
    /// </summary>
    /// <param name="Modifiers">The <see cref="GenericModifiers"/> for the field.</param>
    /// <param name="Type">The type of the field.</param>
    /// <param name="Name">The name of the field.</param>
    [Serializable]
    public record FieldDefinition(GenericModifiers Modifiers, TypeReference Type, string Name) : ICode
    {
        /// <inheritdoc />
        public void WriteTo(ICodeBuilder builder)
        {
            builder.WriteModifiers(Modifiers);
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            builder.AppendLine(";");
        }
    }

    /// <summary>
    ///     Base class for property getter and setters.
    /// </summary>
    [Serializable]
    public abstract record PropertyMethodDefinition : MethodDefinition
    {
        private readonly bool _isSetter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyMethodDefinition"/> class.
        /// </summary>
        /// <param name="isSetter">Whether ths method is a setter(<c>true</c>) or a getter(<c>false</c>).</param>
        protected PropertyMethodDefinition(bool isSetter)
        {
            _isSetter = isSetter;
        }

        /// <summary>
        ///     Writes the property method body to the <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        protected abstract void WriteBody(ICodeBuilder builder);

        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.Append(_isSetter ? "set" : "get");
            WriteBody(builder);
        }
    }

    /// <summary>
    ///     Represents an implemented property method with a body.
    /// </summary>
    [Serializable]
    public record ImplementedPropertyMethodDefinition : PropertyMethodDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ImplementedPropertyMethodDefinition"/> class.
        /// </summary>
        /// <param name="methodBody">The body of the method.</param>
        /// <param name="isSetter">Whether ths method is a setter(<c>true</c>) or a getter(<c>false</c>).</param>
        public ImplementedPropertyMethodDefinition(MethodBodyDefinition methodBody, bool isSetter)
            : base(isSetter)
        {
            MethodBody = methodBody;
        }

        /// <summary>
        ///     Gets the method body.
        /// </summary>
        public MethodBodyDefinition MethodBody { get; }

        /// <inheritdoc />
        protected override void WriteBody(ICodeBuilder builder)
        {
            MethodBody.WriteTo(builder);
        }
    }

    /// <summary>
    ///     Represents a simple getter for e.g. an auto property.
    /// </summary>
    [Serializable]
    public record SimplePropertyGetter : PropertyMethodDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SimplePropertyGetter"/> class.
        /// </summary>
        public SimplePropertyGetter()
            : base(false)
        {
        }

        /// <inheritdoc />
        protected override void WriteBody(ICodeBuilder builder)
        {
            builder.Append(";");
        }
    }

    /// <summary>
    ///     Represents a simple setter for e.g. an auto property.
    /// </summary>
    [Serializable]
    public record SimplePropertySetter : PropertyMethodDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SimplePropertySetter"/> class.
        /// </summary>
        public SimplePropertySetter()
            : base(true)
        {
        }

        /// <inheritdoc />
        protected override void WriteBody(ICodeBuilder builder)
        {
            builder.Append(";");
        }
    }

    /// <summary>
    ///     Represents a property definition.
    /// </summary>
    /// <param name="Modifiers">The parent <see cref="MethodModifiers"/> to use for the property.</param>
    /// <param name="Type">The type of the property.</param>
    /// <param name="Name">The name of the property.</param>
    /// <param name="GetMethod">The getter method or <c>null</c> if the property should have no getter.</param>
    /// <param name="SetMethod">The setter method or <c>null</c> if the property should have no setter.</param>
    /// <param name="GetterModifiers">The getter specific <see cref="MethodModifiers"/>.</param>
    /// <param name="SetterModifiers">The setter specific <see cref="MethodModifiers"/>.</param>
    /// <param name="IndexerType">The type for the property indexer;<c>null</c> for properties with no indexer.</param>
    /// <param name="IndexerName">The name for the property indexer;<c>null</c> for properties with no indexer.</param>
    [Serializable]
    public record PropertyDefinition(MethodModifiers Modifiers, TypeReference Type, string Name,
        PropertyMethodDefinition? GetMethod,
        PropertyMethodDefinition? SetMethod, MethodModifiers GetterModifiers = MethodModifiers.None,
        MethodModifiers SetterModifiers = MethodModifiers.None, TypeReference? IndexerType = null,
        string? IndexerName = null) : CodeDefinition
    {
        /// <inheritdoc />
        public override void WriteTo(ICodeBuilder builder)
        {
            builder.WriteModifiers(Modifiers);
            Type.WriteReference(builder);
            builder.Append(" ");
            builder.Append(Name);
            var hasIndexer = IndexerType != null && IndexerName != null;

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
                builder.WriteModifiers(GetterModifiers);
                simpleGetter.WriteTo(builder);
                builder.Append(" ");
                builder.WriteModifiers(SetterModifiers);
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
                    builder.WriteModifiers(GetterModifiers);
                    GetMethod.WriteTo(builder);

                    builder.AppendLine();
                }

                if (SetMethod != null)
                {
                    builder.WriteModifiers(SetterModifiers);
                    SetMethod.WriteTo(builder);
                    builder.AppendLine();
                }

                builder.Indentation--;
                builder.AppendLine("}");
            }
        }
    }
}