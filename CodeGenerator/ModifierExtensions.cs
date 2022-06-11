using System;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Extensions to write different modifiers to <see cref="ICodeBuilder"/>.
    /// </summary>
    public static class ModifierExtensions
    {
        /// <summary>
        ///     Writes <see cref="TypeModifiers"/> to a <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        /// <param name="modifiers">The <see cref="TypeModifiers"/> to write.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when an invalid modifier combination was passed.
        /// </exception>
        public static void WriteModifiers(this ICodeBuilder builder, TypeModifiers modifiers)
        {
            WriteModifiersPart1(builder, (GenericModifiers)modifiers);
            
            if ((modifiers & TypeModifiers.Sealed) != 0) builder.Append("sealed ");
            
            WriteModifiersPart2(builder, (GenericModifiers)modifiers);

            if ((modifiers & TypeModifiers.Record) != 0)
            {
                builder.Append("record ");
            }
            else if ((modifiers & TypeModifiers.Class) != 0) // leave out class modifier if it is a record, as it is optional
            {
                if ((modifiers & TypeModifiers.Struct) != 0)
                    throw new ArgumentException(
                        "class modifier is incompatible with struct modifiers.");
                builder.Append("class ");
            }
            else if ((modifiers & TypeModifiers.Struct) != 0)
            {
                builder.Append("struct ");
            }
        }

        /// <summary>
        ///     Writes <see cref="MethodModifiers"/> to a <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        /// <param name="modifiers">The <see cref="MethodModifiers"/> to write.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when an invalid modifier combination was passed.
        /// </exception>
        public static void WriteModifiers(this ICodeBuilder builder, MethodModifiers modifiers)
        {
            WriteModifiersPart1(builder, (GenericModifiers)modifiers);

            if ((modifiers & MethodModifiers.Abstract) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Override | MethodModifiers.Virtual)) != 0)
                    throw new ArgumentException(
                        "abstract modifier is incompatible with static, virtual, override modifiers.");
                builder.Append("abstract ");
            }

            if ((modifiers & MethodModifiers.Virtual) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Override | MethodModifiers.Abstract)) != 0)
                    throw new ArgumentException(
                        "virtual modifier is incompatible with static, override, abstract modifiers.");
                builder.Append("virtual ");
            }

            if ((modifiers & MethodModifiers.Override) != 0)
            {
                if ((modifiers & (MethodModifiers.Static | MethodModifiers.Abstract | MethodModifiers.Virtual)) != 0)
                    throw new ArgumentException(
                        "override modifier is incompatible with static, abstract, virtual modifiers.");
                builder.Append("override ");
            }


            WriteModifiersPart2(builder, (GenericModifiers)modifiers);
        }

        /// <summary>
        ///     Writes <see cref="GenericModifiers"/> to a <see cref="ICodeBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICodeBuilder"/> to write to.</param>
        /// <param name="modifiers">The <see cref="GenericModifiers"/> to write.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when an invalid modifier combination was passed.
        /// </exception>
        public static void WriteModifiers(this ICodeBuilder builder, GenericModifiers modifiers)
        {
            WriteModifiersPart1(builder, modifiers);
            WriteModifiersPart2(builder, modifiers);
        }

        private static void WriteModifiersPart1(ICodeBuilder builder, GenericModifiers modifiers)
        {
            if ((modifiers & GenericModifiers.Public) != 0)
            {
                if ((modifiers & (GenericModifiers.Private | GenericModifiers.Protected | GenericModifiers.Internal)) !=
                    0)
                    throw new ArgumentException(
                        "public modifier is incompatible with private, protected, internal modifiers.");
                builder.Append("public ");
            }
            else if ((modifiers & GenericModifiers.Private) != 0)
            {
                if ((modifiers & (GenericModifiers.Public | GenericModifiers.Protected | GenericModifiers.Internal)) !=
                    0)
                    throw new ArgumentException(
                        "private modifier is incompatible with public, protected, internal modifiers.");
                builder.Append("private ");
            }
            else if ((modifiers & GenericModifiers.Protected) != 0)
            {
                if ((modifiers & (GenericModifiers.Public | GenericModifiers.Private)) != 0)
                    throw new ArgumentException(
                        "protected modifier is incompatible with public, private modifiers.");
                builder.Append("protected ");
            }

            if ((modifiers & GenericModifiers.Internal) != 0) builder.Append("internal ");
        }

        private static void WriteModifiersPart2(ICodeBuilder builder, GenericModifiers modifiers)
        {
            if ((modifiers & GenericModifiers.Partial) != 0) builder.Append("partial ");

            if ((modifiers & GenericModifiers.Static) != 0) builder.Append("static ");

            if ((modifiers & GenericModifiers.Unsafe) != 0) builder.Append("unsafe ");
        }
    }
}