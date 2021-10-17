using System;

namespace engenious.Content.CodeGenerator
{
    /// <summary>
    ///     Modifiers that are generic <see cref="MethodModifiers"/> and <see cref="TypeModifiers"/>.
    /// </summary>
    [Flags]
    public enum GenericModifiers
    {
        /// <summary>
        ///     No modifiers.
        /// </summary>
        None = 0,
        /// <summary>
        ///     The public modifier.
        /// </summary>
        Public = 1,
        /// <summary>
        ///     The private modifier.
        /// </summary>
        Private = 2,
        /// <summary>
        ///     The protected modifier.
        /// </summary>
        Protected = 4,
        /// <summary>
        ///     The internal modifier.
        /// </summary>
        Internal = 8,
        /// <summary>
        ///     The static modifier.
        /// </summary>
        Static = 16,
        /// <summary>
        ///     The unsafe modifier.
        /// </summary>
        Unsafe = 32,
        /// <summary>
        ///     The partial modifier.
        /// </summary>
        Partial = 64
    }

    /// <summary>
    ///     Modifiers that can be applied to types.
    /// </summary>
    [Flags]
    public enum TypeModifiers
    {
        /// <summary>
        ///     No modifiers.
        /// </summary>
        None = 0,
        /// <summary>
        ///     The public modifier.
        /// </summary>
        Public = 1,
        /// <summary>
        ///     The private modifier.
        /// </summary>
        Private = 2,
        /// <summary>
        ///     The protected modifier.
        /// </summary>
        Protected = 4,
        /// <summary>
        ///     The internal modifier.
        /// </summary>
        Internal = 8,
        /// <summary>
        ///     The static modifier.
        /// </summary>
        Static = 16,
        /// <summary>
        ///     The unsafe modifier.
        /// </summary>
        Unsafe = 32,
        /// <summary>
        ///     The partial modifier.
        /// </summary>
        Partial = 64,
        /// <summary>
        ///     The sealed modifier.
        /// </summary>
        Sealed = 128,
        /// <summary>
        ///     The class modifier which marks the type as a class.
        /// </summary>
        Class = 256,
        /// <summary>
        ///     The struct modifier which marks the type as a struct.
        /// </summary>
        Struct = 512
    }

    /// <summary>
    ///     Modifiers that can be applied to methods.
    /// </summary>
    [Flags]
    public enum MethodModifiers
    {
        /// <summary>
        ///     No modifiers.
        /// </summary>
        None = 0,
        /// <summary>
        ///     The public modifier.
        /// </summary>
        Public = 1,
        /// <summary>
        ///     The private modifier.
        /// </summary>
        Private = 2,
        /// <summary>
        ///     The protected modifier.
        /// </summary>
        Protected = 4,
        /// <summary>
        ///     The internal modifier.
        /// </summary>
        Internal = 8,
        /// <summary>
        ///     The static modifier.
        /// </summary>
        Static = 16,
        /// <summary>
        ///     The unsafe modifier.
        /// </summary>
        Unsafe = 32,
        /// <summary>
        ///     The partial modifier.
        /// </summary>
        Partial = 64,
        /// <summary>
        ///     The virtual modifier.
        /// </summary>
        Virtual = 128,
        /// <summary>
        ///     The abstract modifier.
        /// </summary>
        Abstract = 256,
        /// <summary>
        ///     The override modifier.
        /// </summary>
        Override = 512
    }
}